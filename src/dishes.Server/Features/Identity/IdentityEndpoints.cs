using dishes.Server.Features.Identity.GetProfile;
using dishes.Server.Features.Identity.GetUserBadges;
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

        routes.MapGet("/badges", GetBadges)
            .WithName("GetBadges")
            .WithDescription("Get the badges of the currently authenticated user.")
            .Produces<List<BadgeResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

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

    private static async Task<IResult> GetBadges(
        [FromServices] GetUserBadgesHandler handler,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        return await handler.HandleAsync(userId, httpContext.RequestServices.GetRequiredService<Data.AppDbContext>(), httpContext.RequestAborted);
    }
}
