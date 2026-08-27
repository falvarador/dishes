namespace dishes.Server.Data.Entities;

public class RecipeRecipeCategory
{
    public Guid RecipeId { get; set; }
    public Guid CategoryId { get; set; }

    public Recipe? Recipe { get; set; }
    public RecipeCategory? Category { get; set; }
}
