## Why

Recipe creation is a core feature that allows creators to share their culinary expertise with the community. Without this capability, the platform cannot fulfill its primary purpose of enabling content creators to publish and share recipes. This is foundational for the entire recipes/content ecosystem.

## What Changes

- New capability to create, manage, and publish recipes with comprehensive metadata
- Recipe creation form supporting all required fields: title, description, prep time, difficulty level, categories, tags, ingredients, and photo
- Recipe detail view displaying all recipe information
- Image upload/storage for recipe cover photos
- Ingredient list management with quantities and units
- Step-by-step instruction creation for recipes

## Capabilities

### New Capabilities
- `recipes/creation`: Creation of new recipes with full metadata (title, description, cover photo, prep time, difficulty, categories, tags, ingredients, instructions)
- `recipes/detail`: Display and retrieval of recipe details including all ingredients and preparation steps

### Modified Capabilities
(none)

## Impact

- Affected code: New `Features/Recipes` module with recipe CRUD operations
- Affected APIs: New endpoints `POST /api/recipes`, `GET /api/recipes/{id}`, `PUT /api/recipes/{id}`, `DELETE /api/recipes/{id}`
- Database: New `Recipe` entity with related tables for categories, tags, ingredients, and instructions
- Frontend: New recipe creation and detail views
- No breaking changes to existing APIs
