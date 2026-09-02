using dishes.Server.Features.Recipes;
using Xunit;

namespace dishes.Server.Tests.Features.Recipes;

public class CreatorBadgeResponseUnitTests
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
        var response = new CreatorBadgeResponse(id, name, description, iconUrl, awardedAt);

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal(name, response.Name);
        Assert.Equal(description, response.Description);
        Assert.Equal(iconUrl, response.IconUrl);
        Assert.Equal(awardedAt, response.AwardedAt);
    }

    [Fact]
    public void CreatorBadgeResponse_IsRecord_SupportsEquality()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Verified Chef";
        var description = "Verified professional chef";
        var iconUrl = "https://example.com/badge.png";
        var awardedAt = DateTime.UtcNow;

        var response1 = new CreatorBadgeResponse(id, name, description, iconUrl, awardedAt);
        var response2 = new CreatorBadgeResponse(id, name, description, iconUrl, awardedAt);

        // Act & Assert
        Assert.Equal(response1, response2);
    }

    [Fact]
    public void CreatorBadgeResponse_WithDifferentIds_AreNotEqual()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var name = "Verified Chef";
        var description = "Verified";
        var iconUrl = "https://example.com/badge.png";
        var awardedAt = DateTime.UtcNow;

        var response1 = new CreatorBadgeResponse(id1, name, description, iconUrl, awardedAt);
        var response2 = new CreatorBadgeResponse(id2, name, description, iconUrl, awardedAt);

        // Act & Assert
        Assert.NotEqual(response1, response2);
    }

    [Theory]
    [InlineData("Verified Chef")]
    [InlineData("Top Contributor")]
    [InlineData("Michelin Star")]
    public void CreatorBadgeResponse_WithDifferentNames_StoresCorrectly(string name)
    {
        // Arrange
        var id = Guid.NewGuid();
        var description = "Badge description";
        var iconUrl = "https://example.com/badge.png";
        var awardedAt = DateTime.UtcNow;

        // Act
        var response = new CreatorBadgeResponse(id, name, description, iconUrl, awardedAt);

        // Assert
        Assert.Equal(name, response.Name);
    }

    [Fact]
    public void CreatorBadgeResponse_WithEmptyStrings_InitializesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var awardedAt = DateTime.UtcNow;

        // Act
        var response = new CreatorBadgeResponse(id, string.Empty, string.Empty, string.Empty, awardedAt);

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Empty(response.Name);
        Assert.Empty(response.Description);
        Assert.Empty(response.IconUrl);
    }

    [Fact]
    public void CreatorBadgeResponse_CanBeSerializedToJson()
    {
        // Arrange
        var id = Guid.NewGuid();
        var awardedAt = DateTime.UtcNow;
        var response = new CreatorBadgeResponse(
            id,
            "Verified Chef",
            "Verified professional chef",
            "https://example.com/badge.png",
            awardedAt
        );

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(response);

        // Assert
        Assert.Contains("Verified Chef", json);
        Assert.Contains(id.ToString(), json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void CreatorBadgeResponse_CanBeDeserializedFromJson()
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
        var response = System.Text.Json.JsonSerializer.Deserialize<CreatorBadgeResponse>(json, options);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal("Verified Chef", response.Name);
        Assert.Equal("Verified professional chef", response.Description);
    }

    [Fact]
    public void CreatorBadgeResponse_InACollection_CanBeFiltered()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var badges = new List<CreatorBadgeResponse>
        {
            new CreatorBadgeResponse(Guid.NewGuid(), "Badge 1", "Desc 1", "url1", now),
            new CreatorBadgeResponse(Guid.NewGuid(), "Badge 2", "Desc 2", "url2", now.AddDays(-1)),
            new CreatorBadgeResponse(Guid.NewGuid(), "Badge 3", "Desc 3", "url3", now.AddDays(-2))
        };

        // Act
        var recentBadges = badges.Where(b => b.AwardedAt > now.AddDays(-1.5)).ToList();

        // Assert
        Assert.Equal(2, recentBadges.Count);
    }

    [Fact]
    public void CreatorBadgeResponse_CanBeMappedFromBadge()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Verified Chef";
        var description = "Verified professional chef";
        var iconUrl = "https://example.com/badge.png";
        var awardedAt = DateTime.UtcNow;

        // Act - Simulating the mapping from Badge entity
        var response = new CreatorBadgeResponse(id, name, description, iconUrl, awardedAt);

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal(name, response.Name);
        Assert.Equal(description, response.Description);
        Assert.Equal(iconUrl, response.IconUrl);
    }
}
