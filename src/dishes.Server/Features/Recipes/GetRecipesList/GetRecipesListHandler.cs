using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Recipes.GetRecipesList;

public static class GetRecipesListHandler
{
    private static async Task<IResult> HandleAsync(
        [FromQuery] int? status,
        [FromQuery] string? difficulty,
        [FromQuery] string? prepTime,
        [FromQuery] Guid? creatorId,
        [FromQuery] string? search,
        [FromQuery] string? categories,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] string? sortOrder = "desc",
        AppDbContext context = default!,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;
        if (string.IsNullOrWhiteSpace(sortOrder) || (sortOrder != "asc" && sortOrder != "desc"))
            sortOrder = "desc";

        // Build the base query
        IQueryable<Recipe> query = context.Recipes;

        // Only include categories if we need to filter by them
        var hasCategoryFilter = !string.IsNullOrWhiteSpace(categories);
        if (hasCategoryFilter)
        {
            query = query
                .Include(r => r.Categories)
                .ThenInclude(rc => rc.Category);
        }

        // Apply filters
        if (status.HasValue)
        {
            query = query.Where(r => (int)r.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            query = query.Where(r => r.Difficulty == difficulty);
        }

        if (!string.IsNullOrWhiteSpace(prepTime))
        {
            query = query.Where(r => r.PrepTime == prepTime);
        }

        if (creatorId.HasValue)
        {
            query = query.Where(r => r.CreatorId == creatorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(r =>
                r.Title.ToLower().Contains(searchTerm.ToLower()) ||
                r.Ingredients.Any(i => i.IngredientName.ToLower().Contains(searchTerm.ToLower())));
        }

        // Filter by categories (comma-separated values)
        if (hasCategoryFilter)
        {
            var categoryNames = categories.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .ToList();

            if (categoryNames.Count > 0)
            {
                query = query.Where(r => r.Categories.Any(rc =>
                    categoryNames.Contains(rc.Category!.Name)));
            }
        }

        // Materialize and deduplicate in memory to avoid EF Core issues with Distinct() over includes
        var allRecipes = await query
            .Include(r => r.Ratings)
            .ToListAsync(cancellationToken);

        // Remove duplicate recipes from N:M relationships (only happens if categories were included)
        var distinctRecipes = allRecipes.DistinctBy(r => r.Id).ToList();

        // Get total count after deduplication
        var totalCount = distinctRecipes.Count;

        // Apply sorting in memory
        var sortedRecipes = ApplySortingInMemory(distinctRecipes, sortBy, sortOrder);

        // Apply pagination in memory
        var recipes = sortedRecipes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Map to summary responses with rating aggregates
        var data = recipes.Select(r =>
        {
            var averageRating = r.Ratings.Any() ? Math.Round(r.Ratings.Average(x => x.Rating), 2) : 0.0;
            return new RecipeSummaryResponse(
                r.Id,
                r.Title,
                r.Difficulty,
                r.CoverPhotoPath,
                r.CreatorId,
                r.CreatedAt,
                r.Status == RecipeStatus.Published,
                averageRating,
                r.Ratings.Count
            );
        }).ToList();

        var response = new PaginatedRecipesResponse(data, totalCount, page, pageSize);
        return Results.Ok(response);
    }

    private static IQueryable<Recipe> ApplySorting(
        IQueryable<Recipe> query,
        string? sortBy,
        string? sortOrder)
    {
        var sortField = sortBy?.ToLower() ?? "createdat";
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortField switch
        {
            "title" => isDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
            "difficulty" => isDescending
                ? query.OrderByDescending(r => r.Difficulty)
                : query.OrderBy(r => r.Difficulty),
            "preptime" => isDescending
                ? query.OrderByDescending(r => r.PrepTime)
                : query.OrderBy(r => r.PrepTime),
            "createdat" or "created" => isDescending
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt),
            "updatedat" or "updated" => isDescending
                ? query.OrderByDescending(r => r.UpdatedAt)
                : query.OrderBy(r => r.UpdatedAt),
            "publishedat" or "published" => isDescending
                ? query.OrderByDescending(r => r.PublishedAt)
                : query.OrderBy(r => r.PublishedAt),
            "status" => isDescending
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            _ => query.OrderByDescending(r => r.CreatedAt) // Default sorting
        };
    }

    private static List<Recipe> ApplySortingInMemory(
        List<Recipe> recipes,
        string? sortBy,
        string? sortOrder)
    {
        var sortField = sortBy?.ToLower() ?? "createdat";
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortField switch
        {
            "title" => isDescending
                ? recipes.OrderByDescending(r => r.Title).ToList()
                : recipes.OrderBy(r => r.Title).ToList(),
            "difficulty" => isDescending
                ? recipes.OrderByDescending(r => r.Difficulty).ToList()
                : recipes.OrderBy(r => r.Difficulty).ToList(),
            "preptime" => isDescending
                ? recipes.OrderByDescending(r => r.PrepTime).ToList()
                : recipes.OrderBy(r => r.PrepTime).ToList(),
            "createdat" or "created" => isDescending
                ? recipes.OrderByDescending(r => r.CreatedAt).ToList()
                : recipes.OrderBy(r => r.CreatedAt).ToList(),
            "updatedat" or "updated" => isDescending
                ? recipes.OrderByDescending(r => r.UpdatedAt).ToList()
                : recipes.OrderBy(r => r.UpdatedAt).ToList(),
            "publishedat" or "published" => isDescending
                ? recipes.OrderByDescending(r => r.PublishedAt).ToList()
                : recipes.OrderBy(r => r.PublishedAt).ToList(),
            "status" => isDescending
                ? recipes.OrderByDescending(r => r.Status).ToList()
                : recipes.OrderBy(r => r.Status).ToList(),
            _ => recipes.OrderByDescending(r => r.CreatedAt).ToList() // Default sorting
        };
    }

    public static IEndpointRouteBuilder MapGetRecipesList(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("", HandleAsync)
            .WithName("GetRecipesList")
            .WithDescription("Get a list of recipes with filtering, searching, and pagination")
            .Produces<PaginatedRecipesResponse>(StatusCodes.Status200OK);

        return routes;
    }
}
