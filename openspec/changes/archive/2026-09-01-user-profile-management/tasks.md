## 1. AppIdentityUser Class Creation

- [x] 1.1 Create `src/dishes.Server/Data/Entities/AppIdentityUser.cs` extending `IdentityUser` with profile properties (FullName, CulinaryTitle, Biography, Location, ProfilePhotoUrl, CreatedAt) and verify the file compiles without errors

- [x] 1.2 Add a constructor to `AppIdentityUser` that initializes profile fields to empty strings or appropriate defaults and verify constructor is called during object creation tests

## 2. Database Migration

- [x] 2.1 Create EF Core migration `AddProfileFieldsToAppIdentityUser` using `dotnet ef migrations add` in the server project and verify the migration file is generated in `Migrations/` directory

- [x] 2.2 Apply the migration with `dotnet ef database update` and verify the `AspNetUsers` table now contains the new profile columns (FullName, CulinaryTitle, Biography, Location, ProfilePhotoUrl, CreatedAt)

## 3. Program.cs and Identity Registration Updates

- [x] 3.1 Update `src/dishes.Server/Program.cs` to change `MapIdentityApi<IdentityUser>()` to `MapIdentityApi<AppIdentityUser>()` and verify the application builds successfully

- [x] 3.2 Verify that all Identity-related middleware and services automatically resolve to `AppIdentityUser` in Program.cs and the application starts without runtime errors

## 4. Handler Updates for AppIdentityUser

- [x] 4.1 Update `src/dishes.Server/Features/Identity/Register/RegisterHandler.cs` to inject `UserManager<AppIdentityUser>` instead of `UserManager<IdentityUser>` and verify it compiles and accepts profile data in registration requests

- [x] 4.2 Update `src/dishes.Server/Features/Identity/Login/LoginHandler.cs` to inject `UserManager<AppIdentityUser>` and modify responses to include profile data and verify login returns profile information

- [x] 4.3 Update `src/dishes.Server/Features/Identity/UpdateProfile/UpdateProfileHandler.cs` (if it exists) to work with `AppIdentityUser` profile properties and verify it persists profile updates correctly

## 5. New Profile Endpoints

- [x] 5.1 Create `src/dishes.Server/Features/Identity/GetProfile/GetProfileHandler.cs` that retrieves the authenticated user's profile from `UserManager<AppIdentityUser>` and returns HTTP 200 with profile data, or HTTP 401 if unauthenticated, and verify the handler compiles

- [x] 5.2 Create `src/dishes.Server/Features/Identity/GetProfile/GetProfileRequest.cs` (empty request DTO) and `GetProfileResponse.cs` (containing profile fields) and verify DTOs are properly defined

- [x] 5.3 Create `src/dishes.Server/Features/Identity/UpdateProfile/UpdateProfileRequest.cs` with profile fields (FullName, CulinaryTitle, Biography, Location, ProfilePhotoUrl) and add validation (max length constraints per design) and verify validation rules are applied

- [x] 5.4 Create or update `src/dishes.Server/Features/Identity/UpdateProfile/UpdateProfileHandler.cs` to accept `UpdateProfileRequest`, validate data, update the authenticated user's `AppIdentityUser` profile fields via `UserManager<AppIdentityUser>`, persist changes with `UpdateAsync()`, and return HTTP 200 with updated profile or HTTP 401 if unauthenticated, and verify the handler compiles and logic is correct

- [x] 5.5 Create `src/dishes.Server/Features/Identity/IdentityEndpoints.cs` (or update existing) to map `GET /api/user/profile` to `GetProfileHandler` and `PUT /api/user/profile` to `UpdateProfileHandler` and verify endpoints are registered in Program.cs

## 6. Request/Response DTOs and Validation

- [x] 6.1 Ensure `GetProfileResponse.cs` includes all profile fields (FullName, CulinaryTitle, Biography, Location, ProfilePhotoUrl, CreatedAt) and matches the spec requirements and verify DTO structure matches API contract

- [x] 6.2 Ensure `UpdateProfileRequest.cs` includes validation attributes for max length (FullName: 255, CulinaryTitle: 100, Biography: 1000, Location: 200, ProfilePhotoUrl: 500) and verify validation happens on bind

- [x] 6.3 Ensure profile response DTOs are consistent across all identity handlers (Register, Login, GetProfile, UpdateProfile) and return the same structure and verify response shape is standardized

## 7. Application Build and Verification

- [x] 7.1 Run `dotnet build` on the entire dishes.slnx solution and verify no compilation errors or warnings related to Identity or profile changes

- [x] 7.2 Run the application with `dotnet run` from `src/dishes.Server` and verify it starts successfully without runtime Identity configuration errors

## 8. Integration Tests

- [x] 8.1 Create `test/dishes.Server.Tests/Features/Identity/GetProfileHandlerTests.cs` with tests for: a) authenticated user retrieves own profile (HTTP 200), b) unauthenticated request returns 401, c) profile photo URL is included if set and verify all tests pass

- [x] 8.2 Create or update `test/dishes.Server.Tests/Features/Identity/UpdateProfileHandlerTests.cs` with tests for: a) authenticated user updates profile with valid data (HTTP 200), b) full name exceeding 255 characters returns 400, c) culinary title exceeding 100 characters returns 400, d) unauthenticated request returns 401 and verify all tests pass

- [x] 8.3 Create or update `test/dishes.Server.Tests/Features/Identity/RegisterHandlerTests.cs` to verify new user accounts include initialized profile fields with default values and verify profile is retrievable post-registration

- [x] 8.4 Run all identity-related tests with `run_tests` and verify no regressions in existing authentication tests and all new profile tests pass

## 9. Manual Testing and Documentation

- [ ] 9.1 Update or create `src/dishes.Server/dishes.Server.http` file with request examples for: `GET /api/user/profile` (with auth header), `PUT /api/user/profile` with sample profile data, and verify the examples are syntactically correct

- [ ] 9.2 Test the profile endpoints manually using the HTTP file in Visual Studio or a REST client, verify GET returns user profile, verify PUT accepts updates and persists them, and document any edge cases observed

## 10. Final Build and Validation

- [ ] 10.1 Run a full solution build (`dotnet build` or `run_build`) and confirm all projects compile with no errors or warnings

- [x] 10.2 Run all tests in the solution and verify no regressions and all profile-related tests pass

- [ ] 10.3 Verify that the Git status shows expected file additions/modifications (AppIdentityUser, migration, new handlers, updated handlers) and commit changes with appropriate message referencing RF-02
