## Why

The Dishes feature currently only supports listing dishes (`GET /api/dishes`). There is no way to create, retrieve a single dish, update, or delete a dish through the API, so the feature is not usable as a full CRUD resource like the existing Ingredients feature. Completing the CRUD surface brings Dishes to parity with Ingredients and unlocks proper manual and automated testing of the feature.

## What Changes

- Add `POST /api/dishes` to create a new dish (validated via DataAnnotations on a `DishRequest` record, following the `IngredientRequest` pattern).
- Add `GET /api/dishes/{id}` to retrieve a single dish by id (404 when not found).
- Add `PUT /api/dishes/{id}` to update an existing dish's name (404 when not found, validation errors when invalid).
- Add `DELETE /api/dishes/{id}` to remove an existing dish (404 when not found, 204 on success).
- Update `dishes.Server.http` to include manual test requests for all new Dishes endpoints.
- Add a `test` folder (sibling to `src`-equivalent, i.e. at repo root alongside `dishes.Server`) containing a new xUnit test project with unit tests covering Dishes CRUD handlers and their validations, aiming for strong coverage of success and failure paths.

## Capabilities

### New Capabilities
- `dishes`: Full CRUD capability for Dishes (list, get by id, create, update, delete) with DataAnnotations-based validation, mirroring the Ingredients capability.

### Modified Capabilities
(none — Dishes did not have a published spec yet; this establishes the full capability)

## Impact

- Affected code: `dishes.Server/Features/Dishes/**` (new handler folders for Create/GetById/Update/Delete, updated `DishesEndpoints.cs`), new `DishRequest.cs` record.
- Affected test surface: new test project under `test/` at repo root referencing `dishes.Server`, using an in-memory/SQLite test database.
- Affected docs/tooling: `dishes.Server/dishes.Server.http` updated with new Dishes endpoint requests.
- No breaking changes to existing `GET /api/dishes` behavior.
