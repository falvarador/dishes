using dishes.Server.Features.Identity.GetUserBadges;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class BadgeResponseUnitTests
{
    [Fact]
    public void Constructor_WithValidData_InitializesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Verified Chef";
        var description = "User has been verified as a professional chef";
        var iconUrl = "https://example.com/badge-verified.png";
        var awardedAt = DateTime.UtcNow;

        // Act
        var response = new BadgeResponse
        {
            Id = id,
            Name = name,
            Description = description,
            IconUrl = iconUrl,
            AwardedAt = awardedAt
        };

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal(name, response.Name);
        Assert.Equal(description, response.Description);
        Assert.Equal(iconUrl, response.IconUrl);
        Assert.Equal(awardedAt, response.AwardedAt);
    }

    [Fact]
    public void BadgeResponse_WithDefaultValues_InitializesWithEmptyStrings()
    {
        // Act
        var response = new BadgeResponse();

        // Assert
        Assert.Equal(Guid.Empty, response.Id);
        Assert.Equal(string.Empty, response.Name);
        Assert.Equal(string.Empty, response.Description);
        Assert.Equal(string.Empty, response.IconUrl);
        Assert.Equal(default(DateTime), response.AwardedAt);
    }

    [Fact]
    public void BadgeResponse_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var response = new BadgeResponse();
        var id = Guid.NewGuid();
        var name = "Top Contributor";
        var description = "User has made significant contributions";
        var iconUrl = "https://example.com/badge-top.png";
        var awardedAt = DateTime.UtcNow.AddDays(-10);

        // Act
        response.Id = id;
        response.Name = name;
        response.Description = description;
        response.IconUrl = iconUrl;
        response.AwardedAt = awardedAt;

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal(name, response.Name);
        Assert.Equal(description, response.Description);
        Assert.Equal(iconUrl, response.IconUrl);
        Assert.Equal(awardedAt, response.AwardedAt);
    }

    [Theory]
    [InlineData("Verified Chef")]
    [InlineData("Top Contributor")]
    [InlineData("Michelin Star")]
    [InlineData("Rising Star")]
    public void BadgeResponse_WithDifferentNames_StoresCorrectly(string name)
    {
        // Act
        var response = new BadgeResponse { Name = name };

        // Assert
        Assert.Equal(name, response.Name);
    }

    [Fact]
    public void BadgeResponse_CanBeSerializedToJson()
    {
        // Arrange
        var id = Guid.NewGuid();
        var awardedAt = DateTime.UtcNow;
        var response = new BadgeResponse
        {
            Id = id,
            Name = "Verified Chef",
            Description = "Verified professional chef",
            IconUrl = "https://example.com/badge.png",
            AwardedAt = awardedAt
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(response);

        // Assert
        Assert.Contains("Verified Chef", json);
        Assert.Contains(id.ToString(), json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void BadgeResponse_CanBeDeserializedFromJson()
    {
        // Arrange
        var id = Guid.NewGuid();
        var awardedAt = DateTime.UtcNow;
        var json = $@"{{
            ""id"": ""{id}"",
            ""name"": ""Verified Chef"",
            ""description"": ""Verified professional chef"",
            ""iconUrl"": ""https://example.com/badge.png"",
            ""awardedAt"": ""{awardedAt:O}""
        }}";

        // Act
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var response = System.Text.Json.JsonSerializer.Deserialize<BadgeResponse>(json, options);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal("Verified Chef", response.Name);
        Assert.Equal("Verified professional chef", response.Description);
    }

    [Fact]
    public void MultipleBadgeResponses_CanBeCollected()
    {
        // Arrange
        var badges = new List<BadgeResponse>
        {
            new BadgeResponse { Id = Guid.NewGuid(), Name = "Badge 1", Description = "First badge", IconUrl = "url1", AwardedAt = DateTime.UtcNow },
            new BadgeResponse { Id = Guid.NewGuid(), Name = "Badge 2", Description = "Second badge", IconUrl = "url2", AwardedAt = DateTime.UtcNow },
            new BadgeResponse { Id = Guid.NewGuid(), Name = "Badge 3", Description = "Third badge", IconUrl = "url3", AwardedAt = DateTime.UtcNow }
        };

        // Act
        var distinctCount = badges.Select(b => b.Id).Distinct().Count();

        // Assert
        Assert.Equal(3, badges.Count);
        Assert.Equal(3, distinctCount);
    }
}
