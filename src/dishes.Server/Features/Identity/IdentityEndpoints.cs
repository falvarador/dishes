using dishes.Server.Data;
using dishes.Server.Features.Identity.FollowUser;
using dishes.Server.Features.Identity.GetProfile;
using dishes.Server.Features.Identity.GetUserBadges;
using dishes.Server.Features.Identity.GetUserStatistics;
using dishes.Server.Features.Identity.UnfollowUser;
using dishes.Server.Features.Identity.UpdateProfile;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        routes.MapGet("/statistics", GetStatistics)
            .WithName("GetUserStatistics")
            .WithDescription("Get the currently authenticated user's creator statistics.")
            .Produces<UserStatisticsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        routes.MapPost("/follows/{userId}", FollowUser)
            .WithName("FollowUser")
            .WithDescription("Follow another creator.")
            .Produces<FollowUserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        routes.MapDelete("/follows/{userId}", UnfollowUser)
            .WithName("UnfollowUser")
            .WithDescription("Unfollow a creator.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        routes.MapGet("/achievements", GetAchievements)
            .WithName("GetAchievements")
            .WithDescription("Get the unlocked achievements of the currently authenticated user.")
            .Produces<List<AchievementResponse>>(StatusCodes.Status200OK)
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
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        return await handler.HandleAsync(userId, httpContext.RequestServices.GetRequiredService<AppDbContext>(), httpContext.RequestAborted);
    }

    private static async Task<IResult> GetStatistics(
        [FromServices] GetUserStatisticsHandler handler,
        HttpContext httpContext,
        AppDbContext context)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        return await handler.HandleAsync(userId, context, httpContext.RequestAborted);
    }

    private static async Task<IResult> FollowUser(
        [FromServices] FollowUserHandler handler,
        string userId,
        HttpContext httpContext,
        AppDbContext context)
    {
        var followerUserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(followerUserId))
        {
            return Results.Unauthorized();
        }

        return await handler.HandleAsync(followerUserId, userId, context, httpContext.RequestAborted);
    }

    private static async Task<IResult> UnfollowUser(
        [FromServices] UnfollowUserHandler handler,
        string userId,
        HttpContext httpContext,
        AppDbContext context)
    {
        var followerUserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(followerUserId))
        {
            return Results.Unauthorized();
        }

        return await handler.HandleAsync(followerUserId, userId, context, httpContext.RequestAborted);
    }

    private static async Task<IResult> GetAchievements(
        [FromServices] GetUserAchievementsHandler handler,
        HttpContext httpContext)
    {
        try
        {
            var achievements = await handler.HandleAsync(httpContext.User);
            return Results.Ok(achievements);
        }
        catch (InvalidOperationException)
        {
            return Results.Unauthorized();
        }
    }
}

