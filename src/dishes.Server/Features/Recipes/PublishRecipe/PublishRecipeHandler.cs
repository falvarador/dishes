using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.PublishRecipe;

public static class PublishRecipeHandler
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

        if (recipe.Status == RecipeStatus.Published)
        {
            return Results.BadRequest(new { error = "Recipe is already published" });
        }

        // Validate recipe has required content before publishing
        if (!recipe.Ingredients.Any())
        {
            return Results.BadRequest(new { error = "Recipe must have at least one ingredient before publishing" });
        }

        if (!recipe.Instructions.Any())
        {
            return Results.BadRequest(new { error = "Recipe must have at least one instruction before publishing" });
        }

        recipe.Status = RecipeStatus.Published;
        recipe.PublishedAt = DateTime.UtcNow;
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

    public static IEndpointRouteBuilder MapPublishRecipe(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/{id}/publish", HandleAsync)
            .WithName("PublishRecipe")
            .WithDescription("Publish a recipe (change from draft to published)")
            .Produces<RecipeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        return routes;
    }
}
