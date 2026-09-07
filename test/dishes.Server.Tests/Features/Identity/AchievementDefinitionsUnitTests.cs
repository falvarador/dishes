using dishes.Server.Features.Identity;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class AchievementDefinitionsUnitTests
{
    [Fact]
    public void TopContributor_HasCorrectProperties()
    {
        // Act & Assert
        Assert.Equal(AchievementDefinitions.TopContributorId, AchievementDefinitions.TopContributor.Id);
        Assert.Equal("Top Contributor", AchievementDefinitions.TopContributor.Name);
        Assert.NotEmpty(AchievementDefinitions.TopContributor.Description);
        Assert.NotEmpty(AchievementDefinitions.TopContributor.Icon);
    }

    [Fact]
    public void MasterChef_HasCorrectProperties()
    {
        // Act & Assert
        Assert.Equal(AchievementDefinitions.MasterChefId, AchievementDefinitions.MasterChef.Id);
        Assert.Equal("Master Chef", AchievementDefinitions.MasterChef.Name);
        Assert.NotEmpty(AchievementDefinitions.MasterChef.Description);
        Assert.NotEmpty(AchievementDefinitions.MasterChef.Icon);
    }

    [Fact]
    public void HotStreak_HasCorrectProperties()
    {
        // Act & Assert
        Assert.Equal(AchievementDefinitions.HotStreakId, AchievementDefinitions.HotStreak.Id);
        Assert.Equal("Hot Streak", AchievementDefinitions.HotStreak.Name);
        Assert.NotEmpty(AchievementDefinitions.HotStreak.Description);
        Assert.NotEmpty(AchievementDefinitions.HotStreak.Icon);
    }

    [Fact]
    public void VideoStar_HasCorrectProperties()
    {
        // Act & Assert
        Assert.Equal(AchievementDefinitions.VideoStarId, AchievementDefinitions.VideoStar.Id);
        Assert.Equal("Video Star", AchievementDefinitions.VideoStar.Name);
        Assert.NotEmpty(AchievementDefinitions.VideoStar.Description);
        Assert.NotEmpty(AchievementDefinitions.VideoStar.Icon);
    }

    [Fact]
    public void All_ContainsFourAchievements()
    {
        // Act & Assert
        Assert.Equal(4, AchievementDefinitions.All.Count);
    }

    [Fact]
    public void All_ContainsAllDefinedAchievements()
    {
        // Act & Assert
        Assert.Contains(AchievementDefinitions.TopContributor, AchievementDefinitions.All);
        Assert.Contains(AchievementDefinitions.MasterChef, AchievementDefinitions.All);
        Assert.Contains(AchievementDefinitions.HotStreak, AchievementDefinitions.All);
        Assert.Contains(AchievementDefinitions.VideoStar, AchievementDefinitions.All);
    }

    [Fact]
    public void GetAchievementById_TopContributor_ReturnsCorrectAchievement()
    {
        // Act
        var achievement = AchievementDefinitions.GetAchievementById(AchievementDefinitions.TopContributorId);

        // Assert
        Assert.NotNull(achievement);
        Assert.Equal("Top Contributor", achievement!.Name);
    }

    [Fact]
    public void GetAchievementById_MasterChef_ReturnsCorrectAchievement()
    {
        // Act
        var achievement = AchievementDefinitions.GetAchievementById(AchievementDefinitions.MasterChefId);

        // Assert
        Assert.NotNull(achievement);
        Assert.Equal("Master Chef", achievement!.Name);
    }

    [Fact]
    public void GetAchievementById_HotStreak_ReturnsCorrectAchievement()
    {
        // Act
        var achievement = AchievementDefinitions.GetAchievementById(AchievementDefinitions.HotStreakId);

        // Assert
        Assert.NotNull(achievement);
        Assert.Equal("Hot Streak", achievement!.Name);
    }

    [Fact]
    public void GetAchievementById_VideoStar_ReturnsCorrectAchievement()
    {
        // Act
        var achievement = AchievementDefinitions.GetAchievementById(AchievementDefinitions.VideoStarId);

        // Assert
        Assert.NotNull(achievement);
        Assert.Equal("Video Star", achievement!.Name);
    }

    [Fact]
    public void GetAchievementById_InvalidId_ReturnsNull()
    {
        // Act
        var achievement = AchievementDefinitions.GetAchievementById("NonExistentAchievement");

        // Assert
        Assert.Null(achievement);
    }

    [Fact]
    public void Achievement_CanBeCreated_WithAllProperties()
    {
        // Arrange
        var id = "TestId";
        var name = "Test Achievement";
        var description = "Test description";
        var icon = "http://example.com/icon.png";

        // Act
        var achievement = new Achievement(id, name, description, icon);

        // Assert
        Assert.Equal(id, achievement.Id);
        Assert.Equal(name, achievement.Name);
        Assert.Equal(description, achievement.Description);
        Assert.Equal(icon, achievement.Icon);
    }

    [Theory]
    [InlineData("TopContributor")]
    [InlineData("MasterChef")]
    [InlineData("HotStreak")]
    [InlineData("VideoStar")]
    public void AllAchievementIds_AreConstants(string expectedId)
    {
        // Act
        var achievement = AchievementDefinitions.GetAchievementById(expectedId);

        // Assert
        Assert.NotNull(achievement);
        Assert.Equal(expectedId, achievement!.Id);
    }
}
