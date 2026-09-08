using dishes.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace dishes.Server.Features.Recipes.DeleteRecipe;

public static class DeleteRecipeHandler
{
    private static async Task<IResult> HandleAsync(
        Guid id,
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

        var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (recipe is null)
        {
            return Results.NotFound();
        }

        // Verify that the user is the creator of the recipe
        if (recipe.CreatorId != userIdGuid)
        {
            return Results.Forbid();
        }

        context.Recipes.Remove(recipe);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    public static IEndpointRouteBuilder MapDeleteRecipe(this IEndpointRouteBuilder routes)
    {
        routes.MapDelete("/{id}", HandleAsync)
            .WithName("DeleteRecipe")
            .WithDescription("Delete a recipe")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
