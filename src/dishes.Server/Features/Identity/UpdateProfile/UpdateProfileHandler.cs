using dishes.Server.Data.Entities;
using dishes.Server.Features.Identity.GetProfile;
using Microsoft.AspNetCore.Identity;

namespace dishes.Server.Features.Identity.UpdateProfile;

public class UpdateProfileHandler(UserManager<AppIdentityUser> userManager)
{
    public async Task<IResult> HandleAsync(HttpContext httpContext, UpdateProfileRequest request)
    {
        var user = await userManager.GetUserAsync(httpContext.User);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        // Apply profile updates
        ApplyProfileUpdates(user, request);

        // Persist changes
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Results.BadRequest(new { errors });
        }

        var response = MapToResponse(user);
        return Results.Ok(response);
    }

    private static void ApplyProfileUpdates(AppIdentityUser user, UpdateProfileRequest request)
    {
        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.CulinaryTitle))
            user.CulinaryTitle = request.CulinaryTitle;

        if (!string.IsNullOrEmpty(request.Biography))
            user.Biography = request.Biography;

        if (!string.IsNullOrEmpty(request.Location))
            user.Location = request.Location;

        if (!string.IsNullOrEmpty(request.ProfilePhotoUrl))
            user.ProfilePhotoUrl = request.ProfilePhotoUrl;
    }

    private static GetProfileResponse MapToResponse(AppIdentityUser user)
    {
        return new GetProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FullName = user.FullName,
            CulinaryTitle = user.CulinaryTitle,
            Biography = user.Biography,
            Location = user.Location,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            CreatedAt = user.CreatedAt
        };
    }
}
