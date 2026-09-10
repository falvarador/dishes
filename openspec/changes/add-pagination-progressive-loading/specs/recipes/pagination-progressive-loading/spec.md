## Purpose

Enables paginated browsing of recipe results with progressive "Load More" capability, displaying total result counts, and persisting pagination state in the URL query parameters for bookmarkable and shareable filtered views.

## ADDED Requirements

### Requirement: Users can view pagination information and total results
The system SHALL display the total number of recipes matching the current search and filter criteria, along with current pagination position (e.g., "Showing 1-12 of 47 recipes").

#### Scenario: Display total results with pagination info
- **WHEN** a user navigates to the recipe list or applies filters
- **THEN** the system displays total count (e.g., "47 recipes found") and current range (e.g., "Showing 1-12")

#### Scenario: Total updates on filter change
- **WHEN** a user changes search terms or applies new filters
- **THEN** the total count updates to reflect only matching recipes

### Requirement: Users can navigate through recipe pages
The system SHALL provide pagination controls (Previous/Next buttons or page numbers) to navigate through recipe results when the total exceeds the current page size.

#### Scenario: Next button loaded more recipes
- **WHEN** user clicks the "Next" button on a page that is not the last page
- **THEN** the recipe list updates to show the next page of results and the URL updates to reflect the new page number or offset

#### Scenario: Previous button returns to prior page
- **WHEN** user clicks the "Previous" button and is not on the first page
- **THEN** the recipe list updates to show the prior page of results and the URL updates accordingly

#### Scenario: Pagination controls disabled at boundaries
- **WHEN** user is on the first page
- **THEN** the "Previous" button is disabled
- **WHEN** user is on the last page
- **THEN** the "Next" button is disabled

### Requirement: Users can progressively load more recipes
The system SHALL provide a "Load More Recipes" button that fetches and appends the next batch of recipes to the current list without replacing existing results.

#### Scenario: Load More button appends recipes
- **WHEN** user clicks the "Load More Recipes" button
- **THEN** the system fetches the next batch of recipes and appends them to the bottom of the current list without clearing existing recipes

#### Scenario: Load More reflects updated count
- **WHEN** more recipes are loaded
- **THEN** the displayed total count and "Showing X-Y of Z" indicator updates to reflect newly loaded items

#### Scenario: Load More button hidden when all loaded
- **WHEN** all available recipes matching the current filters have been loaded
- **THEN** the "Load More Recipes" button is hidden or disabled

### Requirement: Pagination state persists in the URL
The system SHALL store pagination parameters (page number, offset, or limit) in the URL query string, enabling users to bookmark or share paginated views.

#### Scenario: URL contains page number after navigation
- **WHEN** user navigates to a specific page
- **THEN** the browser URL updates to include pagination parameters (e.g., `?page=2` or `?offset=12&limit=12`)

#### Scenario: Reload preserves pagination position
- **WHEN** user reloads the page after navigating to a specific page
- **THEN** the recipe list displays the same page results as before the reload

#### Scenario: Shared pagination URL works for other users
- **WHEN** user shares a URL with pagination parameters (and filters)
- **THEN** another user can visit the same URL and see the same paginated results

### Requirement: Backend API correctly calculates filtered result counts
The system backend SHALL compute the total number of recipes matching all active filters and return pagination metadata with each request (total count, has more flag, current page or offset).

#### Scenario: API returns total count with filters
- **WHEN** GET `/api/recipes?category=Main%20Course&difficulty=Easy&limit=12&offset=0` is called
- **THEN** the response includes `total: <count of recipes matching filters>` and `hasMore: true/false`

#### Scenario: API returns correct offset for Load More
- **WHEN** GET `/api/recipes?limit=12&offset=12` is called after loading the first 12
- **THEN** the response contains recipes 13-24 and the hasMore flag indicates whether more exist beyond these

#### Scenario: API respects pagination parameters
- **WHEN** the endpoint receives `limit` and `offset` parameters
- **THEN** the response returns only the requested page of results and includes the total count across all matching records
