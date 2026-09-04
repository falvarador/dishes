namespace dishes.Server.Data.Entities;

public class UserFollow
{
    public Guid Id { get; set; }
    public string FollowerUserId { get; set; } = string.Empty;
    public string FollowedUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public AppIdentityUser? Follower { get; set; }
    public AppIdentityUser? Followed { get; set; }

    public UserFollow()
    {
    }

    public UserFollow(string followerUserId, string followedUserId)
    {
        Id = Guid.NewGuid();
        FollowerUserId = followerUserId;
        FollowedUserId = followedUserId;
        CreatedAt = DateTime.UtcNow;
    }
}
