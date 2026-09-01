using dishes.Server.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace dishes.Server.Features.Identity.GetProfile;

public class GetProfileHandler(UserManager<AppIdentityUser> userManager)
{
    public async Task<IResult> HandleAsync(HttpContext httpContext)
    {
        var user = await userManager.GetUserAsync(httpContext.User);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var response = MapToResponse(user);
        return Results.Ok(response);
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
