namespace dishes.Server.Data.Entities;

public class Badge
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();

    public Badge()
    {
    }

    public Badge(Guid id, string name, string description, string iconUrl)
    {
        Id = id;
        Name = name;
        Description = description;
        IconUrl = iconUrl;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
