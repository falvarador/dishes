using System.ComponentModel.DataAnnotations;

namespace dishes.Server.Features.Account.AccountSettings;

/// <summary>
/// Response DTO for account settings retrieval.
/// Contains user profile information.
/// </summary>
public record AccountSettingsResponse(
    [property: Required]
    string Id,

    [property: Required]
    [property: EmailAddress]
    string Email,

    [property: Required]
    string UserName,

    [property: MaxLength(500)]
    string FullName,

    [property: MaxLength(200)]
    string CulinaryTitle,

    [property: MaxLength(2000)]
    string Biography,

    [property: MaxLength(200)]
    string Location,

    [property: MaxLength(500)]
    string ProfilePhotoUrl,

    DateTime CreatedAt
);

/// <summary>
/// Request DTO for updating account settings.
/// </summary>
public record UpdateAccountSettingsRequest(
    [property: Required(ErrorMessage = "Full name is required")]
    [property: MaxLength(500, ErrorMessage = "Full name must be at most 500 characters")]
    string FullName,

    [property: MaxLength(200, ErrorMessage = "Culinary title must be at most 200 characters")]
    string CulinaryTitle = "",

    [property: MaxLength(2000, ErrorMessage = "Biography must be at most 2000 characters")]
    string Biography = "",

    [property: MaxLength(200, ErrorMessage = "Location must be at most 200 characters")]
    string Location = "",

    [property: MaxLength(500, ErrorMessage = "Profile photo URL must be at most 500 characters")]
    [property: Url(ErrorMessage = "Profile photo URL must be a valid URL")]
    string ProfilePhotoUrl = ""
);
