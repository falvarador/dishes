## Context

`dishes.Server` uses a vertical-slice / feature-folder architecture (`Features/<Feature>/<Operation>/<Operation>Handler.cs` + `<Feature>Endpoints.cs`). The Ingredients feature already implements the full CRUD pattern with DataAnnotations validation wired through `builder.Services.AddValidation()`. Dishes currently only has `GetDishes`. See proposal.md for motivation.

There is no test project in the solution yet (only `dishes.Server.csproj` and `dishes.client.esproj`). The user wants a `test` folder at the repository root, sibling to the server project, with strong coverage of CRUD operations and validations.

## Goals / Non-Goals

**Goals:**
- Bring Dishes to full CRUD parity with Ingredients using the identical architectural pattern (handler-per-operation, DataAnnotations validation on the request record).
- Keep `dishes.Server.http` synchronized with the new endpoints.
- Stand up a new xUnit test project at `test/dishes.Server.Tests` that exercises all five Dishes endpoints (list/get/create/update/delete) plus validation edge cases, using EF Core's in-memory or SQLite in-memory provider so tests don't depend on the real `Dishes.db` file.

**Non-Goals:**
- No changes to the Ingredients feature.
- No changes to the many-to-many Dish/Ingredient relationship management (e.g., no endpoint to attach/detach ingredients on a dish in this change).
- No UI/client changes; Kiota client regeneration will happen as part of the normal build but no manual client code changes are needed.

## Decisions

- **Mirror the Ingredients pattern exactly**: `DishRequest` record with `[Required]`/`[MaxLength(200)]` DataAnnotations, one static handler class per operation, `DishesEndpoints.cs` composing them. Rationale: consistency with the rest of the codebase, minimal cognitive overhead, reuses `AddValidation()` already registered in `Program.cs`.
- **Test project location**: `test/dishes.Server.Tests/dishes.Server.Tests.csproj`, referencing `dishes.Server.csproj`, added to `dishes.slnx`. Rationale: user explicitly requested a `test` folder at the same level as the source project.
- **Test approach**: Use `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) with the DB context swapped to EF Core's `UseInMemoryDatabase` (or SQLite `:memory:`) per test via a custom factory, to test the endpoints end-to-end (routing + validation + handler + persistence) without touching the real SQLite file. xUnit is used since it's the most common .NET testing framework and is not already dictated by the repo.
- **Coverage targets**: For each of Create/Get-by-id/Update/Delete, cover the success path and the relevant failure path(s) (404 for missing id, 400 for validation errors on Create/Update). For List, cover empty and non-empty scenarios.

## Risks / Trade-offs

- [Risk] Introducing a new test project changes the solution file (`dishes.slnx`) → Mitigation: add it via `dotnet sln`/direct slnx edit and verify the solution still builds.
- [Risk] In-memory/SQLite-in-memory test database behavior can diverge slightly from the real SQLite provider (e.g., cascade behavior) → Mitigation: use SQLite in-memory (`Data Source=:memory:` with a kept-open connection) rather than EF's InMemory provider, since the app already targets SQLite, minimizing provider-specific divergence.
- [Risk] Seeded data in `AppDbContext.OnModelCreating` could interfere with fresh-per-test assumptions → Mitigation: since `HasData` seeds are applied via migrations, the test database factory will create the schema via `EnsureCreated()` on a plain (unseeded) SQLite in-memory connection, so tests start from a clean state and seed their own fixtures as needed.
