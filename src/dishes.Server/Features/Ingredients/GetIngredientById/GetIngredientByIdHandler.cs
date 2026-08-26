using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Ingredients.GetIngredientById;

public static class GetIngredientByIdHandler
{
    private static async Task<IResult> HandleAsync(Guid id, AppDbContext context, CancellationToken cancellationToken)
    {
        var ingredient = await context.Ingredients
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        return ingredient is null
            ? Results.NotFound()
            : Results.Ok(ingredient);
    }

    public static IEndpointRouteBuilder MapGetIngredientById(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/{id}", HandleAsync)
            .WithName("GetIngredientById")
            .WithDescription("Get an ingredient by id")
            .Produces<Ingredient>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
