using dishes.Server.Data;
using Microsoft.AspNetCore.Identity;
using dishes.Server.Data.Entities;
using System.Security.Claims;

namespace dishes.Server.Features.Account.AccountSettings;

/// <summary>
/// Handler for retrieving the authenticated user's account settings.
/// </summary>
public class GetAccountSettingsHandler
{
    public async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        UserManager<AppIdentityUser> userManager,
        CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        var response = new AccountSettingsResponse(
            Id: currentUser.Id,
            Email: currentUser.Email ?? string.Empty,
            UserName: currentUser.UserName ?? string.Empty,
            FullName: currentUser.FullName,
            CulinaryTitle: currentUser.CulinaryTitle,
            Biography: currentUser.Biography,
            Location: currentUser.Location,
            ProfilePhotoUrl: currentUser.ProfilePhotoUrl,
            CreatedAt: currentUser.CreatedAt
        );

        return Results.Ok(response);
    }
}
