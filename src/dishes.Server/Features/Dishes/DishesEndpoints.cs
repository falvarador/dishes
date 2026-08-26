using dishes.Server.Features.Dishes.CreateDish;
using dishes.Server.Features.Dishes.DeleteDish;
using dishes.Server.Features.Dishes.GetDishById;
using dishes.Server.Features.Dishes.GetDishes;
using dishes.Server.Features.Dishes.UpdateDish;

namespace dishes.Server.Features.Dishes;

public static class DishesEndpoints
{
    public static IEndpointRouteBuilder MapDishesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/dishes").WithTags("Dishes");
        group.MapGetDishes();
        group.MapGetDishById();
        group.MapCreateDish();
        group.MapUpdateDish();
        group.MapDeleteDish();

        return group;
    }
}
