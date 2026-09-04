using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity.FollowUser;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class FollowUserHandlerUnitTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly FollowUserHandler _handler = new();

    public FollowUserHandlerUnitTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _context.UserFollows.RemoveRange(_context.UserFollows);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task HandleAsync_SelfFollow_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid().ToString();

        var result = await _handler.HandleAsync(userId, userId, _context, CancellationToken.None);

        Assert.IsType<BadRequest<string>>(result);
        Assert.Empty(_context.UserFollows);
    }

    [Fact]
    public async Task HandleAsync_MissingUser_ReturnsNotFound()
    {
        var followerId = Guid.NewGuid().ToString();
        var missingId = Guid.NewGuid().ToString();

        var result = await _handler.HandleAsync(followerId, missingId, _context, CancellationToken.None);

        Assert.IsType<NotFound<string>>(result);
        Assert.Empty(_context.UserFollows);
    }

    [Fact]
    public async Task HandleAsync_DuplicateFollow_ReturnsConflict()
    {
        var follower = await SeedUserAsync("follower@test.com");
        var followed = await SeedUserAsync("followed@test.com");
        _context.UserFollows.Add(new UserFollow(follower.Id, followed.Id));
        await _context.SaveChangesAsync();

        var result = await _handler.HandleAsync(follower.Id, followed.Id, _context, CancellationToken.None);

        Assert.IsType<Conflict<string>>(result);
        Assert.Equal(1, await _context.UserFollows.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_ValidFollow_ReturnsCreated()
    {
        var follower = await SeedUserAsync("follower-ok@test.com");
        var followed = await SeedUserAsync("followed-ok@test.com");

        var result = await _handler.HandleAsync(follower.Id, followed.Id, _context, CancellationToken.None);

        var created = Assert.IsType<Created<FollowUserResponse>>(result);
        Assert.Equal(followed.Id, created.Value!.FollowedUserId);
        Assert.Equal(follower.Id, created.Value.FollowerUserId);
        Assert.Equal(1, await _context.UserFollows.CountAsync());
    }

    private async Task<AppIdentityUser> SeedUserAsync(string email)
    {
        var user = new AppIdentityUser(email)
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            EmailConfirmed = true,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
