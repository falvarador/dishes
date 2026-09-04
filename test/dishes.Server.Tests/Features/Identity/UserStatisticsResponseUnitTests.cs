using dishes.Server.Features.Identity.GetUserStatistics;
using System.Text.Json;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class UserStatisticsResponseUnitTests
{
    [Fact]
    public void Constructor_WithValidCounts_InitializesCorrectly()
    {
        var response = new UserStatisticsResponse(3, 5, 4);

        Assert.Equal(3, response.PublishedRecipes);
        Assert.Equal(5, response.LikesReceived);
        Assert.Equal(4, response.Followers);
    }

    [Fact]
    public void Constructor_WithZeroCounts_InitializesCorrectly()
    {
        var response = new UserStatisticsResponse(0, 0, 0);

        Assert.Equal(0, response.PublishedRecipes);
        Assert.Equal(0, response.LikesReceived);
        Assert.Equal(0, response.Followers);
    }

    [Fact]
    public void UserStatisticsResponse_CanBeSerializedToJson()
    {
        var response = new UserStatisticsResponse(2, 7, 1);

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Contains("publishedRecipes", json);
        Assert.Contains("likesReceived", json);
        Assert.Contains("followers", json);
        Assert.Contains("2", json);
        Assert.Contains("7", json);
    }

    [Fact]
    public void UserStatisticsResponse_CanBeDeserializedFromJson()
    {
        const string json = """
            {
                "publishedRecipes": 8,
                "likesReceived": 12,
                "followers": 3
            }
            """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var response = JsonSerializer.Deserialize<UserStatisticsResponse>(json, options);

        Assert.NotNull(response);
        Assert.Equal(8, response.PublishedRecipes);
        Assert.Equal(12, response.LikesReceived);
        Assert.Equal(3, response.Followers);
    }
}
