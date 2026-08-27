using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace dishes.Server.Data.Entities;

public class RecipeCategory
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    public ICollection<RecipeRecipeCategory> Recipes { get; set; } = [];

    public RecipeCategory()
    {
    }

    [SetsRequiredMembers]
    public RecipeCategory(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
