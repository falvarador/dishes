using dishes.Server.Features.Ingredients.CreateIngredient;
using dishes.Server.Features.Ingredients.DeleteIngredient;
using dishes.Server.Features.Ingredients.GetIngredientById;
using dishes.Server.Features.Ingredients.GetIngredients;
using dishes.Server.Features.Ingredients.UpdateIngredient;

namespace dishes.Server.Features.Ingredients;

public static class IngredientsEndpoints
{
    public static IEndpointRouteBuilder MapIngredientsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/ingredients").WithTags("Ingredients");
        group.MapGetIngredients();
        group.MapGetIngredientById();
        group.MapCreateIngredient();
        group.MapUpdateIngredient();
        group.MapDeleteIngredient();

        return group;
    }
}
