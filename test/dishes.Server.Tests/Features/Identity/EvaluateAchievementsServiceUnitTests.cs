using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class EvaluateAchievementsServiceUnitTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection;
    private AppDbContext _context = null!;
    private EvaluateAchievementsService _service = null!;
    private readonly Guid _testUserId = Guid.NewGuid();

    public EvaluateAchievementsServiceUnitTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
    }

    public async Task InitializeAsync()
    {
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _service = new EvaluateAchievementsService(_context);

        // Create test user
        var user = new AppIdentityUser { Id = _testUserId.ToString(), UserName = "testuser" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        _context.Dispose();
        _connection.Dispose();
    }

    // Top Contributor Tests (5+ recipes)
    [Fact]
    public async Task EvaluateAssignments_TopContributorUnlockedAt5Recipes()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert
        Assert.Single(achievements);
        Assert.Equal(AchievementDefinitions.TopContributorId, achievements[0].AchievementId);
    }

    [Fact]
    public async Task EvaluateAssignments_TopContributorNotUnlockedBelow5()
    {
        // Arrange
        for (int i = 0; i < 4; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert
        Assert.Empty(achievements);
    }

    [Fact]
    public async Task EvaluateAssignments_TopContributorIgnoresDraftRecipes()
    {
        // Arrange
        for (int i = 0; i < 3; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Published {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }

        for (int i = 0; i < 2; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Draft {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            ) { Status = RecipeStatus.Draft };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert (only 3 published, not 5)
        Assert.Empty(achievements);
    }

    // Master Chef Tests (50+ recipes)
    [Fact]
    public async Task EvaluateAssignments_MasterChefUnlockedAt50Recipes()
    {
        // Arrange
        for (int i = 0; i < 50; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert
        Assert.Equal(2, achievements.Count); // TopContributor + MasterChef
        Assert.Contains(achievements, a => a.AchievementId == AchievementDefinitions.MasterChefId);
    }

    [Fact]
    public async Task EvaluateAssignments_MasterChefNotUnlockedBelow50()
    {
        // Arrange
        for (int i = 0; i < 49; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert (TopContributor should unlock but not MasterChef)
        Assert.Single(achievements);
        Assert.Equal(AchievementDefinitions.TopContributorId, achievements[0].AchievementId);
    }

    // Hot Streak Tests (7 consecutive days)
    [Fact]
    public async Task EvaluateAssignments_HotStreakUnlockedWith7ConsecutiveDays()
    {
        // Arrange - create recipes on 7 consecutive days
        var baseDate = DateTime.UtcNow.Date;
        for (int i = 0; i < 7; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe Day {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            ) 
            { 
                Status = RecipeStatus.Published,
                CreatedAt = baseDate.AddDays(i)
            };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert
        Assert.Contains(achievements, a => a.AchievementId == AchievementDefinitions.HotStreakId);
    }

    [Fact]
    public async Task EvaluateAssignments_HotStreakNotUnlockedWith6ConsecutiveDays()
    {
        // Arrange - create recipes on 6 consecutive days (one short)
        var baseDate = DateTime.UtcNow.Date;
        for (int i = 0; i < 6; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe Day {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            )
            {
                Status = RecipeStatus.Published,
                CreatedAt = baseDate.AddDays(i)
            };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert
        Assert.DoesNotContain(achievements, a => a.AchievementId == AchievementDefinitions.HotStreakId);
    }

    [Fact]
    public async Task EvaluateAssignments_HotStreakMultipleRecipesSameDay()
    {
        // Arrange - create 7 different recipes on consecutive days, but sometimes multiple per day
        var baseDate = DateTime.UtcNow.Date;
        for (int day = 0; day < 7; day++)
        {
            // 2 recipes on each day
            for (int recipeNum = 0; recipeNum < 2; recipeNum++)
            {
                var recipe = new Recipe(
                    id: Guid.NewGuid(),
                    title: $"Recipe Day {day} Num {recipeNum}",
                    description: "Description",
                    prepTime: "30 min",
                    difficulty: "Easy",
                    creatorId: _testUserId
                )
                {
                    Status = RecipeStatus.Published,
                    CreatedAt = baseDate.AddDays(day)
                };
                _context.Recipes.Add(recipe);
            }
        }
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert - should unlock because there are recipes on 7 consecutive days
        Assert.Contains(achievements, a => a.AchievementId == AchievementDefinitions.HotStreakId);
    }

    [Fact]
    public async Task EvaluateAssignments_HotStreakBrokenByMissingDay()
    {
        // Arrange - 3 days, gap, 3 days (total 6 but streak broken)
        var baseDate = DateTime.UtcNow.Date;

        // First 3 days
        for (int i = 0; i < 3; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe Day {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            )
            {
                Status = RecipeStatus.Published,
                CreatedAt = baseDate.AddDays(i)
            };
            _context.Recipes.Add(recipe);
        }

        // Gap on day 3

        // Next 3 days (starting from day 4)
        for (int i = 4; i < 7; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe Day {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            )
            {
                Status = RecipeStatus.Published,
                CreatedAt = baseDate.AddDays(i)
            };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert - no Hot Streak because it's broken
        Assert.DoesNotContain(achievements, a => a.AchievementId == AchievementDefinitions.HotStreakId);
    }

    // Video Star Tests
    [Fact]
    public async Task EvaluateAssignments_VideoStarUnlockedWithVideoUrl()
    {
        // Arrange
        var recipe = new Recipe(
            id: Guid.NewGuid(),
            title: "Video Recipe",
            description: "Description",
            prepTime: "30 min",
            difficulty: "Easy",
            creatorId: _testUserId
        )
        {
            Status = RecipeStatus.Published,
            VideoUrl = "https://example.com/video.mp4"
        };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert
        Assert.Contains(achievements, a => a.AchievementId == AchievementDefinitions.VideoStarId);
    }

    [Fact]
    public async Task EvaluateAssignments_VideoStarNotUnlockedWithoutVideo()
    {
        // Arrange
        var recipe = new Recipe(
            id: Guid.NewGuid(),
            title: "No Video Recipe",
            description: "Description",
            prepTime: "30 min",
            difficulty: "Easy",
            creatorId: _testUserId
        )
        {
            Status = RecipeStatus.Published,
            VideoUrl = null
        };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert
        Assert.Empty(achievements);
    }

    [Fact]
    public async Task EvaluateAssignments_VideoStarNotUnlockedWithEmptyVideoUrl()
    {
        // Arrange
        var recipe = new Recipe(
            id: Guid.NewGuid(),
            title: "Empty Video Recipe",
            description: "Description",
            prepTime: "30 min",
            difficulty: "Easy",
            creatorId: _testUserId
        )
        {
            Status = RecipeStatus.Published,
            VideoUrl = ""
        };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // Act
        var achievements = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert
        Assert.Empty(achievements);
    }

    // Immutability Tests
    [Fact]
    public async Task EvaluateAssignments_AchievementUnlockedOnce_NeverReevaluated()
    {
        // Arrange - first call should unlock
        for (int i = 0; i < 5; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }
        await _context.SaveChangesAsync();

        var firstCall = await _service.EvaluateAndPersistAsync(_testUserId);
        var firstUnlockedAt = firstCall[0].UnlockedAt;

        // Add another recipe
        var newRecipe = new Recipe(
            id: Guid.NewGuid(),
            title: "New Recipe",
            description: "Description",
            prepTime: "30 min",
            difficulty: "Easy",
            creatorId: _testUserId
        ) { Status = RecipeStatus.Published };
        _context.Recipes.Add(newRecipe);
        await _context.SaveChangesAsync();

        // Act - second call should not change unlock date
        System.Threading.Thread.Sleep(100); // Small delay to ensure time difference
        var secondCall = await _service.EvaluateAndPersistAsync(_testUserId);

        // Assert
        Assert.Single(secondCall);
        Assert.Equal(firstUnlockedAt, secondCall[0].UnlockedAt);
    }

    // Isolation Tests
    [Fact]
    public async Task EvaluateAssignments_MultipleUsers_IndependentAchievements()
    {
        // Arrange - second user
        var user2Id = Guid.NewGuid();
        var user2 = new AppIdentityUser { Id = user2Id.ToString(), UserName = "testuser2" };
        _context.Users.Add(user2);

        // User 1 gets 5 recipes
        for (int i = 0; i < 5; i++)
        {
            var recipe = new Recipe(
                id: Guid.NewGuid(),
                title: $"User1 Recipe {i}",
                description: "Description",
                prepTime: "30 min",
                difficulty: "Easy",
                creatorId: _testUserId
            ) { Status = RecipeStatus.Published };
            _context.Recipes.Add(recipe);
        }

        // User 2 gets 0 recipes
        await _context.SaveChangesAsync();

        // Act
        var user1Achievements = await _service.EvaluateAndPersistAsync(_testUserId);
        var user2Achievements = await _service.EvaluateAndPersistAsync(user2Id);

        // Assert
        Assert.Single(user1Achievements);
        Assert.Empty(user2Achievements);
    }
}
