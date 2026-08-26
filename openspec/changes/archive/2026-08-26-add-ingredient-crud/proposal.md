## Why

The Dishes feature can already read dishes and their ingredients, but ingredients themselves have no dedicated API. There is no way to create, update, delete, or list ingredients independently of a dish, which blocks building ingredient management UI and reusing ingredients across dishes.

## What Changes

- Add a new `Ingredients` vertical-slice feature under `dishes.Server/Features/Ingredients` mirroring the existing `Features/Dishes` structure.
- Add CRUD endpoints for ingredients: `GET /api/ingredients`, `GET /api/ingredients/{id}`, `POST /api/ingredients`, `PUT /api/ingredients/{id}`, `DELETE /api/ingredients/{id}`.
- Add request/response validation (e.g., required, max-length name) consistent with the existing `Ingredient` entity constraints.
- Register the new endpoint group in `Program.cs` alongside `MapDishesEndpoints`.
- Regenerate/update the Kiota-generated TypeScript client (`dishes.client/src/client`) to expose the new ingredients endpoints for the Vue frontend.

## Capabilities

### New Capabilities
- `ingredients`: CRUD operations (create, read one, read all, update, delete) for the `Ingredient` entity, exposed via minimal API endpoints under `/api/ingredients`.

### Modified Capabilities
- None.

## Impact

- **Affected code**: `dishes.Server/Features/Ingredients/**` (new), `dishes.Server/Program.cs` (endpoint registration), `dishes.Server/Data/AppDbContext.cs` (no schema change expected, existing `Ingredient` entity is reused).
- **APIs**: New REST endpoints under `/api/ingredients`.
- **Frontend**: `dishes.client/src/client` Kiota-generated client will need regeneration to include the new endpoints; no existing client code is removed.
- **Dependencies**: None new; reuses existing EF Core / SQLite setup.
