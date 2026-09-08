## ADDED Requirements

### Requirement: User can search recipes globally by title or ingredients
The system SHALL provide a global recipe search capability available from the header and the recipe catalog, matching recipes by title or ingredient names.

#### Scenario: Search by recipe title from header
- **WHEN** a user enters text in the header search bar that matches a recipe title
- **THEN** the system returns recipes whose titles contain the search text

#### Scenario: Search by ingredient from catalog
- **WHEN** a user enters text in the catalog search bar that matches any ingredient name used in a recipe
- **THEN** the system returns recipes that include matching ingredients

#### Scenario: Search returns combined matches
- **WHEN** a user enters text that matches both recipe titles and ingredient names
- **THEN** the system returns the union of matching recipes without duplicates

#### Scenario: Empty search query in catalog
- **WHEN** a user clears the search input in the catalog search bar
- **THEN** the system shows the default unfiltered recipe catalog

#### Scenario: No matching recipes
- **WHEN** a user submits a search query with no title or ingredient matches
- **THEN** the system returns an empty result set and the UI indicates no recipes were found
