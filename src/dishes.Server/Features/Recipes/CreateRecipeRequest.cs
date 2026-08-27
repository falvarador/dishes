using System.ComponentModel.DataAnnotations;

namespace dishes.Server.Features.Recipes;

public record CreateRecipeRequest(
    [property: Required(ErrorMessage = "Title is required.")]
    [property: MaxLength(500, ErrorMessage = "Title must be at most 500 characters.")]
    string Title,

    [property: Required(ErrorMessage = "Description is required.")]
    [property: MaxLength(5000, ErrorMessage = "Description must be at most 5000 characters.")]
    string Description,

    [property: MaxLength(500)]
    string? CoverPhotoPath,

    [property: Required(ErrorMessage = "Prep time is required.")]
    [property: MaxLength(50, ErrorMessage = "Prep time must be at most 50 characters.")]
    string PrepTime,

    [property: Required(ErrorMessage = "Difficulty is required.")]
    [property: MaxLength(50, ErrorMessage = "Difficulty must be at most 50 characters.")]
    string Difficulty,

    [property: Required(ErrorMessage = "Ingredients are required.")]
    ICollection<RecipeIngredientDto> Ingredients,

    [property: Required(ErrorMessage = "Instructions are required.")]
    ICollection<RecipeInstructionDto> Instructions,

    ICollection<Guid> CategoryIds = default!,

    ICollection<Guid> TagIds = default!,

    bool IsPublished = false
);

public record RecipeIngredientDto(
    [property: Required]
    [property: MaxLength(200)]
    string IngredientName,

    [property: Required]
    decimal Quantity,

    [property: Required]
    [property: MaxLength(50)]
    string Unit
);

public record RecipeInstructionDto(
    [property: Required]
    int StepNumber,

    [property: Required]
    [property: MaxLength(2000)]
    string Description
);
