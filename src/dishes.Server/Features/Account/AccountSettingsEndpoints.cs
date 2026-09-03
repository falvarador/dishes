using dishes.Server.Data.Entities;
using dishes.Server.Features.Account.AccountSettings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace dishes.Server.Features.Account;

public static class AccountSettingsEndpoints
{
    public static IEndpointRouteBuilder MapAccountSettingsEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/settings", GetAccountSettings)
            .WithName("GetAccountSettings")
            .WithDescription("Get the currently authenticated user's account settings.")
            .Produces<AccountSettingsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        routes.MapPut("/settings", UpdateAccountSettings)
            .WithName("UpdateAccountSettings")
            .WithDescription("Update the currently authenticated user's account settings.")
            .Produces<AccountSettingsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return routes;
    }

    private static async Task<IResult> GetAccountSettings(
        [FromServices] GetAccountSettingsHandler handler,
        ClaimsPrincipal user,
        UserManager<AppIdentityUser> userManager,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(user, userManager, cancellationToken);
    }

    private static async Task<IResult> UpdateAccountSettings(
        [FromServices] UpdateAccountSettingsHandler handler,
        ClaimsPrincipal user,
        [FromBody] UpdateAccountSettingsRequest request,
        UserManager<AppIdentityUser> userManager,
        CancellationToken cancellationToken)
    {
        return await handler.HandleAsync(user, request, userManager, cancellationToken);
    }
}
