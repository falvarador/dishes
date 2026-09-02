using dishes.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Identity.GetUserBadges;

public class GetUserBadgesHandler
{
    public async Task<IResult> HandleAsync(string userId, AppDbContext context, CancellationToken cancellationToken)
    {
        var userBadges = await context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Badge)
            .OrderByDescending(ub => ub.AwardedAt)
            .Select(ub => new BadgeResponse
            {
                Id = ub.BadgeId,
                Name = ub.Badge!.Name,
                Description = ub.Badge.Description,
                IconUrl = ub.Badge.IconUrl,
                AwardedAt = ub.AwardedAt
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(new { badges = userBadges, count = userBadges.Count });
    }
}

public class BadgeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public DateTime AwardedAt { get; set; }
}
