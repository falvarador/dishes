using Microsoft.EntityFrameworkCore;
using dishes.Server.Data;
using dishes.Server.Features.Recipes;

namespace dishes.Server.Features.Recipes.GetRecipeById;

public static class GetRecipeByIdHandler
{
    private static async Task<IResult> HandleAsync(Guid id, AppDbContext context, CancellationToken cancellationToken)
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

        var response = MapToResponse(recipe);
        return Results.Ok(response);
    }

    private static RecipeDetailResponse MapToResponse(dishes.Server.Data.Entities.Recipe recipe)
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

        return new RecipeDetailResponse(
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
            false // IsPublished - will be added when publication logic is implemented
        );
    }

    public static IEndpointRouteBuilder MapGetRecipeById(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/{id}", HandleAsync)
            .WithName("GetRecipeById")
            .WithDescription("Get a recipe by ID")
            .Produces<RecipeDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
