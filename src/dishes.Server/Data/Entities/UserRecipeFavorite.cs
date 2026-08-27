namespace dishes.Server.Data.Entities;

/// <summary>
/// Represents a user's favorite recipe.
/// </summary>
public class UserRecipeFavorite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RecipeId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public Recipe Recipe { get; set; } = null!;

    public UserRecipeFavorite() { }

    public UserRecipeFavorite(Guid userId, Guid recipeId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RecipeId = recipeId;
        CreatedAt = DateTime.UtcNow;
    }
}
