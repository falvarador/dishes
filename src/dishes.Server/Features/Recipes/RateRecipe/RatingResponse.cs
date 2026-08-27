namespace dishes.Server.Features.Recipes.RateRecipe;

public class RatingResponse
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public RatingResponse(Guid id, Guid recipeId, int rating, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        RecipeId = recipeId;
        Rating = rating;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
