using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace dishes.Server.Data.Entities;

public class RecipeInstruction
{
    [Key]
    public Guid Id { get; set; }

    public Guid RecipeId { get; set; }

    [Required]
    public required int StepNumber { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Description { get; set; }

    public Recipe? Recipe { get; set; }

    public RecipeInstruction()
    {
    }

    [SetsRequiredMembers]
    public RecipeInstruction(Guid id, Guid recipeId, int stepNumber, string description)
    {
        Id = id;
        RecipeId = recipeId;
        StepNumber = stepNumber;
        Description = description;
    }
}
