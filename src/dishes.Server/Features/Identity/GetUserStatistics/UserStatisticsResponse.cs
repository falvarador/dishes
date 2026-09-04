namespace dishes.Server.Features.Identity.GetUserStatistics;

public record UserStatisticsResponse(
    int PublishedRecipes,
    int LikesReceived,
    int Followers);
