## MODIFIED Requirements

### Requirement: User can search recipes globally by title or ingredients
The system SHALL provide a global recipe search capability available from the header and the recipe catalog, matching recipes by title or ingredient names, with pagination support for large result sets.

#### Scenario: Search by recipe title from header
- **WHEN** a user enters text in the header search bar that matches a recipe title
- **THEN** the system returns recipes whose titles contain the search text, paginated with total count displayed

#### Scenario: Search by ingredient from catalog
- **WHEN** a user enters text in the catalog search bar that matches any ingredient name used in a recipe
- **THEN** the system returns recipes that include matching ingredients, paginated with page navigation controls

#### Scenario: Search returns combined matches with pagination
- **WHEN** a user enters text that matches both recipe titles and ingredient names
- **THEN** the system returns the union of matching recipes without duplicates, with pagination and total count

#### Scenario: Empty search query in catalog
- **WHEN** a user clears the search input in the catalog search bar
- **THEN** the system shows the default unfiltered recipe catalog with pagination and page 1 is displayed

#### Scenario: No matching recipes found
- **WHEN** a user submits a search query with no title or ingredient matches
- **THEN** the system returns an empty result set, indicates no recipes were found, and shows total count as 0

#### Scenario: GET /api/recipes endpoint returns paginated results
- **WHEN** the backend receives a GET `/api/recipes` request with optional `limit` and `offset` parameters (or `page` and `pageSize`)
- **THEN** the response returns only the requested page of results and includes pagination metadata: `total` (total recipes matching filters), `limit`/`offset` (or `page`/`pageSize`), and `hasMore` (boolean indicating if additional results exist)

#### Scenario: Pagination with multiple filters
- **WHEN** user applies multiple filters (category, difficulty, prep time, search term) along with pagination parameters
- **THEN** the API returns correct result count for the combined filter set with proper pagination
