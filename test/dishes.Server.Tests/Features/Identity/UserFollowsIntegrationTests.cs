using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity.FollowUser;
using dishes.Server.Features.Identity.GetUserStatistics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class UserFollowsIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private IServiceScope _scope = null!;
    private AppDbContext _context = null!;
    private UserManager<AppIdentityUser> _userManager = null!;

    public UserFollowsIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<AppIdentityUser>>();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _scope.Dispose();
        _factory.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task FollowUser_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync($"/api/user/follows/{Guid.NewGuid()}", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnfollowUser_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.DeleteAsync($"/api/user/follows/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FollowUser_SelfFollow_ReturnsBadRequest()
    {
        var (userId, token) = await CreateAndAuthenticateUserAsync("follow_self");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await _client.PostAsync($"/api/user/follows/{userId}", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FollowUser_MissingUser_ReturnsNotFound()
    {
        var (_, token) = await CreateAndAuthenticateUserAsync("follow_missing");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await _client.PostAsync($"/api/user/follows/{Guid.NewGuid()}", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FollowUser_ValidFollow_ReturnsCreated()
    {
        var (_, followerToken) = await CreateAndAuthenticateUserAsync("follow_ok_a");
        var (followedId, _) = await CreateAndAuthenticateUserAsync("follow_ok_b");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", followerToken);

        var response = await _client.PostAsync($"/api/user/follows/{followedId}", null);
        var body = await response.Content.ReadFromJsonAsync<FollowUserResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(followedId.ToString(), body!.FollowedUserId);
    }

    [Fact]
    public async Task FollowUser_DuplicateFollow_ReturnsConflict()
    {
        var (_, followerToken) = await CreateAndAuthenticateUserAsync("follow_dup_a");
        var (followedId, _) = await CreateAndAuthenticateUserAsync("follow_dup_b");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", followerToken);

        var first = await _client.PostAsync($"/api/user/follows/{followedId}", null);
        var second = await _client.PostAsync($"/api/user/follows/{followedId}", null);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task UnfollowUser_MissingRelationship_ReturnsNotFound()
    {
        var (_, token) = await CreateAndAuthenticateUserAsync("unfollow_missing");
        var (otherId, _) = await CreateAndAuthenticateUserAsync("unfollow_missing_other");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var response = await _client.DeleteAsync($"/api/user/follows/{otherId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnfollowUser_ExistingRelationship_ReturnsNoContent()
    {
        var (_, followerToken) = await CreateAndAuthenticateUserAsync("unfollow_ok_a");
        var (followedId, _) = await CreateAndAuthenticateUserAsync("unfollow_ok_b");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", followerToken);

        var followResponse = await _client.PostAsync($"/api/user/follows/{followedId}", null);
        Assert.Equal(HttpStatusCode.Created, followResponse.StatusCode);

        var unfollowResponse = await _client.DeleteAsync($"/api/user/follows/{followedId}");

        Assert.Equal(HttpStatusCode.NoContent, unfollowResponse.StatusCode);
    }

    [Fact]
    public async Task FollowAndUnfollow_UpdatesFollowerCount()
    {
        var (_, followerToken) = await CreateAndAuthenticateUserAsync("follow_count_a");
        var (followedId, followedToken) = await CreateAndAuthenticateUserAsync("follow_count_b");

        _client.DefaultRequestHeaders.Authorization = new("Bearer", followedToken);
        var before = await (await _client.GetAsync("/api/user/statistics"))
            .Content.ReadFromJsonAsync<UserStatisticsResponse>();
        Assert.Equal(0, before!.Followers);

        _client.DefaultRequestHeaders.Authorization = new("Bearer", followerToken);
        var followResponse = await _client.PostAsync($"/api/user/follows/{followedId}", null);
        Assert.Equal(HttpStatusCode.Created, followResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new("Bearer", followedToken);
        var afterFollow = await (await _client.GetAsync("/api/user/statistics"))
            .Content.ReadFromJsonAsync<UserStatisticsResponse>();
        Assert.Equal(1, afterFollow!.Followers);

        _client.DefaultRequestHeaders.Authorization = new("Bearer", followerToken);
        var unfollowResponse = await _client.DeleteAsync($"/api/user/follows/{followedId}");
        Assert.Equal(HttpStatusCode.NoContent, unfollowResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new("Bearer", followedToken);
        var afterUnfollow = await (await _client.GetAsync("/api/user/statistics"))
            .Content.ReadFromJsonAsync<UserStatisticsResponse>();
        Assert.Equal(0, afterUnfollow!.Followers);
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
}
