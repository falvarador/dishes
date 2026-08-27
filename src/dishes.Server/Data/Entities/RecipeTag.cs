using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace dishes.Server.Data.Entities;

public class RecipeTag
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    public ICollection<RecipeRecipeTag> Recipes { get; set; } = [];

    public RecipeTag()
    {
    }

    [SetsRequiredMembers]
    public RecipeTag(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
