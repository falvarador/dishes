using System.Net;
using System.Net.Http.Json;
using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Recipes.RateRecipe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace dishes.Server.Tests.Recipes;

public class RecipeRatingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RecipeRatingTests(CustomWebApplicationFactory factory)
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

    private async Task ClearRatingsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.UserRecipeRatings.RemoveRange(context.UserRecipeRatings);
        await context.SaveChangesAsync();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task RateRecipe_SuccessfullyRates_WithValidRating(int rating)
    {
        await ClearRatingsAsync();
        var recipe = await SeedRecipeAsync();

        var request = new RateRecipeRequest { Rating = rating };
        var response = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/rate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RatingResponse>();
        Assert.NotNull(result);
        Assert.Equal(rating, result!.Rating);
        Assert.Equal(recipe.Id, result.RecipeId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(10)]
    public async Task RateRecipe_ReturnsBadRequest_WithInvalidRating(int invalidRating)
    {
        var recipe = await SeedRecipeAsync();

        var request = new RateRecipeRequest { Rating = invalidRating };
        var response = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/rate", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RateRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        var fakeRecipeId = Guid.NewGuid();
        var request = new RateRecipeRequest { Rating = 5 };
        var response = await _client.PostAsJsonAsync($"/api/recipes/{fakeRecipeId}/rate", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RateRecipe_UpdatesExistingRating_WhenUserRatesAgain()
    {
        await ClearRatingsAsync();
        var recipe = await SeedRecipeAsync();

        // First rating: 3 stars
        var request1 = new RateRecipeRequest { Rating = 3 };
        var response1 = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/rate", request1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var rating1 = await response1.Content.ReadFromJsonAsync<RatingResponse>();

        // Second rating: 5 stars (should update)
        var request2 = new RateRecipeRequest { Rating = 5 };
        var response2 = await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/rate", request2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var rating2 = await response2.Content.ReadFromJsonAsync<RatingResponse>();

        Assert.NotNull(rating2);
        Assert.Equal(5, rating2!.Rating);
        Assert.True(rating2.UpdatedAt >= rating1!.UpdatedAt);
    }

    [Fact]
    public async Task UnrateRecipe_SuccessfullyRemovesRating()
    {
        await ClearRatingsAsync();
        var recipe = await SeedRecipeAsync();

        // Add a rating
        var rateRequest = new RateRecipeRequest { Rating = 4 };
        await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/rate", rateRequest);

        // Remove the rating
        var response = await _client.DeleteAsync($"/api/recipes/{recipe.Id}/rate");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UnrateRecipe_ReturnsNotFound_WhenNoRatingExists()
    {
        await ClearRatingsAsync();
        var recipe = await SeedRecipeAsync();

        var response = await _client.DeleteAsync($"/api/recipes/{recipe.Id}/rate");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnrateRecipe_ReturnsNotFound_WhenRecipeDoesNotExist()
    {
        var fakeRecipeId = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/recipes/{fakeRecipeId}/rate");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RateRecipe_PersistsToDatabase()
    {
        await ClearRatingsAsync();
        var recipe = await SeedRecipeAsync();

        var request = new RateRecipeRequest { Rating = 4 };
        await _client.PostAsJsonAsync($"/api/recipes/{recipe.Id}/rate", request);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedRating = await context.UserRecipeRatings
            .FirstOrDefaultAsync(r => r.RecipeId == recipe.Id);

        Assert.NotNull(savedRating);
        Assert.Equal(4, savedRating!.Rating);
    }
}
