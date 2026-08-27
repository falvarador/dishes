using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace dishes.Server.Data.Entities;

public class RecipeIngredient
{
    [Key]
    public Guid Id { get; set; }

    public Guid RecipeId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string IngredientName { get; set; }

    [Required]
    public required decimal Quantity { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Unit { get; set; }

    public Recipe? Recipe { get; set; }

    public RecipeIngredient()
    {
    }

    [SetsRequiredMembers]
    public RecipeIngredient(Guid id, Guid recipeId, string ingredientName, decimal quantity, string unit)
    {
        Id = id;
        RecipeId = recipeId;
        IngredientName = ingredientName;
        Quantity = quantity;
        Unit = unit;
    }
}
