namespace dishes.Server.Data.Entities;

/// <summary>
/// Represents a user's rating (1-5 stars) for a recipe.
/// </summary>
public class UserRecipeRating
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RecipeId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public Recipe Recipe { get; set; } = null!;

    public UserRecipeRating() { }

    public UserRecipeRating(Guid userId, Guid recipeId, int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        Id = Guid.NewGuid();
        UserId = userId;
        RecipeId = recipeId;
        Rating = rating;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRating(int newRating)
    {
        if (newRating < 1 || newRating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(newRating));

        Rating = newRating;
        UpdatedAt = DateTime.UtcNow;
    }
}
