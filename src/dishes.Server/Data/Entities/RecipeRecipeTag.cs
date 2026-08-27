namespace dishes.Server.Data.Entities;

public class RecipeRecipeTag
{
    public Guid RecipeId { get; set; }
    public Guid TagId { get; set; }

    public Recipe? Recipe { get; set; }
    public RecipeTag? Tag { get; set; }
}
