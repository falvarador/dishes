using Microsoft.AspNetCore.Identity;

namespace dishes.Server.Data.Entities;

public class AppIdentityUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string CulinaryTitle { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppIdentityUser() { }

    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public AppIdentityUser(string userName) : base(userName)
    {
        FullName = string.Empty;
        CulinaryTitle = string.Empty;
        Biography = string.Empty;
        Location = string.Empty;
        ProfilePhotoUrl = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }
}
