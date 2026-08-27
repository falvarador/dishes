## 1. Test Setup

- [x] 1.1 Create `test/dishes.Server.Tests/Ingredients/` folder and `IngredientsCrudTests.cs` file scaffolded with `IClassFixture<CustomWebApplicationFactory>`, mirroring `DishesCrudTests.cs`, and verify the file compiles as part of the test project build.
- [x] 1.2 Add `ClearIngredientsAsync` and `SeedIngredientAsync` helper methods using `AppDbContext` (following the `ClearDishesAsync`/`SeedDishAsync` pattern) and verify they compile and are usable from test methods.

## 2. List and Get Endpoint Tests

- [x] 2.1 Add test `GetIngredients_ReturnsEmptyList_WhenNoIngredientsExist` and verify it passes.
- [x] 2.2 Add test `GetIngredients_ReturnsSeededIngredients` (seed one or more ingredients, call `GET /api/ingredients`, assert contents) and verify it passes.
- [x] 2.3 Add test `GetIngredientById_ReturnsIngredient_WhenExists` and verify it passes.
- [x] 2.4 Add test `GetIngredientById_ReturnsNotFound_WhenIngredientDoesNotExist` and verify it passes.

## 3. Create Endpoint Tests

- [x] 3.1 Add test `CreateIngredient_ReturnsCreated_WhenValid` (POST with a valid name, assert 201 and persisted entity) and verify it passes.
- [x] 3.2 Add test `CreateIngredient_ReturnsBadRequest_WhenNameIsMissing` (covers the `Required` DataAnnotation) and verify it passes.
- [x] 3.3 Add test `CreateIngredient_ReturnsBadRequest_WhenNameExceedsMaxLength` (covers the `MaxLength(200)` DataAnnotation) and verify it passes.

## 4. Update Endpoint Tests

- [x] 4.1 Add test `UpdateIngredient_ReturnsNoContentOrOk_WhenValid` (seed an ingredient, PUT with valid name, assert success and updated value) and verify it passes.
- [x] 4.2 Add test `UpdateIngredient_ReturnsNotFound_WhenIngredientDoesNotExist` and verify it passes.
- [x] 4.3 Add test `UpdateIngredient_ReturnsBadRequest_WhenNameIsInvalid` (missing or too long name) and verify it passes.

## 5. Delete Endpoint Tests

- [x] 5.1 Add test `DeleteIngredient_ReturnsNoContent_WhenExists` (seed, delete, verify removed from database) and verify it passes.
- [x] 5.2 Add test `DeleteIngredient_ReturnsNotFound_WhenIngredientDoesNotExist` and verify it passes.

## 6. Validation

- [x] 6.1 Run `dotnet test` for `test/dishes.Server.Tests` and verify all new and existing Ingredients and Dishes tests pass.
- [x] 6.2 Run a full solution build and verify it succeeds with no warnings/errors introduced.

