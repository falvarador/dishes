namespace dishes.Server.Features.Recipes.GetRecipesList;

public class RecipeSummaryResponse
{
    public RecipeSummaryResponse(
        Guid id,
        string title,
        string difficulty,
        string? coverPhotoPath,
        Guid creatorId,
        DateTime createdAt,
        bool isPublished,
        double averageRating = 0.0,
        int ratingCount = 0)
    {
        Id = id;
        Title = title;
        Difficulty = difficulty;
        CoverPhotoPath = coverPhotoPath;
        CreatorId = creatorId;
        CreatedAt = createdAt;
        IsPublished = isPublished;
        AverageRating = averageRating;
        RatingCount = ratingCount;
    }

    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Difficulty { get; set; }
    public string? CoverPhotoPath { get; set; }
    public Guid CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
}
