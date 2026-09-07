namespace dishes.Server.Features.Identity;

/// <summary>
/// Static definitions for the four unlockable achievements in the system.
/// Each achievement has an immutable id, name, description, and icon URL.
/// </summary>
public static class AchievementDefinitions
{
    public const string TopContributorId = "TopContributor";
    public const string MasterChefId = "MasterChef";
    public const string HotStreakId = "HotStreak";
    public const string VideoStarId = "VideoStar";

    public static readonly Achievement TopContributor = new(
        id: TopContributorId,
        name: "Top Contributor",
        description: "Unlock by publishing 5+ recipes",
        icon: "https://api.example.com/icons/top-contributor.png"
    );

    public static readonly Achievement MasterChef = new(
        id: MasterChefId,
        name: "Master Chef",
        description: "Unlock by publishing 50+ recipes",
        icon: "https://api.example.com/icons/master-chef.png"
    );

    public static readonly Achievement HotStreak = new(
        id: HotStreakId,
        name: "Hot Streak",
        description: "Unlock by publishing at least 1 recipe in each of 7 consecutive days",
        icon: "https://api.example.com/icons/hot-streak.png"
    );

    public static readonly Achievement VideoStar = new(
        id: VideoStarId,
        name: "Video Star",
        description: "Unlock by publishing at least 1 recipe with a video",
        icon: "https://api.example.com/icons/video-star.png"
    );

    public static readonly IReadOnlyList<Achievement> All = new List<Achievement>
    {
        TopContributor,
        MasterChef,
        HotStreak,
        VideoStar
    };

    /// <summary>Gets an achievement by its ID, or null if not found.</summary>
    public static Achievement? GetAchievementById(string achievementId)
    {
        return All.FirstOrDefault(a => a.Id == achievementId);
    }
}

/// <summary>Represents an unlockable achievement definition.</summary>
public class Achievement
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Icon { get; }

    public Achievement(string id, string name, string description, string icon)
    {
        Id = id;
        Name = name;
        Description = description;
        Icon = icon;
    }
}
