using dishes.Server.Features.Identity.GetProfile;
using dishes.Server.Features.Identity.UpdateProfile;
using Microsoft.AspNetCore.Mvc;

namespace dishes.Server.Features.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/profile", GetProfile)
            .WithName("GetProfile");

        routes.MapPut("/profile", UpdateProfile)
            .WithName("UpdateProfile");

        return routes;
    }

    private static async Task<IResult> GetProfile(
        [FromServices] GetProfileHandler handler,
        HttpContext httpContext)
    {
        return await handler.HandleAsync(httpContext);
    }

    private static async Task<IResult> UpdateProfile(
        [FromServices] UpdateProfileHandler handler,
        HttpContext httpContext,
        [FromBody] UpdateProfileRequest request)
    {
        return await handler.HandleAsync(httpContext, request);
    }
}
