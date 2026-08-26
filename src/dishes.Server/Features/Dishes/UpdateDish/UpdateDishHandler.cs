using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Dishes.UpdateDish;

public static class UpdateDishHandler
{
    private static async Task<IResult> HandleAsync(Guid id, DishRequest request, AppDbContext context, CancellationToken cancellationToken)
    {
        var dish = await context.Dishes
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (dish is null)
        {
            return Results.NotFound();
        }

        dish.Name = request.Name;
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(dish);
    }

    public static IEndpointRouteBuilder MapUpdateDish(this IEndpointRouteBuilder routes)
    {
        routes.MapPut("/{id}", HandleAsync)
            .WithName("UpdateDish")
            .WithDescription("Update an existing dish")
            .Produces<Dish>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        return routes;
    }
}
