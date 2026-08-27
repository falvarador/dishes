namespace dishes.Server.Features.Recipes.GetRecipesList;

public class GetRecipesListRequest
{
    /// <summary>Filter by recipe status (Draft=0, Published=1, Archived=2)</summary>
    public int? Status { get; set; }

    /// <summary>Filter by difficulty level (e.g., "Easy", "Medium", "Hard")</summary>
    public string? Difficulty { get; set; }

    /// <summary>Filter by prep time (e.g., "30 mins", "1 hour")</summary>
    public string? PrepTime { get; set; }

    /// <summary>Filter by creator ID</summary>
    public Guid? CreatorId { get; set; }

    /// <summary>Search by title or description (partial match)</summary>
    public string? Search { get; set; }

    /// <summary>Page number (1-based)</summary>
    public int Page { get; set; } = 1;

    /// <summary>Number of records per page</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Field to sort by (e.g., "Title", "CreatedAt", "Difficulty")</summary>
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>Sort order: "asc" for ascending, "desc" for descending</summary>
    public string? SortOrder { get; set; } = "desc";

    /// <summary>Validate pagination parameters</summary>
    public void Validate()
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = 1;
        if (PageSize > 100) PageSize = 100; // Cap at 100 items per page
        if (string.IsNullOrWhiteSpace(SortOrder) || (SortOrder != "asc" && SortOrder != "desc"))
            SortOrder = "desc";
    }
}
