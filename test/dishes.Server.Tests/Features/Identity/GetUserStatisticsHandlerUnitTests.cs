using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity.GetUserStatistics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class GetUserStatisticsHandlerUnitTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly GetUserStatisticsHandler _handler = new();

    public GetUserStatisticsHandlerUnitTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _context.Recipes.RemoveRange(_context.Recipes);
        _context.UserRecipeFavorites.RemoveRange(_context.UserRecipeFavorites);
        _context.UserFollows.RemoveRange(_context.UserFollows);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task HandleAsync_WithNoData_ReturnsZeroCounts()
    {
        var userId = Guid.NewGuid();

        var result = await _handler.HandleAsync(userId.ToString(), _context, CancellationToken.None);

        var ok = Assert.IsType<Ok<UserStatisticsResponse>>(result);
        Assert.Equal(0, ok.Value!.PublishedRecipes);
        Assert.Equal(0, ok.Value.LikesReceived);
        Assert.Equal(0, ok.Value.Followers);
    }

    [Fact]
    public async Task HandleAsync_WithMixedRecipeStatuses_CountsOnlyPublished()
    {
        var userId = Guid.NewGuid();
        await SeedRecipeAsync(userId, RecipeStatus.Published);
        await SeedRecipeAsync(userId, RecipeStatus.Published);
        await SeedRecipeAsync(userId, RecipeStatus.Published);
        await SeedRecipeAsync(userId, RecipeStatus.Draft);
        await SeedRecipeAsync(userId, RecipeStatus.Draft);
        await SeedRecipeAsync(userId, RecipeStatus.Archived);

        var result = await _handler.HandleAsync(userId.ToString(), _context, CancellationToken.None);

        var ok = Assert.IsType<Ok<UserStatisticsResponse>>(result);
        Assert.Equal(3, ok.Value!.PublishedRecipes);
        Assert.Equal(0, ok.Value.LikesReceived);
        Assert.Equal(0, ok.Value.Followers);
    }

    [Fact]
    public async Task HandleAsync_CountsLikesOnOwnedRecipesOnly()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var ownedRecipe = await SeedRecipeAsync(userId, RecipeStatus.Published);
        var otherRecipe = await SeedRecipeAsync(otherUserId, RecipeStatus.Published);

        _context.UserRecipeFavorites.Add(new UserRecipeFavorite(Guid.NewGuid(), ownedRecipe.Id));
        _context.UserRecipeFavorites.Add(new UserRecipeFavorite(Guid.NewGuid(), ownedRecipe.Id));
        _context.UserRecipeFavorites.Add(new UserRecipeFavorite(userId, otherRecipe.Id));
        await _context.SaveChangesAsync();

        var result = await _handler.HandleAsync(userId.ToString(), _context, CancellationToken.None);

        var ok = Assert.IsType<Ok<UserStatisticsResponse>>(result);
        Assert.Equal(2, ok.Value!.LikesReceived);
    }

    [Fact]
    public async Task HandleAsync_CountsFollowersForCurrentUserOnly()
    {
        var user = await SeedUserAsync("stats-user@test.com");
        var otherUser = await SeedUserAsync("stats-other@test.com");
        var follower1 = await SeedUserAsync("stats-follower1@test.com");
        var follower2 = await SeedUserAsync("stats-follower2@test.com");
        var follower3 = await SeedUserAsync("stats-follower3@test.com");

        _context.UserFollows.Add(new UserFollow(follower1.Id, user.Id));
        _context.UserFollows.Add(new UserFollow(follower2.Id, user.Id));
        _context.UserFollows.Add(new UserFollow(follower3.Id, otherUser.Id));
        await _context.SaveChangesAsync();

        var result = await _handler.HandleAsync(user.Id, _context, CancellationToken.None);

        var ok = Assert.IsType<Ok<UserStatisticsResponse>>(result);
        Assert.Equal(2, ok.Value!.Followers);
    }

    private async Task<AppIdentityUser> SeedUserAsync(string email)
    {
        var user = new AppIdentityUser(email)
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            EmailConfirmed = true,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task HandleAsync_WithInvalidUserId_ReturnsBadRequest()
    {
        var result = await _handler.HandleAsync("not-a-guid", _context, CancellationToken.None);

        Assert.IsType<BadRequest<string>>(result);
    }

    private async Task<Recipe> SeedRecipeAsync(Guid creatorId, RecipeStatus status)
    {
        var recipe = new Recipe(
            Guid.NewGuid(),
            "Test Recipe",
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
}
