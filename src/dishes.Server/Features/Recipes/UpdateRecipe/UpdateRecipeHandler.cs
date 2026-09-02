using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace dishes.Server.Features.Recipes.UpdateRecipe;

public static class UpdateRecipeHandler
{
    private static async Task<IResult> HandleAsync(
        Guid id,
        CreateRecipeRequest request,
        AppDbContext context,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        // Extract authenticated user ID from claims
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // Convert string userId to Guid for comparison
        if (!Guid.TryParse(userId, out Guid userIdGuid))
        {
            return Results.BadRequest("Invalid user ID format");
        }

        var recipe = await context.Recipes
            .Include(r => r.Ingredients)
            .Include(r => r.Instructions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (recipe is null)
        {
            return Results.NotFound();
        }

        // Verify that the user is the creator of the recipe
        if (recipe.CreatorId != userIdGuid)
        {
            return Results.Forbid();
        }

        // Update basic properties
        recipe.Title = request.Title;
        recipe.Description = request.Description;
        recipe.PrepTime = request.PrepTime;
        recipe.Difficulty = request.Difficulty;
        recipe.CoverPhotoPath = request.CoverPhotoPath;
        recipe.UpdatedAt = DateTime.UtcNow;

        // Clear and re-add ingredients
        context.RecipeIngredients.RemoveRange(recipe.Ingredients);

        foreach (var ingredient in request.Ingredients)
        {
            var recipeIngredient = new RecipeIngredient(
                Guid.NewGuid(),
                recipe.Id,
                ingredient.IngredientName,
                ingredient.Quantity,
                ingredient.Unit
            );
            context.RecipeIngredients.Add(recipeIngredient);
        }

        // Clear and re-add instructions
        context.RecipeInstructions.RemoveRange(recipe.Instructions);

        foreach (var instruction in request.Instructions)
        {
            var recipeInstruction = new RecipeInstruction(
                Guid.NewGuid(),
                recipe.Id,
                instruction.StepNumber,
                instruction.Description
            );
            context.RecipeInstructions.Add(recipeInstruction);
        }

        // Note: Category and Tag handling would require loading and updating junction tables
        // For now, we preserve existing categories and tags
        // Full implementation would clear and re-add them similar to ingredients/instructions

        await context.SaveChangesAsync(cancellationToken);

        // Reload to get updated relationships
        var updatedRecipe = await context.Recipes
            .Include(r => r.Ingredients)
            .Include(r => r.Instructions)
            .Include(r => r.Categories)
            .ThenInclude(c => c.Category)
            .Include(r => r.Tags)
            .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        var response = MapToResponse(updatedRecipe!);
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

    public static IEndpointRouteBuilder MapUpdateRecipe(this IEndpointRouteBuilder routes)
    {
        routes.MapPut("/{id}", HandleAsync)
            .WithName("UpdateRecipe")
            .WithDescription("Update an existing recipe")
            .Produces<RecipeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        return routes;
    }
}
