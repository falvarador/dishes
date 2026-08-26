using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Ingredients.GetIngredients;

public static class GetIngredientsHandler
{
    private static async Task<IResult> HandleAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var ingredients = await context.Ingredients
            .ToListAsync(cancellationToken);

        return Results.Ok(ingredients);
    }

    public static IEndpointRouteBuilder MapGetIngredients(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/", HandleAsync)
            .WithName("GetIngredients")
            .WithDescription("Get all ingredients")
            .Produces<IEnumerable<Ingredient>>(StatusCodes.Status200OK);

        return routes;
    }
}
