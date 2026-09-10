using System.Net;
using System.Net.Http.Json;
using dishes.Server.Data;
using dishes.Server.Data.Entities;
using dishes.Server.Features.Recipes.GetRecipesList;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace dishes.Server.Tests.Recipes;

public class GetRecipesListTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetRecipesListTests(CustomWebApplicationFactory factory)
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
        string title = "Test Recipe",
        string description = "A test recipe",
        string difficulty = "Easy",
        string prepTime = "30 mins",
        RecipeStatus status = RecipeStatus.Published,
        Guid? creatorId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var recipe = new Recipe(
            Guid.NewGuid(),
            title,
            description,
            prepTime,
            difficulty,
            creatorId ?? Guid.NewGuid()
        )
        {
            Status = status
        };
        context.Recipes.Add(recipe);
        await context.SaveChangesAsync();
        return recipe;
    }

    [Fact]
    public async Task GetRecipesList_ReturnsAllRecipes_WhenNoFiltersApplied()
    {
        await ClearRecipesAsync();

        // Create test recipes
        await SeedRecipeAsync("Recipe 1", "Test 1");
        await SeedRecipeAsync("Recipe 2", "Test 2");
        await SeedRecipeAsync("Recipe 3", "Test 3");

        var response = await _client.GetAsync("/api/recipes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount);
        Assert.Equal(3, result.Data.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetRecipesList_ReturnsEmptyList_WhenNoRecipesExist()
    {
        await ClearRecipesAsync();

        var response = await _client.GetAsync("/api/recipes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(0, result!.TotalCount);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetRecipesList_FiltersByStatus_Published()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Published Recipe 1", "Test", status: RecipeStatus.Published);
        await SeedRecipeAsync("Published Recipe 2", "Test", status: RecipeStatus.Published);
        await SeedRecipeAsync("Draft Recipe", "Test", status: RecipeStatus.Draft);

        var response = await _client.GetAsync("/api/recipes?status=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalCount);
        Assert.All(result.Data, r => Assert.True(r.IsPublished));
    }

    [Fact]
    public async Task GetRecipesList_FiltersByStatus_Draft()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Published Recipe", "Test", status: RecipeStatus.Published);
        await SeedRecipeAsync("Draft Recipe 1", "Test", status: RecipeStatus.Draft);
        await SeedRecipeAsync("Draft Recipe 2", "Test", status: RecipeStatus.Draft);

        var response = await _client.GetAsync("/api/recipes?status=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalCount);
        Assert.All(result.Data, r => Assert.False(r.IsPublished));
    }

    [Fact]
    public async Task GetRecipesList_FiltersByDifficulty()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Easy Recipe 1", "Test", difficulty: "Easy");
        await SeedRecipeAsync("Easy Recipe 2", "Test", difficulty: "Easy");
        await SeedRecipeAsync("Hard Recipe", "Test", difficulty: "Hard");

        var response = await _client.GetAsync("/api/recipes?difficulty=Easy");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalCount);
        Assert.All(result.Data, r => Assert.Equal("Easy", r.Difficulty));
    }

    [Fact]
    public async Task GetRecipesList_FiltersByPrepTime()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Quick Recipe 1", "Test", prepTime: "15 mins");
        await SeedRecipeAsync("Quick Recipe 2", "Test", prepTime: "15 mins");
        await SeedRecipeAsync("Long Recipe", "Test", prepTime: "2 hours");

        var response = await _client.GetAsync("/api/recipes?prepTime=15%20mins");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalCount);
    }

    [Fact]
    public async Task GetRecipesList_FiltersByCreatorId()
    {
        await ClearRecipesAsync();

        var creator1 = Guid.NewGuid();
        var creator2 = Guid.NewGuid();

        await SeedRecipeAsync("Creator 1 Recipe 1", "Test", creatorId: creator1);
        await SeedRecipeAsync("Creator 1 Recipe 2", "Test", creatorId: creator1);
        await SeedRecipeAsync("Creator 2 Recipe", "Test", creatorId: creator2);

        var response = await _client.GetAsync($"/api/recipes?creatorId={creator1}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalCount);
        Assert.All(result.Data, r => Assert.Equal(creator1, r.CreatorId));
    }

    [Fact]
    public async Task GetRecipesList_SearchesByTitle()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Pasta Carbonara", "Classic Italian pasta");
        await SeedRecipeAsync("Pasta Primavera", "Light spring pasta");
        await SeedRecipeAsync("Chocolate Cake", "Sweet dessert");

        var response = await _client.GetAsync("/api/recipes?search=pasta");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalCount);
        Assert.All(result.Data, r => Assert.Contains("Pasta", r.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetRecipesList_SearchesByIngredientName()
    {
        await ClearRecipesAsync();

        var recipe1 = await SeedRecipeAsync("Pasta Carbonara", "Classic Italian dish");
        var recipe2 = await SeedRecipeAsync("Beef Stew", "Hearty beef and vegetable stew");
        await SeedRecipeAsync("Chocolate Cake", "Sweet chocolate dessert");

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.RecipeIngredients.Add(new RecipeIngredient(Guid.NewGuid(), recipe1.Id, "Pasta", 1m, "cup"));
            context.RecipeIngredients.Add(new RecipeIngredient(Guid.NewGuid(), recipe2.Id, "Whole Wheat Pasta", 1m, "cup"));
            await context.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/recipes?search=pasta");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalCount);
    }

    [Fact]
    public async Task GetRecipesList_SearchesByTitleOrIngredient_WithoutDuplicates()
    {
        await ClearRecipesAsync();

        var titleMatch = await SeedRecipeAsync("Pasta Bake", "Baked dish");
        var ingredientMatch = await SeedRecipeAsync("Tomato Soup", "Simple soup");
        var bothMatch = await SeedRecipeAsync("Pasta Salad", "Fresh salad");

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.RecipeIngredients.Add(new RecipeIngredient(Guid.NewGuid(), ingredientMatch.Id, "Pasta", 1m, "cup"));
            context.RecipeIngredients.Add(new RecipeIngredient(Guid.NewGuid(), bothMatch.Id, "Pasta", 1m, "cup"));
            await context.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/recipes?search=pasta");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount);
        Assert.Equal(3, result.Data.Select(r => r.Id).Distinct().Count());
    }

    [Fact]
    public async Task GetRecipesList_SearchWithWhitespaceOnly_ReturnsUnfilteredCatalog()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Recipe 1", "Test 1");
        await SeedRecipeAsync("Recipe 2", "Test 2");
        await SeedRecipeAsync("Recipe 3", "Test 3");

        var response = await _client.GetAsync("/api/recipes?search=%20%20%20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount);
        Assert.Equal(3, result.Data.Count);
    }

    [Fact]
    public async Task GetRecipesList_Pagination_FirstPage()
    {
        await ClearRecipesAsync();

        for (int i = 1; i <= 25; i++)
        {
            await SeedRecipeAsync($"Recipe {i}", "Test");
        }

        var response = await _client.GetAsync("/api/recipes?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(25, result!.TotalCount);
        Assert.Equal(10, result.Data.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetRecipesList_Pagination_MiddlePage()
    {
        await ClearRecipesAsync();

        for (int i = 1; i <= 25; i++)
        {
            await SeedRecipeAsync($"Recipe {i}", "Test");
        }

        var response = await _client.GetAsync("/api/recipes?page=2&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(25, result!.TotalCount);
        Assert.Equal(10, result.Data.Count);
        Assert.Equal(2, result.Page);
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetRecipesList_Pagination_LastPage()
    {
        await ClearRecipesAsync();

        for (int i = 1; i <= 25; i++)
        {
            await SeedRecipeAsync($"Recipe {i}", "Test");
        }

        var response = await _client.GetAsync("/api/recipes?page=3&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(25, result!.TotalCount);
        Assert.Equal(5, result.Data.Count);
        Assert.Equal(3, result.Page);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetRecipesList_Sort_ByTitleAscending()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Zebra Cake", "Test");
        await SeedRecipeAsync("Apple Pie", "Test");
        await SeedRecipeAsync("Banana Bread", "Test");

        var response = await _client.GetAsync("/api/recipes?sortBy=title&sortOrder=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount);
        Assert.Equal("Apple Pie", result.Data[0].Title);
        Assert.Equal("Banana Bread", result.Data[1].Title);
        Assert.Equal("Zebra Cake", result.Data[2].Title);
    }

    [Fact]
    public async Task GetRecipesList_Sort_ByTitleDescending()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Zebra Cake", "Test");
        await SeedRecipeAsync("Apple Pie", "Test");
        await SeedRecipeAsync("Banana Bread", "Test");

        var response = await _client.GetAsync("/api/recipes?sortBy=title&sortOrder=desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount);
        Assert.Equal("Zebra Cake", result.Data[0].Title);
        Assert.Equal("Banana Bread", result.Data[1].Title);
        Assert.Equal("Apple Pie", result.Data[2].Title);
    }

    [Fact]
    public async Task GetRecipesList_Sort_ByDifficulty()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Hard Recipe", "Test", difficulty: "Hard");
        await SeedRecipeAsync("Easy Recipe", "Test", difficulty: "Easy");
        await SeedRecipeAsync("Medium Recipe", "Test", difficulty: "Medium");

        var response = await _client.GetAsync("/api/recipes?sortBy=difficulty&sortOrder=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount);
        Assert.Equal("Easy", result.Data[0].Difficulty);
        Assert.Equal("Hard", result.Data[1].Difficulty);
        Assert.Equal("Medium", result.Data[2].Difficulty);
    }

    [Fact]
    public async Task GetRecipesList_DefaultSort_ByCreatedAtDescending()
    {
        await ClearRecipesAsync();

        var recipe1 = await SeedRecipeAsync("Recipe 1", "Test");
        await Task.Delay(10); // Small delay to ensure different timestamps
        var recipe2 = await SeedRecipeAsync("Recipe 2", "Test");
        await Task.Delay(10);
        var recipe3 = await SeedRecipeAsync("Recipe 3", "Test");

        var response = await _client.GetAsync("/api/recipes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount);
        // Should be in reverse order (most recent first)
        Assert.True(result.Data[0].CreatedAt >= result.Data[1].CreatedAt);
        Assert.True(result.Data[1].CreatedAt >= result.Data[2].CreatedAt);
    }

    [Fact]
    public async Task GetRecipesList_CombinesMultipleFilters()
    {
        await ClearRecipesAsync();

        var creator1 = Guid.NewGuid();
        var creator2 = Guid.NewGuid();

        await SeedRecipeAsync("Easy Pasta 1", "Test", difficulty: "Easy", creatorId: creator1, status: RecipeStatus.Published);
        await SeedRecipeAsync("Easy Pasta 2", "Test", difficulty: "Easy", creatorId: creator1, status: RecipeStatus.Published);
        await SeedRecipeAsync("Hard Pasta", "Test", difficulty: "Hard", creatorId: creator1, status: RecipeStatus.Published);
        await SeedRecipeAsync("Easy Cake", "Test", difficulty: "Easy", creatorId: creator2, status: RecipeStatus.Published);

        var response = await _client.GetAsync($"/api/recipes?creatorId={creator1}&difficulty=Easy&status=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.TotalCount);
        Assert.All(result.Data, r => Assert.Equal(creator1, r.CreatorId));
        Assert.All(result.Data, r => Assert.Equal("Easy", r.Difficulty));
        Assert.All(result.Data, r => Assert.True(r.IsPublished));
    }

    [Fact]
    public async Task GetRecipesList_HandlesPaginationBoundary_PageSizeOfOne()
    {
        await ClearRecipesAsync();

        await SeedRecipeAsync("Recipe 1", "Test");
        await SeedRecipeAsync("Recipe 2", "Test");
        await SeedRecipeAsync("Recipe 3", "Test");

        var response = await _client.GetAsync("/api/recipes?pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount);
        Assert.Single(result.Data);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetRecipesList_CapsPageSize_At100()
    {
        await ClearRecipesAsync();

        for (int i = 0; i < 150; i++)
        {
            await SeedRecipeAsync($"Recipe {i}", "Test");
        }

        var response = await _client.GetAsync("/api/recipes?pageSize=500");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(100, result!.PageSize);
        Assert.Equal(100, result.Data.Count);
    }

    [Fact]
    public async Task GetRecipesList_WithLimitAndOffset_ReturnsCorrectPage()
    {
        await ClearRecipesAsync();

        // Create 25 test recipes
        for (int i = 1; i <= 25; i++)
        {
            await SeedRecipeAsync($"Recipe {i:D2}", $"Description {i}");
        }

        // Request with limit=10, offset=0 (first page)
        var response = await _client.GetAsync("/api/recipes?limit=10&offset=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(25, result!.TotalCount);
        Assert.Equal(10, result.Data.Count);
        Assert.NotNull(result.Pagination);
        Assert.Equal(25, result.Pagination.Total);
        Assert.Equal(10, result.Pagination.Limit);
        Assert.Equal(0, result.Pagination.Offset);
        Assert.True(result.Pagination.HasMore); // offset + limit (10) < total (25)
    }

    [Fact]
    public async Task GetRecipesList_WithLimitAndOffset_SecondPage()
    {
        await ClearRecipesAsync();

        // Create 25 test recipes
        for (int i = 1; i <= 25; i++)
        {
            await SeedRecipeAsync($"Recipe {i:D2}", $"Description {i}");
        }

        // Request with limit=10, offset=10 (second page)
        var response = await _client.GetAsync("/api/recipes?limit=10&offset=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(25, result!.TotalCount);
        Assert.Equal(10, result.Data.Count);
        Assert.NotNull(result.Pagination);
        Assert.Equal(10, result.Pagination.Offset);
        Assert.True(result.Pagination.HasMore); // offset + limit (20) < total (25)
    }

    [Fact]
    public async Task GetRecipesList_WithLimitAndOffset_LastPage()
    {
        await ClearRecipesAsync();

        // Create 25 test recipes
        for (int i = 1; i <= 25; i++)
        {
            await SeedRecipeAsync($"Recipe {i:D2}", $"Description {i}");
        }

        // Request with limit=10, offset=20 (last page with 5 items)
        var response = await _client.GetAsync("/api/recipes?limit=10&offset=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(25, result!.TotalCount);
        Assert.Equal(5, result.Data.Count); // Only 5 items left
        Assert.NotNull(result.Pagination);
        Assert.Equal(20, result.Pagination.Offset);
        Assert.False(result.Pagination.HasMore); // offset + limit (30) >= total (25)
    }

    [Fact]
    public async Task GetRecipesList_WithLimitAndOffset_AndFilter()
    {
        await ClearRecipesAsync();

        // Create 15 Easy recipes and 10 Hard recipes
        for (int i = 1; i <= 15; i++)
        {
            await SeedRecipeAsync($"Easy Recipe {i}", "Easy cooking", difficulty: "Easy");
        }
        for (int i = 1; i <= 10; i++)
        {
            await SeedRecipeAsync($"Hard Recipe {i}", "Hard cooking", difficulty: "Hard");
        }

        // Request with difficulty filter and pagination
        var response = await _client.GetAsync("/api/recipes?difficulty=Easy&limit=5&offset=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(15, result!.TotalCount); // Only Easy recipes count
        Assert.Equal(5, result.Data.Count);
        Assert.All(result.Data, recipe => Assert.Contains("Easy Recipe", recipe.Title));
        Assert.NotNull(result.Pagination);
        Assert.Equal(15, result.Pagination.Total);
        Assert.True(result.Pagination.HasMore);
    }

    [Fact]
    public async Task GetRecipesList_DefaultLimitIsUsed_WhenOffsetProvided()
    {
        await ClearRecipesAsync();

        // Create 20 recipes
        for (int i = 1; i <= 20; i++)
        {
            await SeedRecipeAsync($"Recipe {i}", "Test");
        }

        // Request with offset but no limit (should use default limit of 12)
        var response = await _client.GetAsync("/api/recipes?offset=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedRecipesResponse>();
        Assert.NotNull(result);
        Assert.Equal(20, result!.TotalCount);
        Assert.Equal(12, result.Data.Count); // Default limit
        Assert.NotNull(result.Pagination);
        Assert.Equal(12, result.Pagination.Limit);
    }
}

