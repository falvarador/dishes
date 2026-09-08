## Why

Users need a fast way to discover recipes without navigating category by category. Adding global search in the header and catalog improves findability and reduces time to locate recipes by title or ingredient.

## What Changes

- Add global search behavior for recipes using a search bar available in the application header and in the recipe catalog.
- Allow users to search recipes by recipe title and by ingredient names.
- Ensure search results show matching recipes when either title or any ingredient matches the entered text.
- Define behavior for empty or no-match searches in the catalog experience.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `recipes`: Add global search requirements for searching recipes by title or ingredients from header and catalog entry points.

## Impact

- Affected specs: `openspec/specs/recipes/spec.md`.
- Affected API/backend: recipe listing/search query behavior and filtering by title/ingredient.
- Affected frontend: header search bar and catalog search bar UX and result rendering.
- Affected tests: add coverage for title match, ingredient match, empty query handling, and no-result behavior.
