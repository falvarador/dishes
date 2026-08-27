using System.ComponentModel.DataAnnotations;

namespace dishes.Server.Features.Dishes;

public record DishRequest(
    [property: Required(ErrorMessage = "Name is required.")]
    [property: MaxLength(200, ErrorMessage = "Name must be at most 200 characters.")]
    string Name);
