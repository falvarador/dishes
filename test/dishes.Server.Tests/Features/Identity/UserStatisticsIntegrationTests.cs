using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity.GetUserStatistics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class UserStatisticsIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private IServiceScope _scope = null!;
    private AppDbContext _context = null!;
    private UserManager<AppIdentityUser> _userManager = null!;

    public UserStatisticsIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _scope.Dispose();
        _factory.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetStatistics_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/user/statistics");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStatistics_WithNoData_ReturnsZeroCounts()
    {
        var (_, token) = await CreateAndAuthenticateUserAsync("stats_zero");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await _client.GetAsync("/api/user/statistics");
        var stats = await response.Content.ReadFromJsonAsync<UserStatisticsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(stats);
        Assert.Equal(0, stats.PublishedRecipes);
        Assert.Equal(0, stats.LikesReceived);
        Assert.Equal(0, stats.Followers);
    }

    [Fact]
    public async Task GetStatistics_CountsPublishedRecipesOnly()
    {
        var (userId, token) = await CreateAndAuthenticateUserAsync("stats_recipes");
        await SeedRecipeAsync(userId, RecipeStatus.Published);
        await SeedRecipeAsync(userId, RecipeStatus.Published);
        await SeedRecipeAsync(userId, RecipeStatus.Published);
        await SeedRecipeAsync(userId, RecipeStatus.Draft);
        await SeedRecipeAsync(userId, RecipeStatus.Draft);

        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        var response = await _client.GetAsync("/api/user/statistics");
        var stats = await response.Content.ReadFromJsonAsync<UserStatisticsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, stats!.PublishedRecipes);
    }

    [Fact]
    public async Task GetStatistics_CountsLikesOnOwnRecipesOnly()
    {
        var (userId, token) = await CreateAndAuthenticateUserAsync("stats_likes_owner");
        var (otherUserId, _) = await CreateAndAuthenticateUserAsync("stats_likes_other");
        var ownRecipe = await SeedRecipeAsync(userId, RecipeStatus.Published);
        var otherRecipe = await SeedRecipeAsync(otherUserId, RecipeStatus.Published);

        _context.UserRecipeFavorites.Add(new UserRecipeFavorite(Guid.NewGuid(), ownRecipe.Id));
        _context.UserRecipeFavorites.Add(new UserRecipeFavorite(Guid.NewGuid(), ownRecipe.Id));
        _context.UserRecipeFavorites.Add(new UserRecipeFavorite(userId, otherRecipe.Id));
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        var response = await _client.GetAsync("/api/user/statistics");
        var stats = await response.Content.ReadFromJsonAsync<UserStatisticsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, stats!.LikesReceived);
    }

    [Fact]
    public async Task GetStatistics_CountsFollowersAndIsolatesUsers()
    {
        var (userAId, tokenA) = await CreateAndAuthenticateUserAsync("stats_iso_a");
        var (userBId, tokenB) = await CreateAndAuthenticateUserAsync("stats_iso_b");
        var (follower1Id, _) = await CreateAndAuthenticateUserAsync("stats_iso_f1");
        var (follower2Id, _) = await CreateAndAuthenticateUserAsync("stats_iso_f2");
        var (follower3Id, _) = await CreateAndAuthenticateUserAsync("stats_iso_f3");
        var (follower4Id, _) = await CreateAndAuthenticateUserAsync("stats_iso_f4");

        await SeedRecipeAsync(userAId, RecipeStatus.Published);
        await SeedRecipeAsync(userAId, RecipeStatus.Published);
        await SeedRecipeAsync(userBId, RecipeStatus.Published);
        await SeedRecipeAsync(userBId, RecipeStatus.Published);
        await SeedRecipeAsync(userBId, RecipeStatus.Published);
        await SeedRecipeAsync(userBId, RecipeStatus.Published);
        await SeedRecipeAsync(userBId, RecipeStatus.Published);

        _context.UserFollows.Add(new UserFollow(follower1Id.ToString(), userAId.ToString()));
        _context.UserFollows.Add(new UserFollow(follower2Id.ToString(), userBId.ToString()));
        _context.UserFollows.Add(new UserFollow(follower3Id.ToString(), userBId.ToString()));
        _context.UserFollows.Add(new UserFollow(follower4Id.ToString(), userBId.ToString()));
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", tokenA);
        var statsA = await (await _client.GetAsync("/api/user/statistics"))
            .Content.ReadFromJsonAsync<UserStatisticsResponse>();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", tokenB);
        var statsB = await (await _client.GetAsync("/api/user/statistics"))
            .Content.ReadFromJsonAsync<UserStatisticsResponse>();

        Assert.Equal(2, statsA!.PublishedRecipes);
        Assert.Equal(1, statsA.Followers);
        Assert.Equal(5, statsB!.PublishedRecipes);
        Assert.Equal(3, statsB.Followers);
    }

    private async Task<Recipe> SeedRecipeAsync(Guid creatorId, RecipeStatus status)
    {
        var recipe = new Recipe(
            Guid.NewGuid(),
            "Stats Recipe",
            "Description",
            "10 mins",
            "Easy",
            creatorId)
        {
            Status = status,
            PublishedAt = status == RecipeStatus.Published ? DateTime.UtcNow : null
        };

        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();
        return recipe;
    }

    private async Task<(Guid UserId, string Token)> CreateAndAuthenticateUserAsync(string username)
    {
        var email = $"{username}@test.com";
        var user = new AppIdentityUser(email)
        {
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, "Password123!");
        if (!createResult.Succeeded)
            throw new InvalidOperationException($"Could not create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

        var loginResponse = await _client.PostAsJsonAsync("/api/user/login", new
        {
            email,
            password = "Password123!"
        });

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(loginContent);
        var token = doc.RootElement.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Access token is null");

        var userIdGuid = Guid.TryParse(user.Id, out var guid) ? guid : Guid.NewGuid();
        return (userIdGuid, token);
    }
}
