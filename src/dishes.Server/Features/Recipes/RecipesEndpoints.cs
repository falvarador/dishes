using dishes.Server.Features.Recipes.CreateRecipe;
using dishes.Server.Features.Recipes.DeleteRecipe;
using dishes.Server.Features.Recipes.FavoriteRecipe;
using dishes.Server.Features.Recipes.GetPublishedRecipes;
using dishes.Server.Features.Recipes.GetRecipeById;
using dishes.Server.Features.Recipes.GetRecipesList;
using dishes.Server.Features.Recipes.PublishRecipe;
using dishes.Server.Features.Recipes.RateRecipe;
using dishes.Server.Features.Recipes.UnfavoriteRecipe;
using dishes.Server.Features.Recipes.UnpublishRecipe;
using dishes.Server.Features.Recipes.UnrateRecipe;
using dishes.Server.Features.Recipes.UpdateRecipe;
using dishes.Server.Features.Recipes.UploadRecipeImage;

namespace dishes.Server.Features.Recipes;

public static class RecipesEndpoints
{
    public static IEndpointRouteBuilder MapRecipesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/recipes").WithTags("Recipes");
        group.MapGetRecipesList();
        group.MapGetPublishedRecipes();
        group.MapGetRecipeById();
        group.MapCreateRecipe();
        group.MapUpdateRecipe();
        group.MapDeleteRecipe();
        group.MapPublishRecipe();
        group.MapUnpublishRecipe();
        group.MapUploadRecipeImage();
        group.MapRateRecipe();
        group.MapUnrateRecipe();
        group.MapFavoriteRecipe();
        group.MapUnfavoriteRecipe();

        return group;
    }
}
