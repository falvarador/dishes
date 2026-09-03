using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Recipes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;

namespace dishes.Server.Tests.Features.Recipes;

public class UpdateRecipeWithOwnershipIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private IServiceScope _scope = null!;
    private AppDbContext _context = null!;
    private UserManager<AppIdentityUser> _userManager = null!;

    public UpdateRecipeWithOwnershipIntegrationTests()
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
            .Where(u => u.UserName != null && u.UserName.StartsWith("testuser_update"))
            .ToList();
        foreach (var user in users)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateRecipe_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var request = new CreateRecipeRequest(
            "Updated Title",
            "Updated Description",
            null,
            "30 min",
            "Easy",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/recipes/{recipeId}", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRecipe_ByOwner_Succeeds()
    {
        // Arrange
        var (ownerUserId, ownerToken) = await CreateAndAuthenticateUserAsync("testuser_update_1");
        var recipe = await CreateRecipeAsync(ownerUserId, "Original Title", "Original Description");

        _client.DefaultRequestHeaders.Authorization = new("Bearer", ownerToken);
        var request = new CreateRecipeRequest(
            "Updated Title",
            "Updated Description",
            null,
            "45 min",
            "Medium",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/recipes/{recipe.Id}", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var updated = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("Updated Description", updated.Description);
    }

    [Fact]
    public async Task UpdateRecipe_ByNonOwner_ReturnsForbidden()
    {
        // Arrange
        var (ownerUserId, _) = await CreateAndAuthenticateUserAsync("testuser_update_2");
        var (hackerUserId, hackerToken) = await CreateAndAuthenticateUserAsync("testuser_update_3");

        var recipe = await CreateRecipeAsync(ownerUserId, "Original Title", "Original Description");

        _client.DefaultRequestHeaders.Authorization = new("Bearer", hackerToken);
        var request = new CreateRecipeRequest(
            "Hacked Title",
            "Hacked Description",
            null,
            "30 min",
            "Easy",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/recipes/{recipe.Id}", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);

        // Verify recipe was not modified
        var original = _context.Recipes.FirstOrDefault(r => r.Id == recipe.Id);
        Assert.Equal("Original Title", original?.Title);
    }

    [Fact]
    public async Task UpdateRecipe_WithNonExistentRecipe_ReturnsNotFound()
    {
        // Arrange
        var (_, userToken) = await CreateAndAuthenticateUserAsync("testuser_update_4");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        var nonExistentRecipeId = Guid.NewGuid();
        var request = new CreateRecipeRequest(
            "Updated Title",
            "Updated Description",
            null,
            "30 min",
            "Easy",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/recipes/{nonExistentRecipeId}", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRecipe_OwnerCanUpdateMultipleTimes()
    {
        // Arrange
        var (ownerUserId, ownerToken) = await CreateAndAuthenticateUserAsync("testuser_update_5");
        var recipe = await CreateRecipeAsync(ownerUserId, "Original", "Original Desc");

        _client.DefaultRequestHeaders.Authorization = new("Bearer", ownerToken);

        var request1 = new CreateRecipeRequest(
            "First Update",
            "First Update Desc",
            null,
            "30 min",
            "Easy",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        var request2 = new CreateRecipeRequest(
            "Second Update",
            "Second Update Desc",
            null,
            "45 min",
            "Medium",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response1 = await _client.PutAsJsonAsync($"/api/recipes/{recipe.Id}", request1);
        var updated1 = await response1.Content.ReadFromJsonAsync<RecipeResponse>();

        var response2 = await _client.PutAsJsonAsync($"/api/recipes/{recipe.Id}", request2);
        var updated2 = await response2.Content.ReadFromJsonAsync<RecipeResponse>();

        // Assert
        Assert.True(response1.IsSuccessStatusCode);
        Assert.True(response2.IsSuccessStatusCode);
        Assert.Equal("First Update", updated1.Title);
        Assert.Equal("Second Update", updated2.Title);
    }

    [Fact]
    public async Task UpdateRecipe_PreservesCreatorId()
    {
        // Arrange
        var (ownerUserId, ownerToken) = await CreateAndAuthenticateUserAsync("testuser_update_6");
        var recipe = await CreateRecipeAsync(ownerUserId, "Original", "Original Desc");

        _client.DefaultRequestHeaders.Authorization = new("Bearer", ownerToken);
        var request = new CreateRecipeRequest(
            "Updated",
            "Updated Desc",
            null,
            "30 min",
            "Easy",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/recipes/{recipe.Id}", request);
        var updated = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(ownerUserId, updated.CreatorId);
        Assert.Equal(ownerUserId, recipe.CreatorId);
    }

    [Fact]
    public async Task UpdateRecipe_UpdatedAtChanges()
    {
        // Arrange
        var (ownerUserId, ownerToken) = await CreateAndAuthenticateUserAsync("testuser_update_7");
        var recipe = await CreateRecipeAsync(ownerUserId, "Original", "Original Desc");
        var originalUpdatedAt = recipe.UpdatedAt;

        // Wait a bit to ensure time difference
        await Task.Delay(100);

        _client.DefaultRequestHeaders.Authorization = new("Bearer", ownerToken);
        var request = new CreateRecipeRequest(
            "Updated",
            "Updated Desc",
            null,
            "30 min",
            "Easy",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/recipes/{recipe.Id}", request);
        var updated = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        // Assert
        Assert.NotNull(updated);
        Assert.True(updated.UpdatedAt > originalUpdatedAt);
    }

    private async Task<Recipe> CreateRecipeAsync(Guid creatorId, string title, string description)
    {
        var recipe = new Recipe(
            Guid.NewGuid(),
            title,
            description,
            "30 min",
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

        if (!user.EmailConfirmed)
        {
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, confirmationToken);
        }

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
