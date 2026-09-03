using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Recipes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;

namespace dishes.Server.Tests.Features.Recipes;

public class CreateRecipeWithCreatorIdIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private IServiceScope _scope = null!;
    private AppDbContext _context = null!;
    private UserManager<AppIdentityUser> _userManager = null!;

    public CreateRecipeWithCreatorIdIntegrationTests()
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
            .Where(u => u.UserName != null && u.UserName.StartsWith("testuser_create"))
            .ToList();
        foreach (var user in users)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateRecipe_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateRecipeRequest(
            "Test Recipe",
            "Test Description",
            null,
            "30 min",
            "Easy",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/recipes", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRecipe_WithAuthenticatedUser_UsesCorrectCreatorId()
    {
        // Arrange
        var user = await CreateAndAuthenticateUserAsync("testuser_create_1");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", user.Token);

        var request = new CreateRecipeRequest(
            "My Test Recipe",
            "A recipe created by an authenticated user",
            null,
            "45 min",
            "Medium",
            new List<RecipeIngredientDto>
            {
                new("Flour", 2, "cups")
            },
            new List<RecipeInstructionDto>
            {
                new(1, "Mix ingredients")
            }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/recipes", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var createdRecipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(createdRecipe);
        Assert.Equal(user.UserId, createdRecipe.CreatorId);
    }

    [Fact]
    public async Task CreateRecipe_TwoDifferentUsers_CreatesWithDifferentCreatorIds()
    {
        // Arrange
        var user1 = await CreateAndAuthenticateUserAsync("testuser_create_2");
        var user2 = await CreateAndAuthenticateUserAsync("testuser_create_3");

        var request1 = new CreateRecipeRequest(
            "User 1 Recipe",
            "Recipe by user 1",
            null,
            "30 min",
            "Easy",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        var request2 = new CreateRecipeRequest(
            "User 2 Recipe",
            "Recipe by user 2",
            null,
            "40 min",
            "Medium",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        _client.DefaultRequestHeaders.Authorization = new("Bearer", user1.Token);
        var response1 = await _client.PostAsJsonAsync("/api/recipes", request1);
        var recipe1 = await response1.Content.ReadFromJsonAsync<RecipeResponse>();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", user2.Token);
        var response2 = await _client.PostAsJsonAsync("/api/recipes", request2);
        var recipe2 = await response2.Content.ReadFromJsonAsync<RecipeResponse>();

        // Assert
        Assert.NotEqual(recipe1.CreatorId, recipe2.CreatorId);
        Assert.Equal(user1.UserId, recipe1.CreatorId);
        Assert.Equal(user2.UserId, recipe2.CreatorId);
    }

    [Fact]
    public async Task CreateRecipe_WithValidData_SavesRecipeToDatabase()
    {
        // Arrange
        var user = await CreateAndAuthenticateUserAsync("testuser_create_4");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", user.Token);

        var request = new CreateRecipeRequest(
            "Database Test Recipe",
            "Testing that recipe is saved correctly",
            null,
            "50 min",
            "Hard",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/recipes", request);
        var createdRecipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        // Verify in database
        var savedRecipe = _context.Recipes.FirstOrDefault(r => r.Id == createdRecipe.Id);

        // Assert
        Assert.NotNull(savedRecipe);
        Assert.Equal(user.UserId, savedRecipe.CreatorId);
        Assert.Equal("Database Test Recipe", savedRecipe.Title);
    }

    [Fact]
    public async Task CreateRecipe_WithMultipleIngredients_PreservesCreatorId()
    {
        // Arrange
        var user = await CreateAndAuthenticateUserAsync("testuser_create_5");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", user.Token);

        var request = new CreateRecipeRequest(
            "Complex Recipe",
            "Recipe with many ingredients",
            null,
            "2 hours",
            "Hard",
            new List<RecipeIngredientDto>
            {
                new("Flour", 2, "cups"),
                new("Sugar", 1, "cup"),
                new("Butter", 1, "cup")
            },
            new List<RecipeInstructionDto>
            {
                new(1, "Mix flour and sugar"),
                new(2, "Add butter")
            }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/recipes", request);
        var createdRecipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        // Assert
        Assert.NotNull(createdRecipe);
        Assert.Equal(user.UserId, createdRecipe.CreatorId);
        Assert.Equal(3, createdRecipe.Ingredients.Count);
    }

    [Fact]
    public async Task CreateRecipe_CreatorIdIsNotRandom_MatchesAuthenticatedUser()
    {
        // Arrange
        var user = await CreateAndAuthenticateUserAsync("testuser_create_6");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", user.Token);

        var request1 = new CreateRecipeRequest(
            "Recipe 1",
            "First recipe",
            null,
            "30 min",
            "Easy",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        var request2 = new CreateRecipeRequest(
            "Recipe 2",
            "Second recipe",
            null,
            "40 min",
            "Medium",
            new List<RecipeIngredientDto>(),
            new List<RecipeInstructionDto>()
        );

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/recipes", request1);
        var recipe1 = await response1.Content.ReadFromJsonAsync<RecipeResponse>();

        var response2 = await _client.PostAsJsonAsync("/api/recipes", request2);
        var recipe2 = await response2.Content.ReadFromJsonAsync<RecipeResponse>();

        // Assert - Both recipes should have the same CreatorId (the authenticated user)
        Assert.Equal(recipe1.CreatorId, recipe2.CreatorId);
        Assert.Equal(user.UserId, recipe1.CreatorId);
        Assert.Equal(user.UserId, recipe2.CreatorId);
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
