using System.ComponentModel.DataAnnotations;

namespace dishes.Server.Features.Identity.UpdateProfile;

public class UpdateProfileRequest
{
    [MaxLength(255, ErrorMessage = "Full name cannot exceed 255 characters")]
    public string? FullName { get; set; }

    [MaxLength(100, ErrorMessage = "Culinary title cannot exceed 100 characters")]
    public string? CulinaryTitle { get; set; }

    [MaxLength(1000, ErrorMessage = "Biography cannot exceed 1000 characters")]
    public string? Biography { get; set; }

    [MaxLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
    public string? Location { get; set; }

    [MaxLength(500, ErrorMessage = "Profile photo URL cannot exceed 500 characters")]
    public string? ProfilePhotoUrl { get; set; }
}
