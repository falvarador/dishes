using System.Net;
using System.Net.Http.Json;
using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Recipes.FavoriteRecipe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace dishes.Server.Tests.Recipes;

public class RecipeFavoriteTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RecipeFavoriteTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<Recipe> SeedRecipeAsync(string title = "Test Recipe")
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var recipe = new Recipe(Guid.NewGuid(), title, "Test description", "30 mins", "Easy", Guid.NewGuid());
        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();
        return recipe;
    }

    private async Task ClearFavoritesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.UserRecipeFavorites.RemoveRange(context.UserRecipeFavorites);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task FavoriteRecipe_SuccessfullyAddsToFavorites()
    {
        await ClearFavoritesAsync();
        var recipe = await SeedRecipeAsync();

        var response = await _client.PostAsync($"/api/recipes/{recipe.Id}/favorite", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<FavoriteResponse>();
        Assert.NotNull(result);
        Assert.Equal(recipe.Id, result!.RecipeId);
    }

    [Fact]
    public async Task FavoriteRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        var fakeRecipeId = Guid.NewGuid();
        var response = await _client.PostAsync($"/api/recipes/{fakeRecipeId}/favorite", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FavoriteRecipe_ReturnsConflict_WhenAlreadyFavorited()
    {
        await ClearFavoritesAsync();
        var recipe = await SeedRecipeAsync();

        // Add to favorites first time
        var response1 = await _client.PostAsync($"/api/recipes/{recipe.Id}/favorite", null);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        // Try to add again
        var response2 = await _client.PostAsync($"/api/recipes/{recipe.Id}/favorite", null);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }

    [Fact]
    public async Task UnfavoriteRecipe_SuccessfullyRemovesFromFavorites()
    {
        await ClearFavoritesAsync();
        var recipe = await SeedRecipeAsync();

        // Add to favorites
        await _client.PostAsync($"/api/recipes/{recipe.Id}/favorite", null);

        // Remove from favorites
        var response = await _client.DeleteAsync($"/api/recipes/{recipe.Id}/favorite");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UnfavoriteRecipe_ReturnsNotFound_WhenNotInFavorites()
    {
        await ClearFavoritesAsync();
        var recipe = await SeedRecipeAsync();

        var response = await _client.DeleteAsync($"/api/recipes/{recipe.Id}/favorite");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnfavoriteRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        var fakeRecipeId = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/recipes/{fakeRecipeId}/favorite");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FavoriteRecipe_PersistsToDatabase()
    {
        await ClearFavoritesAsync();
        var recipe = await SeedRecipeAsync();

        await _client.PostAsync($"/api/recipes/{recipe.Id}/favorite", null);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedFavorite = await context.UserRecipeFavorites
            .FirstOrDefaultAsync(f => f.RecipeId == recipe.Id);

        Assert.NotNull(savedFavorite);
        Assert.Equal(recipe.Id, savedFavorite!.RecipeId);
    }

    [Fact]
    public async Task UnfavoriteRecipe_RemovesFromDatabase()
    {
        await ClearFavoritesAsync();
        var recipe = await SeedRecipeAsync();

        // Add to favorites
        await _client.PostAsync($"/api/recipes/{recipe.Id}/favorite", null);

        // Remove from favorites
        await _client.DeleteAsync($"/api/recipes/{recipe.Id}/favorite");

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedFavorite = await context.UserRecipeFavorites
            .FirstOrDefaultAsync(f => f.RecipeId == recipe.Id);

        Assert.Null(savedFavorite);
    }

    [Fact]
    public async Task MultipleFavorites_CanBeMaintainedSeparately()
    {
        await ClearFavoritesAsync();
        var recipe1 = await SeedRecipeAsync("Recipe 1");
        var recipe2 = await SeedRecipeAsync("Recipe 2");

        // Favorite both recipes
        var response1 = await _client.PostAsync($"/api/recipes/{recipe1.Id}/favorite", null);
        var response2 = await _client.PostAsync($"/api/recipes/{recipe2.Id}/favorite", null);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var favorites = await context.UserRecipeFavorites.ToListAsync();

        Assert.Equal(2, favorites.Count);
    }
}
