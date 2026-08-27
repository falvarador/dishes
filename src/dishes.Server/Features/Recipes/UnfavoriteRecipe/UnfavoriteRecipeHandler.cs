using dishes.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.UnfavoriteRecipe;

public static class UnfavoriteRecipeHandler
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

        // Find and delete the user's favorite for this recipe
        var favorite = await context.UserRecipeFavorites
            .FirstOrDefaultAsync(f => f.RecipeId == id && f.UserId == userId, cancellationToken);

        if (favorite is null)
            return Results.NotFound("Recipe is not in your favorites.");

        context.UserRecipeFavorites.Remove(favorite);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    public static IEndpointRouteBuilder MapUnfavoriteRecipe(this IEndpointRouteBuilder routes)
    {
        routes.MapDelete("/{id}/favorite", HandleAsync)
            .WithName("UnfavoriteRecipe")
            .WithDescription("Remove a recipe from user's favorites.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return routes;
    }
}
