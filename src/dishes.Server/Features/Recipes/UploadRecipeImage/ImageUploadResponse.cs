namespace dishes.Server.Features.Recipes.UploadRecipeImage;

public class ImageUploadResponse
{
    public ImageUploadResponse(Guid recipeId, string imagePath)
    {
        RecipeId = recipeId;
        ImagePath = imagePath;
    }

    public Guid RecipeId { get; set; }
    public string ImagePath { get; set; }
}
