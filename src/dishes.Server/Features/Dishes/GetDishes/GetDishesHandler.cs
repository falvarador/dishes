using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Dishes.GetDishes;

public static class GetDishesHandler
{
    private static async Task<IResult> HandleAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var dishes = await context.Dishes
            .Include(d => d.Ingredients)
            .ToListAsync(cancellationToken);
        
        return Results.Ok(dishes);
    }

    public static IEndpointRouteBuilder MapGetDishes(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/", HandleAsync)
            .WithName("GetDishes")
            .WithDescription("Get all dishes")
            .Produces<IEnumerable<Dish>>(StatusCodes.Status200OK);
        
        return routes;
    }
}
