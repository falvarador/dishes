using dishes.Server.Features.Identity;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class AchievementResponseUnitTests
{
    [Fact]
    public void AchievementResponse_CanBeCreated_WithAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var response = new AchievementResponse
        {
            Id = "TopContributor",
            Name = "Top Contributor",
            Description = "Published 5+ recipes",
            Icon = "https://example.com/icon.png",
            UnlockedAt = now,
            CreatedAt = now
        };

        // Assert
        Assert.Equal("TopContributor", response.Id);
        Assert.Equal("Top Contributor", response.Name);
        Assert.Equal("Published 5+ recipes", response.Description);
        Assert.Equal("https://example.com/icon.png", response.Icon);
        Assert.Equal(now, response.UnlockedAt);
        Assert.Equal(now, response.CreatedAt);
    }

    [Fact]
    public void AchievementResponse_DefaultConstructor_InitializesEmptyStrings()
    {
        // Act
        var response = new AchievementResponse();

        // Assert
        Assert.Equal(string.Empty, response.Id);
        Assert.Equal(string.Empty, response.Name);
        Assert.Equal(string.Empty, response.Description);
        Assert.Equal(string.Empty, response.Icon);
    }

    [Fact]
    public void AchievementResponse_CanSetProperties_Individually()
    {
        // Arrange
        var response = new AchievementResponse();
        var now = DateTime.UtcNow;

        // Act
        response.Id = "MasterChef";
        response.Name = "Master Chef";
        response.Description = "Published 50+ recipes";
        response.Icon = "https://example.com/master-chef.png";
        response.UnlockedAt = now;
        response.CreatedAt = now;

        // Assert
        Assert.Equal("MasterChef", response.Id);
        Assert.Equal("Master Chef", response.Name);
        Assert.Equal("Published 50+ recipes", response.Description);
        Assert.Equal("https://example.com/master-chef.png", response.Icon);
        Assert.Equal(now, response.UnlockedAt);
        Assert.Equal(now, response.CreatedAt);
    }

    [Fact]
    public void AchievementResponse_UnlockedAtAndCreatedAt_CanBeDifferent()
    {
        // Arrange
        var unlockedAt = DateTime.UtcNow.AddDays(-5);
        var createdAt = DateTime.UtcNow;

        var response = new AchievementResponse
        {
            UnlockedAt = unlockedAt,
            CreatedAt = createdAt
        };

        // Assert
        Assert.NotEqual(unlockedAt, createdAt);
        Assert.Equal(unlockedAt, response.UnlockedAt);
        Assert.Equal(createdAt, response.CreatedAt);
    }

    [Theory]
    [InlineData("TopContributor", "Top Contributor")]
    [InlineData("MasterChef", "Master Chef")]
    [InlineData("HotStreak", "Hot Streak")]
    [InlineData("VideoStar", "Video Star")]
    public void AchievementResponse_MapsAllAchievementTypes(string id, string name)
    {
        // Arrange & Act
        var response = new AchievementResponse
        {
            Id = id,
            Name = name,
            Description = "Test description",
            Icon = "https://example.com/test.png",
            UnlockedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal(name, response.Name);
    }
}
