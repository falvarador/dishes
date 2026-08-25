using dishes.Server.Features.Dishes.GetDishes;

namespace dishes.Server.Features.Dishes;

public static class DishesEndpoints
{
    public static IEndpointRouteBuilder MapDishesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/dishes").WithTags("Dishes");
        group.MapGetDishes();

        return group;
    }
}