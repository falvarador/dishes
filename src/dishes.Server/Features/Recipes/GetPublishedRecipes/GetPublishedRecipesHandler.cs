using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.GetPublishedRecipes;

public static class GetPublishedRecipesHandler
{
    private static async Task<IResult> HandleAsync(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var recipes = await context.Recipes
            .Where(r => r.Status == RecipeStatus.Published)
            .Include(r => r.Ingredients)
            .Include(r => r.Instructions)
            .Include(r => r.Categories)
            .ThenInclude(c => c.Category)
            .Include(r => r.Tags)
            .ThenInclude(t => t.Tag)
            .OrderByDescending(r => r.PublishedAt)
            .ToListAsync(cancellationToken);

        var response = recipes.Select(MapToResponse).ToList();
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
            true // Always published
        );
    }

    public static IEndpointRouteBuilder MapGetPublishedRecipes(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/published", HandleAsync)
            .WithName("GetPublishedRecipes")
            .WithDescription("Get all published recipes")
            .Produces<List<RecipeResponse>>(StatusCodes.Status200OK);

        return routes;
    }
}
