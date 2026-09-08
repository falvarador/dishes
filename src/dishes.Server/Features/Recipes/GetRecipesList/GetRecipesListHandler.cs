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
            var searchTerm = search.Trim().ToLower();
            query = query.Where(r =>
                r.Title.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                r.Ingredients.Any(i => i.IngredientName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, sortBy, sortOrder);

        // Apply pagination
        var recipes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.Ratings)
            .ToListAsync(cancellationToken);

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

    public static IEndpointRouteBuilder MapGetRecipesList(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("", HandleAsync)
            .WithName("GetRecipesList")
            .WithDescription("Get a list of recipes with filtering, searching, and pagination")
            .Produces<PaginatedRecipesResponse>(StatusCodes.Status200OK);

        return routes;
    }
}
