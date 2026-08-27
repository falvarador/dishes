namespace dishes.Server.Features.Recipes;

public record RecipeResponse(
    Guid Id,
    string Title,
    string Description,
    string? CoverPhotoPath,
    string PrepTime,
    string Difficulty,
    Guid CreatorId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ICollection<RecipeIngredientResponse> Ingredients,
    ICollection<RecipeInstructionResponse> Instructions,
    ICollection<RecipeCategoryResponse> Categories,
    ICollection<RecipeTagResponse> Tags,
    bool IsPublished
);

public record RecipeIngredientResponse(
    Guid Id,
    string IngredientName,
    decimal Quantity,
    string Unit
);

public record RecipeInstructionResponse(
    Guid Id,
    int StepNumber,
    string Description
);

public record RecipeCategoryResponse(
    Guid Id,
    string Name
);

public record RecipeTagResponse(
    Guid Id,
    string Name
);

public record RecipeDetailResponse(
    Guid Id,
    string Title,
    string Description,
    string? CoverPhotoPath,
    string PrepTime,
    string Difficulty,
    Guid CreatorId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ICollection<RecipeIngredientResponse> Ingredients,
    ICollection<RecipeInstructionResponse> Instructions,
    ICollection<RecipeCategoryResponse> Categories,
    ICollection<RecipeTagResponse> Tags,
    bool IsPublished
);
