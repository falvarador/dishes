using System.ComponentModel.DataAnnotations;

namespace dishes.Server.Features.Ingredients;

public record IngredientRequest(
    [property: Required(ErrorMessage = "Name is required.")]
    [property: MaxLength(200, ErrorMessage = "Name must be at most 200 characters.")]
    string Name);
