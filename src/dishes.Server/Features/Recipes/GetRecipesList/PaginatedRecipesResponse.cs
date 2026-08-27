namespace dishes.Server.Features.Recipes.GetRecipesList;

public class PaginatedRecipesResponse
{
    public PaginatedRecipesResponse(
        List<RecipeSummaryResponse> data,
        int totalCount,
        int page,
        int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
    }

    public List<RecipeSummaryResponse> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
