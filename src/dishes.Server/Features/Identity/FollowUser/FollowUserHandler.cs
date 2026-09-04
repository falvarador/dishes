using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Identity.FollowUser;

public class FollowUserHandler
{
    public async Task<IResult> HandleAsync(
        string followerUserId,
        string followedUserId,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (string.Equals(followerUserId, followedUserId, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest("You cannot follow yourself.");
        }

        var followedExists = await context.Users
            .AnyAsync(u => u.Id == followedUserId, cancellationToken);

        if (!followedExists)
        {
            return Results.NotFound($"User with ID {followedUserId} not found.");
        }

        var alreadyFollowing = await context.UserFollows
            .AnyAsync(
                f => f.FollowerUserId == followerUserId && f.FollowedUserId == followedUserId,
                cancellationToken);

        if (alreadyFollowing)
        {
            return Results.Conflict("You already follow this user.");
        }

        var follow = new UserFollow(followerUserId, followedUserId);
        context.UserFollows.Add(follow);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Results.Conflict("You already follow this user.");
        }

        var response = new FollowUserResponse(
            follow.Id,
            follow.FollowerUserId,
            follow.FollowedUserId,
            follow.CreatedAt);

        return Results.Created($"/api/user/follows/{followedUserId}", response);
    }
}
