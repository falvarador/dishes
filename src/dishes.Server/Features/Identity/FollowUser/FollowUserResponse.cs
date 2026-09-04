namespace dishes.Server.Features.Identity.FollowUser;

public record FollowUserResponse(
    Guid Id,
    string FollowerUserId,
    string FollowedUserId,
    DateTime CreatedAt);
