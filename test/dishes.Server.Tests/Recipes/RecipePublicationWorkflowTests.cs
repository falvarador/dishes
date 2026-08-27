using System.Net;
using System.Net.Http.Json;
using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Recipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace dishes.Server.Tests.Recipes;

public class RecipePublicationWorkflowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RecipePublicationWorkflowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task ClearRecipesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.RecipeInstructions.RemoveRange(context.RecipeInstructions);
        context.RecipeIngredients.RemoveRange(context.RecipeIngredients);
        context.Recipes.RemoveRange(context.Recipes);
        context.RecipeCategories.RemoveRange(context.RecipeCategories);
        context.RecipeTags.RemoveRange(context.RecipeTags);
        await context.SaveChangesAsync();
    }

    private async Task<Recipe> SeedRecipeAsync(
        string title = "Seed Recipe",
        string description = "A test recipe",
        RecipeStatus status = RecipeStatus.Draft)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var recipe = new Recipe(
            Guid.NewGuid(),
            title,
            description,
            "30 mins",
            "Easy",
            Guid.NewGuid()
        )
        {
            Status = status
        };
        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();
        return recipe;
    }

    private async Task SeedRecipeWithIngredientsAndInstructionsAsync(Recipe recipe)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbRecipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipe.Id);

        if (dbRecipe is not null)
        {
            var ingredient = new RecipeIngredient(
                Guid.NewGuid(),
                recipe.Id,
                "Default Ingredient",
                1m,
                "cup"
            );
            context.RecipeIngredients.Add(ingredient);

            var instruction = new RecipeInstruction(
                Guid.NewGuid(),
                recipe.Id,
                1,
                "Default instruction"
            );
            context.RecipeInstructions.Add(instruction);

            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task PublishRecipe_SuccessfullyPublishes_WhenRecipeHasIngredientsAndInstructions()
    {
        await ClearRecipesAsync();
        var recipe = await SeedRecipeAsync("Test Recipe", "A recipe to publish");
        await SeedRecipeWithIngredientsAndInstructionsAsync(recipe);

        var response = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/publish", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(result);
        Assert.True(result!.IsPublished);

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbRecipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipe.Id);
        Assert.NotNull(dbRecipe);
        Assert.Equal(RecipeStatus.Published, dbRecipe!.Status);
        Assert.NotNull(dbRecipe.PublishedAt);
    }

    [Fact]
    public async Task PublishRecipe_ReturnsBadRequest_WhenRecipeIsAlreadyPublished()
    {
        await ClearRecipesAsync();
        var recipe = await SeedRecipeAsync("Already Published", "Test", RecipeStatus.Published);
        await SeedRecipeWithIngredientsAndInstructionsAsync(recipe);

        var response = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/publish", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Contains("already published", result!["error"].ToString()!.ToLower());
    }

    [Fact]
    public async Task PublishRecipe_ReturnsBadRequest_WhenRecipeHasNoIngredients()
    {
        await ClearRecipesAsync();
        var recipe = await SeedRecipeAsync("No Ingredients", "Test recipe");

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var instruction = new RecipeInstruction(Guid.NewGuid(), recipe.Id, 1, "Step 1");
        context.RecipeInstructions.Add(instruction);
        await context.SaveChangesAsync();

        var response = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/publish", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Contains("ingredient", result!["error"].ToString()!.ToLower());
    }

    [Fact]
    public async Task PublishRecipe_ReturnsBadRequest_WhenRecipeHasNoInstructions()
    {
        await ClearRecipesAsync();
        var recipe = await SeedRecipeAsync("No Instructions", "Test recipe");

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ingredient = new RecipeIngredient(Guid.NewGuid(), recipe.Id, "Ingredient", 1m, "cup");
        context.RecipeIngredients.Add(ingredient);
        await context.SaveChangesAsync();

        var response = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/publish", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Contains("instruction", result!["error"].ToString()!.ToLower());
    }

    [Fact]
    public async Task PublishRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync($"/api/recipes/{Guid.NewGuid()}/publish", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnpublishRecipe_SuccessfullyUnpublishes_WhenRecipeIsPublished()
    {
        await ClearRecipesAsync();
        var recipe = await SeedRecipeAsync("To Unpublish", "Test", RecipeStatus.Published);
        await SeedRecipeWithIngredientsAndInstructionsAsync(recipe);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbRecipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipe.Id);
        if (dbRecipe is not null)
        {
            dbRecipe.PublishedAt = DateTime.UtcNow.AddHours(-1);
            await context.SaveChangesAsync();
        }

        var response = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/unpublish", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(result);
        Assert.False(result!.IsPublished);

        // Verify in database
        using var scope2 = _factory.Services.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbRecipe2 = await context2.Recipes.FirstOrDefaultAsync(r => r.Id == recipe.Id);
        Assert.NotNull(dbRecipe2);
        Assert.Equal(RecipeStatus.Draft, dbRecipe2!.Status);
        Assert.Null(dbRecipe2.PublishedAt);
    }

    [Fact]
    public async Task UnpublishRecipe_ReturnsBadRequest_WhenRecipeIsAlreadyDraft()
    {
        await ClearRecipesAsync();
        var recipe = await SeedRecipeAsync("Already Draft", "Test", RecipeStatus.Draft);

        var response = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/unpublish", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Contains("draft", result!["error"].ToString()!.ToLower());
    }

    [Fact]
    public async Task UnpublishRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync($"/api/recipes/{Guid.NewGuid()}/unpublish", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublishedRecipes_ReturnsOnlyPublishedRecipes()
    {
        await ClearRecipesAsync();

        // Create mix of published and draft recipes
        var published1 = await SeedRecipeAsync("Published 1", "Test", RecipeStatus.Published);
        await SeedRecipeWithIngredientsAndInstructionsAsync(published1);

        var published2 = await SeedRecipeAsync("Published 2", "Test", RecipeStatus.Published);
        await SeedRecipeWithIngredientsAndInstructionsAsync(published2);

        var draft = await SeedRecipeAsync("Draft Recipe", "Test", RecipeStatus.Draft);
        await SeedRecipeWithIngredientsAndInstructionsAsync(draft);

        // Set published dates
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var p1 = await context.Recipes.FirstOrDefaultAsync(r => r.Id == published1.Id);
        if (p1 is not null) p1.PublishedAt = DateTime.UtcNow.AddHours(-2);
        var p2 = await context.Recipes.FirstOrDefaultAsync(r => r.Id == published2.Id);
        if (p2 is not null) p2.PublishedAt = DateTime.UtcNow.AddHours(-1);
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/recipes/published");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RecipeResponse>>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.True(result.All(r => r.IsPublished));
        Assert.DoesNotContain(result, r => r.Title == "Draft Recipe");
    }

    [Fact]
    public async Task GetPublishedRecipes_ReturnsEmptyList_WhenNoPublishedRecipes()
    {
        await ClearRecipesAsync();

        // Create only draft recipes
        var draft1 = await SeedRecipeAsync("Draft 1", "Test", RecipeStatus.Draft);
        var draft2 = await SeedRecipeAsync("Draft 2", "Test", RecipeStatus.Draft);

        var response = await _client.GetAsync("/api/recipes/published");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RecipeResponse>>();
        Assert.NotNull(result);
        Assert.Empty(result!);
    }

    [Fact]
    public async Task GetPublishedRecipes_ReturnsInDescendingPublishedAtOrder()
    {
        await ClearRecipesAsync();

        var recipe1 = await SeedRecipeAsync("Recipe 1", "Test", RecipeStatus.Published);
        await SeedRecipeWithIngredientsAndInstructionsAsync(recipe1);

        var recipe2 = await SeedRecipeAsync("Recipe 2", "Test", RecipeStatus.Published);
        await SeedRecipeWithIngredientsAndInstructionsAsync(recipe2);

        var recipe3 = await SeedRecipeAsync("Recipe 3", "Test", RecipeStatus.Published);
        await SeedRecipeWithIngredientsAndInstructionsAsync(recipe3);

        // Set published dates - recipe3 most recent, recipe1 oldest
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var r1 = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipe1.Id);
        if (r1 is not null) r1.PublishedAt = DateTime.UtcNow.AddHours(-3);
        var r2 = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipe2.Id);
        if (r2 is not null) r2.PublishedAt = DateTime.UtcNow.AddHours(-2);
        var r3 = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipe3.Id);
        if (r3 is not null) r3.PublishedAt = DateTime.UtcNow.AddHours(-1);
        await context.SaveChangesAsync();

        var response = await _client.GetAsync("/api/recipes/published");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RecipeResponse>>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.Count);
        Assert.Equal("Recipe 3", result[0].Title);
        Assert.Equal("Recipe 2", result[1].Title);
        Assert.Equal("Recipe 1", result[2].Title);
    }
}
