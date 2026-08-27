using dishes.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Ingredients.DeleteIngredient;

public static class DeleteIngredientHandler
{
    private static async Task<IResult> HandleAsync(Guid id, AppDbContext context, CancellationToken cancellationToken)
    {
        var ingredient = await context.Ingredients
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (ingredient is null)
        {
            return Results.NotFound();
        }

        context.Ingredients.Remove(ingredient);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    public static IEndpointRouteBuilder MapDeleteIngredient(this IEndpointRouteBuilder routes)
    {
        routes.MapDelete("/{id}", HandleAsync)
            .WithName("DeleteIngredient")
            .WithDescription("Delete an existing ingredient")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
