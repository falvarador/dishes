## Why

The current recipe catalog displays results in a single, potentially overwhelming list without pagination controls. As the recipe database grows, users cannot easily navigate large result sets or progressively explore available recipes. This feature enables scalable browsing through pagination with summary totals and a "Load More" option for progressive loading of additional recipes.

## What Changes

- Display total recipe count from search/filter queries
- Support paginated navigation through recipe results (page-based or offset-based)
- Provide a "Load More Recipes" button for progressive loading of additional results
- Persist pagination state in the URL for bookmarking and sharing filtered/paginated views
- Update API to support limit/offset or page/pageSize query parameters
- Ensure backend correctly counts total results including active filters

## Capabilities

### New Capabilities

- `recipes/pagination-progressive-loading`: Pagination and progressive loading for recipe catalog browsing, including total result display, page navigation, "Load More" button, URL state persistence, and backend limit/offset support

### Modified Capabilities

- `recipes`: Backend GET `/api/recipes` endpoint now accepts and correctly calculates pagination parameters (`limit`, `offset` or `page`, `pageSize`) with filtered result counts

## Impact

- **Backend**: Recipe list endpoint (`GetRecipesListHandler`) must track total count per query and accept limit/offset or page/size parameters
- **Frontend**: Recipe list UI must display pagination controls, total count badge, "Load More" button, and manage pagination state in URL query parameters
- **API Contract**: `/api/recipes` response includes pagination metadata (total, current page/offset, hasMore flag)
- **Database**: No schema changes; uses existing recipe data with query-based counting
- **Dependencies**: Vue 3 pagination components; likely use existing @tanstack/vue-query or fetch library for managing paginated requests
