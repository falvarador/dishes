## 1. Database Schema and Data Model

- [ ] 1.1 Create `Recipe` entity class with properties: Id (Guid), Title (string, required, max 500), Description (string, required, max 5000), CoverPhotoPath (string, nullable), PrepTime (string, e.g., "35m", "2 hrs"), Difficulty (enum: Easy, Medium, Hard, Advanced), CreatorId (Guid), CreatedAt (DateTime), UpdatedAt (DateTime), and navigation properties for Ingredients, Instructions, Categories, Tags. Verify the model compiles.
- [ ] 1.2 Create `RecipeIngredient` entity with properties: Id (Guid), RecipeId (Guid, foreign key), IngredientName (string), Quantity (decimal), Unit (string, e.g., "cup", "tsp", "gram"), Display/verify compiles.
- [ ] 1.3 Create `RecipeInstruction` entity with properties: Id (Guid), RecipeId (Guid, foreign key), StepNumber (int), Description (string), and navigation to Recipe. Verify compiles.
- [ ] 1.4 Create `RecipeCategory` entity with properties: Id (Guid), Name (string, required, max 100, unique). Verify compiles.
- [ ] 1.5 Create `RecipeTag` entity with properties: Id (Guid), Name (string, required, max 100). Verify compiles.
- [ ] 1.6 Create `RecipeRecipeCategory` junction entity for many-to-many Recipe-Category relationship. Verify compiles.
- [ ] 1.7 Create `RecipeRecipeTag` junction entity for many-to-many Recipe-Tag relationship. Verify compiles.
- [ ] 1.8 Update `AppDbContext` to configure Recipe entities with proper relationships (one-to-many for Ingredients/Instructions, many-to-many for Categories/Tags). Verify context configures without errors.
- [ ] 1.9 Create and apply EF Core migrations for new Recipe schema. Verify migration is generated and database schema updated by running `dotnet ef database update` and checking tables exist.

## 2. Request/Response DTOs and Validation

- [ ] 2.1 Create `RecipeRequest` record with properties: Title (required, max 500 chars), Description (required, max 5000 chars), Difficulty (enum string), PrepTime (required, pattern like "35m", "2 hrs"), CategoryIds (list of Guid or names), Tags (list of strings). Add DataAnnotations validation attributes. Verify compiles.
- [ ] 2.2 Create `RecipeIngredientRequest` record with properties: IngredientName (required), Quantity (required, decimal), Unit (required, max 50). Add validation. Verify compiles.
- [ ] 2.3 Create `RecipeInstructionRequest` record with properties: StepNumber (int), Description (required, max 2000). Verify compiles.
- [ ] 2.4 Create `RecipeResponse` record with properties: Id, Title, Description, CoverPhotoPath, PrepTime, Difficulty, CreatorId, CreatedAt, Ingredients (list), Instructions (list), Categories (list), Tags (list). Verify compiles.
- [ ] 2.5 Create `RecipeIngredientResponse` record with properties: Id, IngredientName, Quantity, Unit. Verify compiles.
- [ ] 2.6 Create `RecipeInstructionResponse` record with properties: Id, StepNumber, Description. Verify compiles.

## 3. Recipe Creation Handler

- [ ] 3.1 Create `CreateRecipeHandler` in `Features/Recipes/CreateRecipe/` folder following the existing pattern (handler class with HandleAsync method, MapCreateRecipe endpoint mapper). Handler should validate RecipeRequest, create Recipe entity, add ingredients and instructions, save to database, return 201 Created with recipe ID. Verify handler compiles.
- [ ] 3.2 Implement image upload in CreateRecipeHandler: accept MultipartFormData with file, save to local file system under configured recipe images directory, store path in Recipe.CoverPhotoPath. Verify file operations work without errors.
- [ ] 3.3 Create `CreateRecipeHandler` tests in `test/dishes.Server.Tests/Recipes/RecipeCrudTests.cs`: test valid recipe creation, test missing required fields (title, description), test invalid prep time format, test max length violations. Verify all tests pass.

## 4. Recipe Detail Handler

