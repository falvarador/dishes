namespace dishes.Server.Features.Identity.GetProfile;

public class GetProfileResponse
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CulinaryTitle { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
