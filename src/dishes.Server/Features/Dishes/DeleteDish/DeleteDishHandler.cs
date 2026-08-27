using dishes.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Dishes.DeleteDish;

public static class DeleteDishHandler
{
    private static async Task<IResult> HandleAsync(Guid id, AppDbContext context, CancellationToken cancellationToken)
    {
        var dish = await context.Dishes
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (dish is null)
        {
            return Results.NotFound();
        }

        context.Dishes.Remove(dish);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    public static IEndpointRouteBuilder MapDeleteDish(this IEndpointRouteBuilder routes)
    {
        routes.MapDelete("/{id}", HandleAsync)
            .WithName("DeleteDish")
            .WithDescription("Delete an existing dish")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
