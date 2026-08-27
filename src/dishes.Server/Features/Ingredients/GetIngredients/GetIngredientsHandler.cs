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

        var response = ingredients.Select(i => new IngredientResponse(i.Id, i.Name));

        return Results.Ok(response);
    }

    public static IEndpointRouteBuilder MapGetIngredients(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/", HandleAsync)
            .WithName("GetIngredients")
            .WithDescription("Get all ingredients")
            .Produces<IEnumerable<IngredientResponse>>(StatusCodes.Status200OK);

        return routes;
    }
}
