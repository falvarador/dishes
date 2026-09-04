using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Identity.GetUserStatistics;

public class GetUserStatisticsHandler
{
    public async Task<IResult> HandleAsync(
        string userId,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var creatorId))
        {
            return Results.BadRequest("Invalid user ID format");
        }

        var publishedRecipes = await context.Recipes
            .CountAsync(r => r.CreatorId == creatorId && r.Status == RecipeStatus.Published, cancellationToken);

        var likesReceived = await context.UserRecipeFavorites
            .CountAsync(f => f.Recipe.CreatorId == creatorId, cancellationToken);

        var followers = await context.UserFollows
            .CountAsync(f => f.FollowedUserId == userId, cancellationToken);

        return Results.Ok(new UserStatisticsResponse(publishedRecipes, likesReceived, followers));
    }
}
