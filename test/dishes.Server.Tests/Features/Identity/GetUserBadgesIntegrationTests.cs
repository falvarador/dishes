using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity.GetUserBadges;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class GetUserBadgesIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private AppDbContext _context = null!;
    private UserManager<AppIdentityUser> _userManager = null!;

    public GetUserBadgesIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();

        await CleanupAsync();
    }

    public async Task DisposeAsync()
    {
        await CleanupAsync();
        _factory.Dispose();
    }

    private async Task CleanupAsync()
    {
        var userBadges = _context.UserBadges.ToList();
        _context.UserBadges.RemoveRange(userBadges);

        var badges = _context.Badges.ToList();
        _context.Badges.RemoveRange(badges);

        var users = _context.Users
            .Where(u => u.UserName != null && u.UserName.StartsWith("testuser_badge"))
            .ToList();
        foreach (var user in users)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetUserBadges_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/badges");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserBadges_UserWithNoBadges_ReturnsEmptyList()
    {
        // Arrange
        var (_, userToken) = await CreateAndAuthenticateUserAsync("testuser_badge_1");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/badges");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("[]", content);
    }

    [Fact]
    public async Task GetUserBadges_UserWithOneBadge_ReturnsSingleBadge()
    {
        // Arrange
        var (userIdGuid, userToken) = await CreateAndAuthenticateUserAsync("testuser_badge_2");
        var badge = await CreateBadgeAsync("Verified Chef", "User has been verified as a professional chef", "https://example.com/badge.png");
        await AwardBadgeToUserAsync(userIdGuid.ToString(), badge.Id);

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/badges");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Verified Chef", content);
    }

    [Fact]
    public async Task GetUserBadges_UserWithMultipleBadges_ReturnsAllBadges()
    {
        // Arrange
        var (userIdGuid, userToken) = await CreateAndAuthenticateUserAsync("testuser_badge_3");
        var badge1 = await CreateBadgeAsync("Verified Chef", "Professional verification", "https://example.com/badge1.png");
        var badge2 = await CreateBadgeAsync("Top Contributor", "Significant contributions", "https://example.com/badge2.png");
        var badge3 = await CreateBadgeAsync("Rising Star", "Emerging talent", "https://example.com/badge3.png");

        await AwardBadgeToUserAsync(userIdGuid.ToString(), badge1.Id);
        await AwardBadgeToUserAsync(userIdGuid.ToString(), badge2.Id);
        await AwardBadgeToUserAsync(userIdGuid.ToString(), badge3.Id);

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/badges");
        var badges = await response.Content.ReadFromJsonAsync<List<BadgeResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(3, badges.Count);
        Assert.Contains(badges, b => b.Name == "Verified Chef");
        Assert.Contains(badges, b => b.Name == "Top Contributor");
        Assert.Contains(badges, b => b.Name == "Rising Star");
    }

    [Fact]
    public async Task GetUserBadges_BadgesOrderedByMostRecent()
    {
        // Arrange
        var (userIdGuid, userToken) = await CreateAndAuthenticateUserAsync("testuser_badge_4");
        var now = DateTime.UtcNow;

        var badge1 = await CreateBadgeAsync("Badge 1", "First", "url1");
        var badge2 = await CreateBadgeAsync("Badge 2", "Second", "url2");
        var badge3 = await CreateBadgeAsync("Badge 3", "Third", "url3");

        await AwardBadgeToUserWithDateAsync(userIdGuid.ToString(), badge1.Id, now.AddDays(-10));
        await AwardBadgeToUserWithDateAsync(userIdGuid.ToString(), badge2.Id, now);
        await AwardBadgeToUserWithDateAsync(userIdGuid.ToString(), badge3.Id, now.AddDays(-5));

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/badges");
        var badges = await response.Content.ReadFromJsonAsync<List<BadgeResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(3, badges.Count);
        // Most recent should be first
        Assert.Equal("Badge 2", badges.First().Name);
    }

    [Fact]
    public async Task GetUserBadges_DifferentUsersGetDifferentBadges()
    {
        // Arrange
        var (user1IdGuid, user1Token) = await CreateAndAuthenticateUserAsync("testuser_badge_5");
        var (user2IdGuid, user2Token) = await CreateAndAuthenticateUserAsync("testuser_badge_6");

        var badge1 = await CreateBadgeAsync("User 1 Badge", "For user 1", "url1");
        var badge2 = await CreateBadgeAsync("User 2 Badge", "For user 2", "url2");
        var sharedBadge = await CreateBadgeAsync("Shared Badge", "For both", "url_shared");

        await AwardBadgeToUserAsync(user1IdGuid.ToString(), badge1.Id);
        await AwardBadgeToUserAsync(user1IdGuid.ToString(), sharedBadge.Id);
        await AwardBadgeToUserAsync(user2IdGuid.ToString(), badge2.Id);
        await AwardBadgeToUserAsync(user2IdGuid.ToString(), sharedBadge.Id);

        // Act - Get badges for user 1
        _client.DefaultRequestHeaders.Authorization = new("Bearer", user1Token);
        var response1 = await _client.GetAsync("/api/badges");
        var badges1 = await response1.Content.ReadFromJsonAsync<List<BadgeResponse>>();

        // Act - Get badges for user 2
        _client.DefaultRequestHeaders.Authorization = new("Bearer", user2Token);
        var response2 = await _client.GetAsync("/api/badges");
        var badges2 = await response2.Content.ReadFromJsonAsync<List<BadgeResponse>>();

        // Assert
        Assert.Equal(2, badges1.Count);
        Assert.Equal(2, badges2.Count);
        Assert.Contains(badges1, b => b.Name == "User 1 Badge");
        Assert.Contains(badges2, b => b.Name == "User 2 Badge");
        Assert.DoesNotContain(badges1, b => b.Name == "User 2 Badge");
        Assert.DoesNotContain(badges2, b => b.Name == "User 1 Badge");
    }

    [Fact]
    public async Task GetUserBadges_BadgeContainsAllProperties()
    {
        // Arrange
        var (userIdGuid, userToken) = await CreateAndAuthenticateUserAsync("testuser_badge_7");
        var badgeName = "Verified Chef";
        var badgeDescription = "Professional chef verification";
        var badgeIconUrl = "https://example.com/chef-badge.png";

        var badge = await CreateBadgeAsync(badgeName, badgeDescription, badgeIconUrl);
        await AwardBadgeToUserAsync(userIdGuid.ToString(), badge.Id);

        _client.DefaultRequestHeaders.Authorization = new("Bearer", userToken);

        // Act
        var response = await _client.GetAsync("/api/badges");
        var badges = await response.Content.ReadFromJsonAsync<List<BadgeResponse>>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var returnedBadge = badges.First();
        Assert.NotEqual(Guid.Empty, returnedBadge.Id);
        Assert.Equal(badgeName, returnedBadge.Name);
        Assert.Equal(badgeDescription, returnedBadge.Description);
        Assert.Equal(badgeIconUrl, returnedBadge.IconUrl);
        Assert.NotEqual(default(DateTime), returnedBadge.AwardedAt);
    }

    private async Task<Badge> CreateBadgeAsync(string name, string description, string iconUrl)
    {
        var badge = new Badge
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IconUrl = iconUrl
        };

        _context.Badges.Add(badge);
        await _context.SaveChangesAsync();
        return badge;
    }

    private async Task AwardBadgeToUserAsync(string userId, Guid badgeId)
    {
        var userBadge = new UserBadge
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BadgeId = badgeId,
            AwardedAt = DateTime.UtcNow
        };

        _context.UserBadges.Add(userBadge);
        await _context.SaveChangesAsync();
    }

    private async Task AwardBadgeToUserWithDateAsync(string userId, Guid badgeId, DateTime awardedAt)
    {
        var userBadge = new UserBadge
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BadgeId = badgeId,
            AwardedAt = awardedAt
        };

        _context.UserBadges.Add(userBadge);
        await _context.SaveChangesAsync();
    }

    private async Task<(Guid UserId, string Token)> CreateAndAuthenticateUserAsync(string username)
    {
        var user = new AppIdentityUser(username)
        {
            Email = $"{username}@test.com"
        };

        var createResult = await _userManager.CreateAsync(user, "Password123!");
        if (!createResult.Succeeded)
            throw new InvalidOperationException($"Could not create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

        await _client.PostAsJsonAsync("/register", new
        {
            email = $"{username}@test.com",
            password = "Password123!"
        });

        var loginResponse = await _client.PostAsJsonAsync("/login", new
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
