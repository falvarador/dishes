using dishes.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.DeleteRecipe;

public static class DeleteRecipeHandler
{
    private static async Task<IResult> HandleAsync(
        Guid id,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (recipe is null)
        {
            return Results.NotFound();
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