- [ ] 4.1 Create `GetRecipeHandler` in `Features/Recipes/GetRecipe/` folder returning recipe with all ingredients and instructions. Use `.Include()` to eager-load Ingredients, Instructions, Categories, Tags to avoid N+1 queries. Test loads recipe with all related data.  Verify compiles and loads data efficiently.
- [ ] 4.2 Create `GetRecipesHandler` in `Features/Recipes/GetRecipes/` folder returning list of recipes (for listing). Verify compiles.
- [ ] 4.3 Create test `GetRecipe_ReturnsRecipeWithAllDetails` that seeds recipe with ingredients/instructions and verifies they are returned. Verify test passes.
- [ ] 4.4 Create test `GetRecipe_ReturnsNotFound_WhenRecipeDoesNotExist`. Verify test passes.

## 5. Recipe Endpoints Registration

- [ ] 5.1 Create `RecipesEndpoints.cs` in `Features/Recipes/` following existing pattern (endpoint group mapper MapRecipesEndpoints that registers POST, GET endpoints). Verify file creates and compiles.
- [ ] 5.2 Register recipe endpoints in `Program.cs`: call `apiGroup.MapRecipesEndpoints()` with `/api/recipes` prefix. Verify Program.cs compiles.
- [ ] 5.3 Run full solution build and verify no compilation errors introduced.

## 6. Recipe Management (Edit/Delete)

- [ ] 6.1 Create `UpdateRecipeHandler` in `Features/Recipes/UpdateRecipe/` folder. Handler should accept recipe ID and RecipeRequest, find recipe, validate, update fields, clear and re-add ingredients/instructions, save, return 200 OK. Verify compiles.
- [ ] 6.2 Create `DeleteRecipeHandler` in `Features/Recipes/DeleteRecipe/` folder. Handler should find recipe, delete, return 204 NoContent. Verify compiles.
- [ ] 6.3 Register Update and Delete endpoints in RecipesEndpoints. Verify compiles and endpoints available.
- [ ] 6.4 Create test `UpdateRecipe_ReturnsOk_WhenValid` (seed recipe, update fields, verify updated). Verify test passes.
- [ ] 6.5 Create test `DeleteRecipe_ReturnsNoContent_WhenExists` and verify deletion succeeds. Verify test passes.

## 7. Validation Tests

- [ ] 7.1 Create test `CreateRecipe_ReturnsBadRequest_WhenTitleIsMissing`. Verify test passes.
- [ ] 7.2 Create test `CreateRecipe_ReturnsBadRequest_WhenDescriptionIsMissing`. Verify test passes.
- [ ] 7.3 Create test `CreateRecipe_ReturnsBadRequest_WhenTitleExceedsMaxLength`. Verify test passes.
- [ ] 7.4 Create test `CreateRecipe_ReturnsBadRequest_WithInvalidDifficulty`. Verify test passes.
- [ ] 7.5 Create test `CreateRecipe_SucceedsWithValidIngredients` (create recipe with 3+ ingredients, verify all stored and returned). Verify test passes.
- [ ] 7.6 Create test `CreateRecipe_SucceedsWithInstructions` (create recipe with 5+ steps, verify all stored in order). Verify test passes.

## 8. Integration Tests

- [ ] 8.1 Create comprehensive integration tests in `test/dishes.Server.Tests/Recipes/RecipeCrudTests.cs` using `CustomWebApplicationFactory` and SQLite in-memory database. Verify test project compiles and includes new test class.
- [ ] 8.2 Create test helper methods (`ClearRecipesAsync`, `SeedRecipeAsync`) following existing Dishes pattern for test setup/teardown.
- [ ] 8.3 Run `dotnet test` for test project and verify all Recipes tests pass (minimum 8+ tests covering CRUD operations and validation).

## 9. Database Seeding and Initial Data

- [ ] 9.1 Create initial categories in database seed data (Main Course, Appetizer, Dessert, Breakfast, Seafood, Baking, Lunch, Dinner, Snack). Add to AppDbContext.HasData() configuration. Verify seed data included in migrations and can be verified via database query.

## 10. Final Validation and Build

- [ ] 10.1 Run `dotnet build` for entire solution and verify zero compilation errors and warnings.
- [ ] 10.2 Run `dotnet test` for entire test project and verify all tests pass (both new Recipes tests and existing Ingredients/Dishes tests).
- [ ] 10.3 Verify API endpoints are accessible via manual test or `.http` file (add POST /api/recipes, GET /api/recipes/{id}, PUT, DELETE examples to `dishes.Server.http`). Suggest running against actual server instance to test end-to-end.
- [ ] 10.4 Verify recipeCreation spec requirements are fully implemented by cross-checking each Requirement vs handler/endpoint code.
