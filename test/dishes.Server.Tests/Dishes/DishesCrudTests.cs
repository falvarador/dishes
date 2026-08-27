using System.Net;
using System.Net.Http.Json;
using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace dishes.Server.Tests.Dishes;

public class DishesCrudTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DishesCrudTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task ClearDishesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Dishes.RemoveRange(context.Dishes);
        await context.SaveChangesAsync();
    }

    private async Task<Dish> SeedDishAsync(string name = "Seed Dish")
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dish = new Dish(Guid.NewGuid(), name);
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();
        return dish;
    }

    [Fact]
    public async Task GetDishes_ReturnsEmptyList_WhenNoDishesExist()
    {
        await ClearDishesAsync();

        var response = await _client.GetAsync("/api/dishes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dishes = await response.Content.ReadFromJsonAsync<List<Dish>>();
        Assert.NotNull(dishes);
        Assert.Empty(dishes);
    }

    [Fact]
    public async Task GetDishes_ReturnsCreatedDishes()
    {
        await ClearDishesAsync();
        var dish = await SeedDishAsync("Lasagna");

        var response = await _client.GetAsync("/api/dishes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dishes = await response.Content.ReadFromJsonAsync<List<Dish>>();
        Assert.NotNull(dishes);
        Assert.Contains(dishes, d => d.Id == dish.Id && d.Name == "Lasagna");
    }

    [Fact]
    public async Task GetDishById_ReturnsDish_WhenDishExists()
    {
        await ClearDishesAsync();
        var dish = await SeedDishAsync("Paella");

        var response = await _client.GetAsync($"/api/dishes/{dish.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dish>();
        Assert.NotNull(result);
        Assert.Equal("Paella", result!.Name);
    }

    [Fact]
    public async Task GetDishById_ReturnsNotFound_WhenDishDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/dishes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_ReturnsCreated_WhenNameIsValid()
    {
        var response = await _client.PostAsJsonAsync("/api/dishes", new { name = "Tacos" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Dish>();
        Assert.NotNull(created);
        Assert.Equal("Tacos", created!.Name);
        Assert.NotEqual(Guid.Empty, created.Id);
    }

    [Fact]
    public async Task CreateDish_ReturnsBadRequest_WhenNameIsMissing()
    {
        var response = await _client.PostAsJsonAsync("/api/dishes", new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDish_ReturnsBadRequest_WhenNameExceedsMaxLength()
    {
        var response = await _client.PostAsJsonAsync("/api/dishes", new { name = new string('a', 201) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDish_ReturnsOk_WhenDishExistsAndNameIsValid()
    {
        var dish = await SeedDishAsync("Old Name");

        var response = await _client.PutAsJsonAsync($"/api/dishes/{dish.Id}", new { name = "New Name" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<Dish>();
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated!.Name);
    }

    [Fact]
    public async Task UpdateDish_ReturnsNotFound_WhenDishDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/dishes/{Guid.NewGuid()}", new { name = "Anything" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDish_ReturnsBadRequest_WhenNameIsInvalid()
    {
        var dish = await SeedDishAsync("Valid Name");

        var response = await _client.PutAsJsonAsync($"/api/dishes/{dish.Id}", new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDish_ReturnsNoContent_WhenDishExists()
    {
        var dish = await SeedDishAsync("To Delete");

        var response = await _client.DeleteAsync($"/api/dishes/{dish.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/dishes/{dish.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteDish_ReturnsNotFound_WhenDishDoesNotExist()
    {
        var response = await _client.DeleteAsync($"/api/dishes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
