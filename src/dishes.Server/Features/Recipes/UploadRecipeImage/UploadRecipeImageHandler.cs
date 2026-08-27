using dishes.Server.Data;
using dishes.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.UploadRecipeImage;

public static class UploadRecipeImageHandler
{
    private static async Task<IResult> HandleAsync(
        Guid id,
        IFormCollection form,
        AppDbContext context,
        IImageUploadService imageUploadService,
        CancellationToken cancellationToken)
    {
        // Get the recipe
        var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (recipe is null)
        {
            return Results.NotFound();
        }

        // Get the image file from form
        var file = form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "Image file is required" });
        }

        try
        {
            // Upload the image
            var imagePath = await imageUploadService.UploadRecipeImageAsync(file, id, cancellationToken);

            // Delete old image if it exists
            if (!string.IsNullOrEmpty(recipe.CoverPhotoPath))
            {
                await imageUploadService.DeleteRecipeImageAsync(recipe.CoverPhotoPath, cancellationToken);
            }

            // Update recipe with new image path
            recipe.CoverPhotoPath = imagePath;
            recipe.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            var response = new ImageUploadResponse(recipe.Id, imagePath);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public static IEndpointRouteBuilder MapUploadRecipeImage(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/{id}/image", HandleAsync)
            .WithName("UploadRecipeImage")
            .WithDescription("Upload a cover image for a recipe")
            .Accepts<ImageUploadRequest>("multipart/form-data")
            .Produces<ImageUploadResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .DisableAntiforgery();

        return routes;
    }
}
