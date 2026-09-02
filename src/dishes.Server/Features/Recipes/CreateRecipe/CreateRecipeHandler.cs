using dishes.Server.Data;
using dishes.Server.Data.Entities;
using System.Security.Claims;

namespace dishes.Server.Features.Recipes.CreateRecipe;

public static class CreateRecipeHandler
{
    private static async Task<IResult> HandleAsync(
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

        // Convert string userId to Guid for CreatorId
        if (!Guid.TryParse(userId, out Guid creatorId))
        {
            return Results.BadRequest("Invalid user ID format");
        }

        var recipe = new Recipe(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.PrepTime,
            request.Difficulty,
            creatorId
        )
        {
            CoverPhotoPath = request.CoverPhotoPath
        };

        context.Recipes.Add(recipe);

        // Add ingredients
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

        // Add instructions
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

        // Add categories
        if (request.CategoryIds != null)
        {
            foreach (var categoryId in request.CategoryIds)
            {
                var recipeCategory = new RecipeRecipeCategory
                {
                    RecipeId = recipe.Id,
                    CategoryId = categoryId
                };
                context.Add(recipeCategory);
            }
        }

        // Add tags
        if (request.TagIds != null)
        {
            foreach (var tagId in request.TagIds)
            {
                var recipeTag = new RecipeRecipeTag
                {
                    RecipeId = recipe.Id,
                    TagId = tagId
                };
                context.Add(recipeTag);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(recipe, request.Ingredients.Select(i => new RecipeIngredientResponse(Guid.NewGuid(), i.IngredientName, i.Quantity, i.Unit)).ToList(),
            request.Instructions.Select(i => new RecipeInstructionResponse(Guid.NewGuid(), i.StepNumber, i.Description)).ToList(),
            [], [], request.IsPublished);

        return Results.Created($"/api/recipes/{recipe.Id}", response);
    }

    private static RecipeResponse MapToResponse(Recipe recipe, List<RecipeIngredientResponse> ingredients, List<RecipeInstructionResponse> instructions, List<RecipeCategoryResponse> categories, List<RecipeTagResponse> tags, bool isPublished)
    {
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
            isPublished
        );
    }

    public static IEndpointRouteBuilder MapCreateRecipe(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/", HandleAsync)
            .WithName("CreateRecipe")
            .WithDescription("Create a new recipe")
            .Produces<RecipeResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return routes;
    }
}
