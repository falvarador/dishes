using dishes.Server.Data;
using Microsoft.AspNetCore.Identity;
using dishes.Server.Data.Entities;
using System.Security.Claims;

namespace dishes.Server.Features.Account.AccountSettings;

/// <summary>
/// Handler for updating the authenticated user's account settings.
/// Only the authenticated user can update their own settings.
/// </summary>
public class UpdateAccountSettingsHandler
{
    public async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        UpdateAccountSettingsRequest request,
        UserManager<AppIdentityUser> userManager,
        CancellationToken cancellationToken)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.Unauthorized();
        }

        // Update user profile properties
        currentUser.FullName = request.FullName;
        currentUser.CulinaryTitle = request.CulinaryTitle;
        currentUser.Biography = request.Biography;
        currentUser.Location = request.Location;
        currentUser.ProfilePhotoUrl = request.ProfilePhotoUrl;

        var result = await userManager.UpdateAsync(currentUser);
        if (!result.Succeeded)
        {
            return Results.BadRequest(new
            {
                message = "Failed to update account settings",
                errors = result.Errors.Select(e => e.Description)
            });
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
