namespace dishes.Server.Features.Recipes.UploadRecipeImage;

public class ImageUploadRequest
{
    public required IFormFile Image { get; set; }
}
