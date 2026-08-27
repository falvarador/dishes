using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.RateRecipe;

public static class RateRecipeHandler
{
    private static async Task<IResult> HandleAsync(
        Guid id,
        RateRecipeRequest request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        // Validate rating is 1-5
        if (request.Rating < 1 || request.Rating > 5)
            return Results.BadRequest("Rating must be between 1 and 5.");

        // Get current user ID (placeholder - should come from auth context)
        // TODO: Get from HttpContext.User.FindFirst("sub") or similar claim
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Fixed test userId

        // Check if recipe exists
        var recipe = await context.Recipes.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (recipe is null)
            return Results.NotFound($"Recipe with ID {id} not found.");

        // Check if user already rated this recipe
        var existingRating = await context.UserRecipeRatings
            .FirstOrDefaultAsync(r => r.RecipeId == id && r.UserId == userId, cancellationToken);

        if (existingRating is not null)
        {
            // Update existing rating
            existingRating.UpdateRating(request.Rating);
            context.UserRecipeRatings.Update(existingRating);
        }
        else
        {
            // Create new rating
            var newRating = new UserRecipeRating(userId, id, request.Rating);
            context.UserRecipeRatings.Add(newRating);
        }

        await context.SaveChangesAsync(cancellationToken);

        var rating = existingRating ?? new UserRecipeRating(userId, id, request.Rating);
        var response = new RatingResponse(rating.Id, rating.RecipeId, rating.Rating, rating.CreatedAt, rating.UpdatedAt);
        return Results.Ok(response);
    }

    public static IEndpointRouteBuilder MapRateRecipe(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/{id}/rate", HandleAsync)
            .WithName("RateRecipe")
            .WithDescription("Rate a recipe (1-5 stars). Updates rating if already rated.")
            .Produces<RatingResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return routes;
    }
}
