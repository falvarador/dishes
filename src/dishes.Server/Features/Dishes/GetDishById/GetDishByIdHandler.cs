using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Dishes.GetDishById;

public static class GetDishByIdHandler
{
    private static async Task<IResult> HandleAsync(Guid id, AppDbContext context, CancellationToken cancellationToken)
    {
        var dish = await context.Dishes
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return dish is null
            ? Results.NotFound()
            : Results.Ok(dish);
    }

    public static IEndpointRouteBuilder MapGetDishById(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/{id}", HandleAsync)
            .WithName("GetDishById")
            .WithDescription("Get a dish by id")
            .Produces<Dish>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
