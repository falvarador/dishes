using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class GetUserAchievementsIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private IServiceScope _scope = null!;
    private AppDbContext _context = null!;
    private UserManager<AppIdentityUser> _userManager = null!;

    public GetUserAchievementsIntegrationTests()
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
        var userAchievements = _context.UserAchievements.ToList();
        _context.UserAchievements.RemoveRange(userAchievements);

        var recipes = _context.Recipes
            .Where(r => r.CreatorId != Guid.Empty)
            .ToList();
        _context.Recipes.RemoveRange(recipes);

        var users = _context.Users
            .Where(u => u.UserName != null && u.UserName.StartsWith("testuser_achievement"))
            .ToList();
        foreach (var user in users)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
    }

    private async Task<(Guid userId, string token)> CreateAndAuthenticateUserAsync(string username)
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

    [Fact]
    public async Task GetUserAchievements_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/user/achievements");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserAchievements_UserWithNoAchievements_ReturnsEmptyList()
    {
        // Arrange
        var (_, userToken) = await CreateAndAuthenticateUserAsync("testuser_achievement_1");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/user/achievements");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("[]", content);
    }

    [Fact]
    public async Task GetUserAchievements_TopContributorUnlocked_ReturnsAchievement()
    {
        // Arrange
        var (userId, userToken) = await CreateAndAuthenticateUserAsync("testuser_achievement_2");

        // Create 5 published recipes for TopContributor
        for (int i = 0; i < 5; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: userId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/user/achievements");
        var achievements = await response.Content.ReadFromJsonAsync<List<AchievementResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(achievements);
        Assert.Single(achievements);
        Assert.Equal("TopContributor", achievements[0].Id);
        Assert.Equal("Top Contributor", achievements[0].Name);
    }

    [Fact]
    public async Task GetUserAchievements_MasterChefUnlocked_ReturnsAchievement()
    {
        // Arrange
        var (userId, userToken) = await CreateAndAuthenticateUserAsync("testuser_achievement_3");

        // Create 50 published recipes for MasterChef
        for (int i = 0; i < 50; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: userId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/user/achievements");
        var achievements = await response.Content.ReadFromJsonAsync<List<AchievementResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(achievements);
        Assert.Equal(2, achievements.Count); // TopContributor + MasterChef
        Assert.Contains(achievements, a => a.Id == "MasterChef");
    }

    [Fact]
    public async Task GetUserAchievements_HotStreakUnlocked_ReturnsAchievement()
    {
        // Arrange
        var (userId, userToken) = await CreateAndAuthenticateUserAsync("testuser_achievement_4");

        // Create recipes on 7 consecutive days
        var baseDate = DateTime.UtcNow.Date.AddDays(-6);
        for (int i = 0; i < 7; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe Day {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: userId
            )
            {
                Status = RecipeStatus.Published,
                CreatedAt = baseDate.AddDays(i)
            };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/user/achievements");
        var achievements = await response.Content.ReadFromJsonAsync<List<AchievementResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(achievements);
        Assert.Contains(achievements, a => a.Id == "HotStreak");
    }

    [Fact]
    public async Task GetUserAchievements_VideoStarUnlocked_ReturnsAchievement()
    {
        // Arrange
        var (userId, userToken) = await CreateAndAuthenticateUserAsync("testuser_achievement_5");

        var recipe = new Recipe(
            id: Guid.NewGuid(),
            title: "Video Recipe",
            description: "Description",
            prepTime: "30 min",
            difficulty: "Easy",
            creatorId: userId
        )
        {
            Status = RecipeStatus.Published,
            VideoUrl = "https://example.com/video.mp4"
        };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/user/achievements");
        var achievements = await response.Content.ReadFromJsonAsync<List<AchievementResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(achievements);
        Assert.Contains(achievements, a => a.Id == "VideoStar");
    }

    [Fact]
    public async Task GetUserAchievements_MultipleAchievementsUnlocked_ReturnsAll()
    {
        // Arrange
        var (userId, userToken) = await CreateAndAuthenticateUserAsync("testuser_achievement_6");

        // Create 50 recipes (TopContributor + MasterChef)
        for (int i = 0; i < 50; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: userId
            )
            {
                Status = RecipeStatus.Published,
                VideoUrl = i == 0 ? "https://example.com/video.mp4" : null // VideoStar
            };
            _context.Recipes.Add(recipe);
        }

        // Add 7 consecutive days for HotStreak
        var baseDate = DateTime.UtcNow.Date.AddDays(-6);
        for (int i = 0; i < 7; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Streak Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: userId
            )
            {
                Status = RecipeStatus.Published,
                CreatedAt = baseDate.AddDays(i)
            };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/user/achievements");
        var achievements = await response.Content.ReadFromJsonAsync<List<AchievementResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(achievements);
        Assert.Equal(4, achievements.Count); // All 4 achievements
        Assert.Contains(achievements, a => a.Id == "TopContributor");
        Assert.Contains(achievements, a => a.Id == "MasterChef");
        Assert.Contains(achievements, a => a.Id == "HotStreak");
        Assert.Contains(achievements, a => a.Id == "VideoStar");
    }

    [Fact]
    public async Task GetUserAchievements_MultipleRequests_ImmutableUnlockDate()
    {
        // Arrange
        var (userId, userToken) = await CreateAndAuthenticateUserAsync("testuser_achievement_7");

        // Create 5 recipes for TopContributor
        for (int i = 0; i < 5; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: userId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act - First request
        var response1 = await _client.GetAsync("/api/user/achievements");
        var achievements1 = await response1.Content.ReadFromJsonAsync<List<AchievementResponse>>();
        var firstUnlockedAt = achievements1![0].UnlockedAt;

        // Add another recipe
        var newRecipe = new Recipe(
            id: Guid.NewGuid(),
            title: "New Recipe",
            description: "Description",
            prepTime: "30 min",
            difficulty: "Easy",
            creatorId: userId
        ) { Status = RecipeStatus.Published };
        _context.Recipes.Add(newRecipe);
        await _context.SaveChangesAsync();

        // Small delay to ensure time would be different if it was re-evaluated
        System.Threading.Thread.Sleep(100);

        // Second request
        var response2 = await _client.GetAsync("/api/user/achievements");
        var achievements2 = await response2.Content.ReadFromJsonAsync<List<AchievementResponse>>();
        var secondUnlockedAt = achievements2![0].UnlockedAt;

        // Assert - UnlockedAt should remain the same
        Assert.Equal(firstUnlockedAt, secondUnlockedAt);
    }

    [Fact]
    public async Task GetUserAchievements_TwoUsers_IndependentAchievements()
    {
        // Arrange
        var (user1Id, user1Token) = await CreateAndAuthenticateUserAsync("testuser_achievement_8");
        var (user2Id, user2Token) = await CreateAndAuthenticateUserAsync("testuser_achievement_9");

        // User 1 gets 5 recipes (TopContributor)
        for (int i = 0; i < 5; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"User1 Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: user1Id
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }

        // User 2 gets 0 recipes
        await _context.SaveChangesAsync();

        // Act
        _client.DefaultRequestHeaders.Authorization = new("Bearer", user1Token);
        var response1 = await _client.GetAsync("/api/user/achievements");
        var achievements1 = await response1.Content.ReadFromJsonAsync<List<AchievementResponse>>();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", user2Token);
        var response2 = await _client.GetAsync("/api/user/achievements");
        var achievements2 = await response2.Content.ReadFromJsonAsync<List<AchievementResponse>>();

        // Assert
        Assert.Single(achievements1!);
        Assert.Empty(achievements2!);
    }

    [Fact]
    public async Task GetUserAchievements_ResponseContainsAllRequiredFields()
    {
        // Arrange
        var (userId, userToken) = await CreateAndAuthenticateUserAsync("testuser_achievement_10");

        // Create 5 recipes
        for (int i = 0; i < 5; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: userId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/user/achievements");
        var achievements = await response.Content.ReadFromJsonAsync<List<AchievementResponse>>();

        // Assert
        Assert.NotNull(achievements);
        Assert.Single(achievements);

        var achievement = achievements[0];
        Assert.NotNull(achievement.Id);
        Assert.NotEmpty(achievement.Id);
        Assert.NotNull(achievement.Name);
        Assert.NotEmpty(achievement.Name);
        Assert.NotNull(achievement.Description);
        Assert.NotEmpty(achievement.Description);
        Assert.NotNull(achievement.Icon);
        Assert.NotEmpty(achievement.Icon);
        Assert.NotEqual(DateTime.MinValue, achievement.UnlockedAt);
        Assert.NotEqual(DateTime.MinValue, achievement.CreatedAt);
    }
}
