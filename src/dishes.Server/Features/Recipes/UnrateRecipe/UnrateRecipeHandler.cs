using dishes.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.UnrateRecipe;

public static class UnrateRecipeHandler
{
    private static async Task<IResult> HandleAsync(
        Guid id,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        // Get current user ID (placeholder - should come from auth context)
        // TODO: Get from HttpContext.User.FindFirst("sub") or similar claim
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Fixed test userId

        // Check if recipe exists
        var recipe = await context.Recipes.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (recipe is null)
            return Results.NotFound($"Recipe with ID {id} not found.");

        // Find and delete the user's rating for this recipe
        var rating = await context.UserRecipeRatings
            .FirstOrDefaultAsync(r => r.RecipeId == id && r.UserId == userId, cancellationToken);

        if (rating is null)
            return Results.NotFound("User has not rated this recipe.");

        context.UserRecipeRatings.Remove(rating);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    public static IEndpointRouteBuilder MapUnrateRecipe(this IEndpointRouteBuilder routes)
    {
        routes.MapDelete("/{id}/rate", HandleAsync)
            .WithName("UnrateRecipe")
            .WithDescription("Remove a user's rating from a recipe.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return routes;
    }
}
