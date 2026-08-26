using dishes.Server.Data;
using dishes.Server.Data.Entities;

namespace dishes.Server.Features.Ingredients.CreateIngredient;

public static class CreateIngredientHandler
{
    private static async Task<IResult> HandleAsync(IngredientRequest request, AppDbContext context, CancellationToken cancellationToken)
    {
        var ingredient = new Ingredient(Guid.NewGuid(), request.Name);

        context.Ingredients.Add(ingredient);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/ingredients/{ingredient.Id}", ingredient);
    }

    public static IEndpointRouteBuilder MapCreateIngredient(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/", HandleAsync)
            .WithName("CreateIngredient")
            .WithDescription("Create a new ingredient")
            .Produces<Ingredient>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return routes;
    }
}
