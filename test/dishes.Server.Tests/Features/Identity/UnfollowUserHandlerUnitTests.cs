using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity.UnfollowUser;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace dishes.Server.Tests.Features.Identity;

public class UnfollowUserHandlerUnitTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly UnfollowUserHandler _handler = new();

    public UnfollowUserHandlerUnitTests()
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
    public async Task HandleAsync_MissingRelationship_ReturnsNotFound()
    {
        var result = await _handler.HandleAsync(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            _context,
            CancellationToken.None);

        Assert.IsType<NotFound<string>>(result);
    }

    [Fact]
    public async Task HandleAsync_ExistingRelationship_ReturnsNoContent()
    {
        var follower = await SeedUserAsync("unfollow-follower@test.com");
        var followed = await SeedUserAsync("unfollow-followed@test.com");
        _context.UserFollows.Add(new UserFollow(follower.Id, followed.Id));
        await _context.SaveChangesAsync();

        var result = await _handler.HandleAsync(follower.Id, followed.Id, _context, CancellationToken.None);

        Assert.IsType<NoContent>(result);
        Assert.Empty(_context.UserFollows);
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
