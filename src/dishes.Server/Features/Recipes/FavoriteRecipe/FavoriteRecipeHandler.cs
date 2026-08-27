using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.FavoriteRecipe;

public static class FavoriteRecipeHandler
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

        // Check if user already favorited this recipe
        var existingFavorite = await context.UserRecipeFavorites
            .FirstOrDefaultAsync(f => f.RecipeId == id && f.UserId == userId, cancellationToken);

        if (existingFavorite is not null)
            return Results.Conflict("Recipe is already in your favorites.");

        // Create new favorite
        var favorite = new UserRecipeFavorite(userId, id);
        context.UserRecipeFavorites.Add(favorite);
        await context.SaveChangesAsync(cancellationToken);

        var response = new FavoriteResponse(favorite.Id, favorite.RecipeId, favorite.CreatedAt);
        return Results.Created($"/api/recipes/{id}/favorite", response);
    }

    public static IEndpointRouteBuilder MapFavoriteRecipe(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/{id}/favorite", HandleAsync)
            .WithName("FavoriteRecipe")
            .WithDescription("Add a recipe to user's favorites.")
            .Produces<FavoriteResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .WithOpenApi();

        return routes;
    }
}
