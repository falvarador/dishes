## Why

The Ingredients feature has full CRUD (list, get by id, create, update, delete) but currently has zero automated test coverage. The Dishes feature already has a solid xUnit test suite (`test/dishes.Server.Tests/Dishes/DishesCrudTests.cs`) using `WebApplicationFactory` + SQLite in-memory. Bringing Ingredients to the same coverage standard closes a gap and protects the feature (including its DataAnnotations validation) from regressions.

## What Changes

- Add `test/dishes.Server.Tests/Ingredients/IngredientsCrudTests.cs` covering all five Ingredients endpoints and their validation rules, mirroring the existing `DishesCrudTests` structure and using the existing `CustomWebApplicationFactory`.
- No production code changes; no new packages required (existing test project already has everything needed).

## Capabilities

### New Capabilities
(none)

### Modified Capabilities
(none — this is test-only work with no change to the Ingredients capability's specified behavior)

## Impact

- Affected code: new test file under `test/dishes.Server.Tests/Ingredients/`.
- No changes to `src/dishes.Server` production code or API behavior.
- Improves code coverage and regression safety for the Ingredients feature.
