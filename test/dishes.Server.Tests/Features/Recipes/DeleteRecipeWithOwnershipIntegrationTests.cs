using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Recipes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;

namespace dishes.Server.Tests.Features.Recipes;

public class DeleteRecipeWithOwnershipIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private IServiceScope _scope = null!;
    private AppDbContext _context = null!;
    private UserManager<AppIdentityUser> _userManager = null!;

    public DeleteRecipeWithOwnershipIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();

        await CleanupAsync();
    }

    public async Task DisposeAsync()
    {
        await CleanupAsync();
        _scope.Dispose();
        _factory.Dispose();
    }

    private async Task CleanupAsync()
    {
        var recipes = _context.Recipes.ToList();
        _context.Recipes.RemoveRange(recipes);

        var users = _context.Users
            .Where(u => u.UserName != null && u.UserName.StartsWith("testuser_delete"))
            .ToList();
        foreach (var user in users)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteRecipe_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var recipeId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/recipes/{recipeId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRecipe_ByOwner_Succeeds()
    {
        // Arrange
        var (ownerUserId, ownerToken) = await CreateAndAuthenticateUserAsync("testuser_delete_1");
        var recipe = await CreateRecipeAsync(ownerUserId, "Recipe to Delete");

        _client.DefaultRequestHeaders.Authorization = new("Bearer", ownerToken);

        // Act
        var response = await _client.DeleteAsync($"/api/recipes/{recipe.Id}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        // Verify recipe is actually deleted
        var getResponse = await _client.GetAsync($"/api/recipes/{recipe.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteRecipe_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var (ownerUserId, _) = await CreateAndAuthenticateUserAsync("testuser_delete_2");
        var (hackerUserId, hackerToken) = await CreateAndAuthenticateUserAsync("testuser_delete_3");

        var recipe = await CreateRecipeAsync(ownerUserId, "Secret Recipe");

        _client.DefaultRequestHeaders.Authorization = new("Bearer", hackerToken);

        // Act
        var response = await _client.DeleteAsync($"/api/recipes/{recipe.Id}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);

        // Verify recipe was not deleted
        var stillExists = _context.Recipes.FirstOrDefault(r => r.Id == recipe.Id);
        Assert.NotNull(stillExists);
        Assert.Equal("Secret Recipe", stillExists.Title);
    }

    [Fact]
    public async Task DeleteRecipe_WithNonExistentRecipe_ReturnsNotFound()
    {
        // Arrange
        var (_, userToken) = await CreateAndAuthenticateUserAsync("testuser_delete_4");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        var nonExistentRecipeId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/recipes/{nonExistentRecipeId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRecipe_OwnerCanDeleteMultipleTimes()
    {
        // Arrange
        var (ownerUserId, ownerToken) = await CreateAndAuthenticateUserAsync("testuser_delete_5");
        var recipe1 = await CreateRecipeAsync(ownerUserId, "Recipe 1");
        var recipe2 = await CreateRecipeAsync(ownerUserId, "Recipe 2");

        _client.DefaultRequestHeaders.Authorization = new("Bearer", ownerToken);

        // Act
        var response1 = await _client.DeleteAsync($"/api/recipes/{recipe1.Id}");
        var response2 = await _client.DeleteAsync($"/api/recipes/{recipe2.Id}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response1.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response2.StatusCode);

        // Verify both are deleted
        var still1 = _context.Recipes.FirstOrDefault(r => r.Id == recipe1.Id);
        var still2 = _context.Recipes.FirstOrDefault(r => r.Id == recipe2.Id);
        Assert.Null(still1);
        Assert.Null(still2);
    }

    private async Task<Recipe> CreateRecipeAsync(Guid creatorId, string title = "Test Recipe")
    {
        var recipe = new Recipe(
            Guid.NewGuid(),
            title,
            "Test Description",
            "30 mins",
            "Easy",
            creatorId
        );
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
            email = $"{username}@test.com",
            password = "Password123!"
        });

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var token = ExtractTokenFromResponse(loginContent);

        // Convert string UserId to Guid - AppIdentityUser.Id is string
        var userIdGuid = Guid.TryParse(user.Id, out var guid) ? guid : Guid.NewGuid();
        return (userIdGuid, token);
    }

    private static string ExtractTokenFromResponse(string jsonContent)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(jsonContent);
        if (doc.RootElement.TryGetProperty("accessToken", out var accessToken))
            return accessToken.GetString() ?? throw new InvalidOperationException("Access token is null");

        throw new InvalidOperationException("Could not extract access token from response");
    }
}
