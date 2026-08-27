namespace dishes.Server.Features.Recipes.FavoriteRecipe;

public class FavoriteResponse
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public DateTime CreatedAt { get; set; }

    public FavoriteResponse(Guid id, Guid recipeId, DateTime createdAt)
    {
        Id = id;
        RecipeId = recipeId;
        CreatedAt = createdAt;
    }
}
