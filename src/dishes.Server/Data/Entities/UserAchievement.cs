namespace dishes.Server.Data.Entities;

public class UserAchievement
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AchievementId { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public AppIdentityUser? User { get; set; }

    public UserAchievement()
    {
    }

    public UserAchievement(Guid id, string userId, string achievementId, DateTime unlockedAt)
    {
        Id = id;
        UserId = userId;
        AchievementId = achievementId;
        UnlockedAt = unlockedAt;
        CreatedAt = DateTime.UtcNow;
    }
}
