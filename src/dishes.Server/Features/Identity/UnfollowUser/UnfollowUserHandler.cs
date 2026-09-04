using dishes.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Identity.UnfollowUser;

public class UnfollowUserHandler
{
    public async Task<IResult> HandleAsync(
        string followerUserId,
        string followedUserId,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var follow = await context.UserFollows
            .FirstOrDefaultAsync(
                f => f.FollowerUserId == followerUserId && f.FollowedUserId == followedUserId,
                cancellationToken);

        if (follow is null)
        {
            return Results.NotFound("Follow relationship not found.");
        }

        context.UserFollows.Remove(follow);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
