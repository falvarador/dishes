## Context

The server uses a vertical-slice minimal API structure: each feature lives under `dishes.Server/Features/<Feature>/`, with an `<Feature>Endpoints.cs` file that maps a route group, and one subfolder per operation (e.g., `GetDishes/GetDishesHandler.cs`) containing a static handler class with a private `HandleAsync` method and a `Map<Operation>` extension method. `Program.cs` wires feature groups under a shared `/api` group. The `Ingredient` entity already exists in `Data/Entities/Ingredient.cs` and is registered in `AppDbContext` with a many-to-many relationship to `Dish`. See proposal.md - Why/What Changes for motivation and scope.

## Goals / Non-Goals

**Goals:**
- Mirror the existing `Features/Dishes` vertical-slice pattern exactly for consistency.
- Provide full CRUD for `Ingredient` without altering the existing entity or database schema.
- Keep validation consistent with the entity's data annotations (`Required`, `MaxLength(200)`).

**Non-Goals:**
- No changes to the `Dish` <-> `Ingredient` relationship or migration schema.
- No authentication/authorization additions (none exist today for Dishes either).
- No pagination/filtering for the list endpoint (matches current `GetDishes` behavior).

## Decisions

- **Feature folder layout**: Create `Features/Ingredients/` with subfolders `GetIngredients/`, `GetIngredientById/`, `CreateIngredient/`, `UpdateIngredient/`, `DeleteIngredient/`, each containing a `<Operation>Handler.cs`, matching the `GetDishes` precedent rather than a single controller-style file. This keeps each operation independently testable and consistent with the established convention.
- **Request DTOs**: Introduce a small `IngredientRequest` record (Name) for create/update bodies instead of binding directly to the `Ingredient` entity, avoiding accidental over-posting of `Id`/`Dishes`. Considered binding directly to `Ingredient` but rejected due to lack of input control.
- **Endpoint registration**: Add `MapIngredientsEndpoints()` extension analogous to `MapDishesEndpoints()`, called from `Program.cs` under the existing `/api` group as `apiGroup.MapIngredientsEndpoints();`.
- **Not found handling**: Use `Results.NotFound()` for missing ids on get/update/delete, consistent with minimal API conventions used elsewhere in ASP.NET Core minimal APIs (no existing precedent in this repo since Dishes only has GET-all).
- **Client regeneration**: The Kiota-generated TypeScript client under `dishes.client/src/client` will be regenerated from the updated OpenAPI document once the endpoints exist; this is a build-time/tooling step, not hand-written code.

## Risks / Trade-offs

- [Deleting an ingredient still referenced by dishes silently removes the join rows] → EF Core's default many-to-many behavior will just remove the join table entries; this matches existing cascade behavior and is acceptable since no explicit restrict rule exists today.
- [No integration tests currently exist for Dishes endpoints] → New Ingredients endpoints will similarly ship without dedicated tests unless the user requests them in tasks.md; flagged here for visibility.

## Migration Plan

- No database migration is required since `Ingredient` and the join table already exist.
- Deploy is a standard code deployment: add the new feature folder, register the endpoint group, and redeploy the server. Rollback is reverting the commit; no data migration to undo.
