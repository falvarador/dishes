namespace dishes.Server.Data.Entities;

public class UserBadge
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid BadgeId { get; set; }
    public DateTime AwardedAt { get; set; }

    // Navigation
    public AppIdentityUser? User { get; set; }
    public Badge? Badge { get; set; }

    public UserBadge()
    {
    }

    public UserBadge(Guid id, string userId, Guid badgeId)
    {
        Id = id;
        UserId = userId;
        BadgeId = badgeId;
        AwardedAt = DateTime.UtcNow;
    }
}
