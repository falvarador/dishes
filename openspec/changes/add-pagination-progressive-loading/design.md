## Context

The current recipe listing endpoint (`GET /api/recipes`) returns all matching recipes in a single response, which can become unwieldy as the recipe database grows. Frontend displays must handle potentially large datasets in memory, and users cannot progressively explore results. The backend and frontend currently have no pagination logic.

## Goals / Non-Goals

**Goals:**
- Implement pagination support on the backend with configurable page size
- Display total recipe count and current pagination position on the frontend
- Provide both page-based navigation (Previous/Next) and progressive "Load More" capability
- Persist pagination state in URL query parameters (bookmarkable and shareable views)
- Ensure pagination works correctly with all existing filters (category, difficulty, prep time, search)
- Maintain backward compatibility where possible (default to page 1 if pagination params omitted)

**Non-Goals:**
- Server-side session state (pagination held in query params, not server session)
- Infinite scroll (explicit "Load More" button; not auto-load on scroll)
- Custom page size per user (fixed page size; default 12 recipes per page)
- Recipe sorting changes (existing sort options apply within each page)
- Database optimization (assume queries remain efficient with filtering; no new indexes required as part of this change)

## Decisions

### 1. Pagination Parameter Strategy
**Decision:** Use `limit` and `offset` parameters in the query string (e.g., `?limit=12&offset=0`), not `page` and `pageSize`.

**Rationale:** 
- Offset-based pagination is simpler for stateless REST APIs.
- Aligns with common database cursor patterns (LIMIT/OFFSET in SQL).
- Frontend can easily calculate next/previous offsets in JavaScript.

**Alternatives Considered:**
- Page/pageSize: More user-friendly in URLs (shows page numbers), but requires calculation to convert to offsets in the SQL layer.
- Cursor-based (keyset): More efficient for large datasets with frequent inserts, but adds complexity for this use case and requires recipe ID ordering contract.

### 2. Response Metadata Structure
**Decision:** Include pagination metadata in the API response as a wrapper object with `data` (recipes array), `pagination` (total, limit, offset, hasMore).

**Rationale:**
- Provides all information the frontend needs for "Load More" logic and display ("X of Y" text).
- `hasMore` boolean simplifies frontend checks for whether to show the "Load More" button.
- Keeps response structure predictable and extensible.

**Example Response:**
```json
{
  "data": [ { "id": 1, "title": "Pasta Carbonara", ... }, ... ],
  "pagination": {
	"total": 47,
	"limit": 12,
	"offset": 0,
	"hasMore": true
  }
}
```

### 3. Default Page Size
**Decision:** Default page size is 12 recipes per page.

**Rationale:**
- Common default in recipe/content discovery apps.
- Balances API response size with UI layout expectations (typically fills 1-2 screens).
- Provides reasonable experience when user first lands on recipe catalog.

**Alternatives Considered:**
- 10 recipes: Slightly smaller, but arbitrary.
- 20 recipes: Larger payloads, may require more scrolling on mobile.
- Dynamic per user: Out of scope for this change.

### 4. Frontend State Management
**Decision:** Query parameters (`?limit=12&offset=0` or converted to `?page=1`) are the single source of truth for pagination. Local Vue component state syncs via `useRecipeFilters` composable.

**Rationale:**
- URL-driven state enables bookmarking and sharing.
- Composable already manages filter state and URL sync (`useRecipeFilters`).
- Avoids duplication between URL and component state.

**Implementation:**
- Extend `useRecipeFilters.ts` to include `limit` and `offset` parsing and query generation.
- When user paginates, update URL; URL change triggers API fetch via React Query/TanStack Vue Query.
- On page load, extract pagination params from URL and populate initial state.

### 5. Loading More vs. Pagination Navigation
**Decision:** Support both page-based navigation (Previous/Next buttons) and a "Load More" button in the same view.

**Rationale:**
- Page nav appeals to users who want to jump; Load More appeals to progressive discovery.
- Can coexist without complexity: both update the URL offset.
- "Load More" appends to the current list (no auto-scroll).
- Next/Previous replaces the list (standard pagination).

**Implementation Detail:**
- "Load More" combines existing data with new response: `recipes = [...recipes, ...newPageRecipes]`.
- Next/Previous replaces: `recipes = newPageRecipes`.

### 6. Backward Compatibility
**Decision:** Omitting pagination parameters defaults to page 1 (offset 0) with default page size (12).

**Rationale:**
- Existing URLs without pagination params continue to work.
- Users sharing old recipe list URLs see the first page, not an error.

## Risks / Trade-offs

**Risk: Large offset queries become slow on very large datasets**
- Mitigation: Accept as acceptable for current scale (hundreds of recipes). If thousands, consider cursor-based pagination in a future iteration. Monitor query performance.

**Risk: User deletes/adds recipes mid-pagination, causing skipped or duplicate results**
- Mitigation: Expected behavior for offset-based pagination on mutable data; document as known limitation. Pagination reflects state at request time; consistency guaranteed only per request, not across requests.

**Risk: Frontend shows stale "hasMore" state if new recipes added during session**
- Mitigation: Low impact. Page reload or filter change will sync. Not worth real-time reflection.

**Risk: "Load More" appending behavior does not match browser back-button expectations**
- Mitigation: Back button updates URL (offset decreases), which replaces the list. Forward button works as expected. Users clicking back will see the prior page list, not a combined list.

## Migration Plan

### Phase 1: Backend Implementation
1. Modify `GetRecipesListHandler.HandleAsync` to accept `limit` and `offset` query parameters.
2. Calculate total count of filtered results before pagination.
3. Apply `Take(limit).Skip(offset)` to EF Core query.
4. Return response with `data` array and `pagination` metadata.
5. Deploy to dev/staging environment.

### Phase 2: Frontend Implementation
1. Extend `useRecipeFilters` to parse and manage `limit`/`offset` from URL.
2. Create `PaginationInfo` component to display "X of Y" and hasMore status.
3. Create pagination navigation buttons (Previous/Next) and "Load More" button.
4. Integrate into recipe list view (`AppNew.vue` or `App.vue`).
5. Test with multiple filters and bookmarking.

### Phase 3: Testing & Validation
1. Integration tests for backend (various limit/offset combos with filters).
2. E2E tests for frontend (pagination, Load More, URL persistence, sharing).
3. Manual QA on mobile and desktop.

### Rollback Strategy
- Disable pagination parameters on backend (ignore incoming limit/offset, return all results as before).
- Remove pagination UI from frontend (hide buttons, show all results).
- No data migration required.
