using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.UnpublishRecipe;

public static class UnpublishRecipeHandler
{
    private static async Task<IResult> HandleAsync(
        Guid id,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var recipe = await context.Recipes
            .Include(r => r.Ingredients)
            .Include(r => r.Instructions)
            .Include(r => r.Categories)
            .ThenInclude(c => c.Category)
            .Include(r => r.Tags)
            .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (recipe is null)
        {
            return Results.NotFound();
        }

        if (recipe.Status == RecipeStatus.Draft)
        {
            return Results.BadRequest(new { error = "Recipe is already in draft status" });
        }

        recipe.Status = RecipeStatus.Draft;
        recipe.PublishedAt = null;
        recipe.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(recipe);
        return Results.Ok(response);
    }

    private static RecipeResponse MapToResponse(Recipe recipe)
    {
        var ingredients = recipe.Ingredients.Select(i => new RecipeIngredientResponse(
            i.Id,
            i.IngredientName,
            i.Quantity,
            i.Unit
        )).ToList();

        var instructions = recipe.Instructions
            .OrderBy(i => i.StepNumber)
            .Select(i => new RecipeInstructionResponse(
                i.Id,
                i.StepNumber,
                i.Description
            )).ToList();

        var categories = recipe.Categories
            .Select(c => new RecipeCategoryResponse(
                c.Category!.Id,
                c.Category.Name
            )).ToList();

        var tags = recipe.Tags
            .Select(t => new RecipeTagResponse(
                t.Tag!.Id,
                t.Tag.Name
            )).ToList();

        return new RecipeResponse(
            recipe.Id,
            recipe.Title,
            recipe.Description,
            recipe.CoverPhotoPath,
            recipe.PrepTime,
            recipe.Difficulty,
            recipe.CreatorId,
            recipe.CreatedAt,
            recipe.UpdatedAt,
            ingredients,
            instructions,
            categories,
            tags,
            recipe.Status == RecipeStatus.Published
        );
    }

    public static IEndpointRouteBuilder MapUnpublishRecipe(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/{id}/unpublish", HandleAsync)
            .WithName("UnpublishRecipe")
            .WithDescription("Unpublish a recipe (change from published to draft)")
            .Produces<RecipeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        return routes;
    }
}
