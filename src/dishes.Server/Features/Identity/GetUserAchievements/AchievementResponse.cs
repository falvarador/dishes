namespace dishes.Server.Features.Identity;

/// <summary>
/// Response DTO for a user's unlocked achievement.
/// </summary>
public class AchievementResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
