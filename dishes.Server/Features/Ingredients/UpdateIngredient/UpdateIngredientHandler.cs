using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Ingredients.UpdateIngredient;

public static class UpdateIngredientHandler
{
    private static async Task<IResult> HandleAsync(Guid id, IngredientRequest request, AppDbContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Name)] = ["Name is required."]
            });
        }

        if (request.Name.Length > 200)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Name)] = ["Name must be at most 200 characters."]
            });
        }

        var ingredient = await context.Ingredients
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (ingredient is null)
        {
            return Results.NotFound();
        }

        ingredient.Name = request.Name;
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(ingredient);
    }

    public static IEndpointRouteBuilder MapUpdateIngredient(this IEndpointRouteBuilder routes)
    {
        routes.MapPut("/{id}", HandleAsync)
            .WithName("UpdateIngredient")
            .WithDescription("Update an existing ingredient")
            .Produces<Ingredient>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        return routes;
    }
}
