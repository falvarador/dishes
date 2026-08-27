using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace dishes.Server.Data.Entities;

public enum RecipeStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public class Recipe
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(5000)]
    public required string Description { get; set; }

    [MaxLength(500)]
    public string? CoverPhotoPath { get; set; }

    [Required]
    [MaxLength(50)]
    public required string PrepTime { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Difficulty { get; set; }

    public Guid CreatorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAt { get; set; }

    public RecipeStatus Status { get; set; } = RecipeStatus.Draft;

    public ICollection<RecipeIngredient> Ingredients { get; set; } = [];
    public ICollection<RecipeInstruction> Instructions { get; set; } = [];
    public ICollection<RecipeRecipeCategory> Categories { get; set; } = [];
    public ICollection<RecipeRecipeTag> Tags { get; set; } = [];
    public ICollection<UserRecipeRating> Ratings { get; set; } = [];
    public ICollection<UserRecipeFavorite> Favorites { get; set; } = [];

    public Recipe()
    {
    }

    [SetsRequiredMembers]
    public Recipe(Guid id, string title, string description, string prepTime, string difficulty, Guid creatorId)
    {
        Id = id;
        Title = title;
        Description = description;
        PrepTime = prepTime;
        Difficulty = difficulty;
        CreatorId = creatorId;
    }
}
