## 1. Dish request & validation

- [x] 1.1 Create `dishes.Server/Features/Dishes/DishRequest.cs` record with `Name` property using `[Required]` and `[MaxLength(200)]` DataAnnotations (mirroring `IngredientRequest`) and verify it compiles

## 2. Dish CRUD handlers

- [x] 2.1 Create `Features/Dishes/GetDishById/GetDishByIdHandler.cs` returning 200 with the dish or 404, and map it in `DishesEndpoints.cs`
- [x] 2.2 Create `Features/Dishes/CreateDish/CreateDishHandler.cs` accepting `DishRequest`, persisting a new `Dish`, and returning 201 with the created resource; map it in `DishesEndpoints.cs`
- [x] 2.3 Create `Features/Dishes/UpdateDish/UpdateDishHandler.cs` accepting `DishRequest`, updating an existing dish's name, returning 200 or 404; map it in `DishesEndpoints.cs`
- [x] 2.4 Create `Features/Dishes/DeleteDish/DeleteDishHandler.cs` removing an existing dish, returning 204 or 404; map it in `DishesEndpoints.cs`
- [x] 2.5 Verify `dishes.Server` builds successfully with `run_build` after all handlers are wired

## 3. Manual test file

- [x] 3.1 Update `dishes.Server/dishes.Server.http` to add `@dishId` variable and requests for `GET /api/dishes/{id}`, `POST /api/dishes`, `PUT /api/dishes/{id}`, `DELETE /api/dishes/{id}`, alongside the existing `GET /api/dishes`

## 4. Test project setup

- [x] 4.1 Create `test/dishes.Server.Tests/dishes.Server.Tests.csproj` xUnit project targeting `net10.0`, referencing `dishes.Server.csproj`, with `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.Sqlite`, and `Microsoft.NET.Test.Sdk`/`xunit`/`xunit.runner.visualstudio` packages
- [x] 4.2 Add the new test project to `dishes.slnx` and verify `dotnet build` / solution build succeeds
- [x] 4.3 Create a `CustomWebApplicationFactory` that swaps `AppDbContext` to use an open SQLite in-memory connection (`EnsureCreated`, no seeded data) and verify a placeholder test using it passes

## 5. Dishes CRUD unit/integration tests

- [x] 5.1 Add tests for `GET /api/dishes` covering empty list and list with created dishes, and verify they pass
- [x] 5.2 Add tests for `GET /api/dishes/{id}` covering existing id (200) and missing id (404), and verify they pass
- [x] 5.3 Add tests for `POST /api/dishes` covering valid creation (201), missing name (400), and name over 200 characters (400), and verify they pass
- [x] 5.4 Add tests for `PUT /api/dishes/{id}` covering valid update (200), missing id (404), and invalid name (400), and verify they pass
- [x] 5.5 Add tests for `DELETE /api/dishes/{id}` covering existing id (204) and missing id (404), and verify they pass
- [x] 5.6 Run the full test project with `run_tests` and confirm all Dishes tests pass with no regressions

## 6. Final verification

- [x] 6.1 Run a full solution build (`run_build` with no project path) and confirm success
- [x] 6.2 Manually confirm `dishes.Server.http` requests are consistent with implemented routes (review file contents)
