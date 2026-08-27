using System.Net;
using System.Net.Http.Json;
using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Ingredients;
using Microsoft.Extensions.DependencyInjection;

namespace dishes.Server.Tests.Ingredients;

public class IngredientsCrudTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public IngredientsCrudTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task ClearIngredientsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Ingredients.RemoveRange(context.Ingredients);
        await context.SaveChangesAsync();
    }

    private async Task<Ingredient> SeedIngredientAsync(string name = "Seed Ingredient")
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ingredient = new Ingredient { Id = Guid.NewGuid(), Name = name };
        context.Ingredients.Add(ingredient);
        await context.SaveChangesAsync();
        return ingredient;
    }

    [Fact]
    public async Task GetIngredients_ReturnsEmptyList_WhenNoIngredientsExist()
    {
        await ClearIngredientsAsync();

        var response = await _client.GetAsync("/api/ingredients");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var ingredients = await response.Content.ReadFromJsonAsync<List<IngredientResponse>>();
        Assert.NotNull(ingredients);
        Assert.Empty(ingredients);
    }

    [Fact]
    public async Task GetIngredients_ReturnsSeededIngredients()
    {
        await ClearIngredientsAsync();
        var ingredient = await SeedIngredientAsync("Tomato");

        var response = await _client.GetAsync("/api/ingredients");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var ingredients = await response.Content.ReadFromJsonAsync<List<IngredientResponse>>();
        Assert.NotNull(ingredients);
        Assert.Contains(ingredients, i => i.Id == ingredient.Id && i.Name == "Tomato");
    }

    [Fact]
    public async Task GetIngredientById_ReturnsIngredient_WhenExists()
    {
        await ClearIngredientsAsync();
        var ingredient = await SeedIngredientAsync("Basil");

        var response = await _client.GetAsync($"/api/ingredients/{ingredient.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IngredientResponse>();
        Assert.NotNull(result);
        Assert.Equal("Basil", result!.Name);
    }

    [Fact]
    public async Task GetIngredientById_ReturnsNotFound_WhenIngredientDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/ingredients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateIngredient_ReturnsCreated_WhenValid()
    {
        var response = await _client.PostAsJsonAsync("/api/ingredients", new { name = "Garlic" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Ingredient>();
        Assert.NotNull(created);
        Assert.Equal("Garlic", created!.Name);
        Assert.NotEqual(Guid.Empty, created.Id);
    }

    [Fact]
    public async Task CreateIngredient_ReturnsBadRequest_WhenNameIsMissing()
    {
        var response = await _client.PostAsJsonAsync("/api/ingredients", new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateIngredient_ReturnsBadRequest_WhenNameExceedsMaxLength()
    {
        var response = await _client.PostAsJsonAsync("/api/ingredients", new { name = new string('a', 201) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateIngredient_ReturnsOk_WhenValid()
    {
        var ingredient = await SeedIngredientAsync("Old Name");

        var response = await _client.PutAsJsonAsync($"/api/ingredients/{ingredient.Id}", new { name = "New Name" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<Ingredient>();
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated!.Name);
    }

    [Fact]
    public async Task UpdateIngredient_ReturnsNotFound_WhenIngredientDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/ingredients/{Guid.NewGuid()}", new { name = "Anything" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateIngredient_ReturnsBadRequest_WhenNameIsInvalid()
    {
        var ingredient = await SeedIngredientAsync("Valid Name");

        var response = await _client.PutAsJsonAsync($"/api/ingredients/{ingredient.Id}", new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteIngredient_ReturnsNoContent_WhenExists()
    {
        var ingredient = await SeedIngredientAsync("To Delete");

        var response = await _client.DeleteAsync($"/api/ingredients/{ingredient.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/ingredients/{ingredient.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteIngredient_ReturnsNotFound_WhenIngredientDoesNotExist()
    {
        var response = await _client.DeleteAsync($"/api/ingredients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
