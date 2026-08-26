using dishes.Server.Data;
using dishes.Server.Data.Entities;

namespace dishes.Server.Features.Dishes.CreateDish;

public static class CreateDishHandler
{
    private static async Task<IResult> HandleAsync(DishRequest request, AppDbContext context, CancellationToken cancellationToken)
    {
        var dish = new Dish(Guid.NewGuid(), request.Name);

        context.Dishes.Add(dish);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/dishes/{dish.Id}", dish);
    }

    public static IEndpointRouteBuilder MapCreateDish(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/", HandleAsync)
            .WithName("CreateDish")
            .WithDescription("Create a new dish")
            .Produces<Dish>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return routes;
    }
}
