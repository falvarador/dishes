## Context

The application currently uses ASP.NET Core Identity for authentication but with the base `IdentityUser` class, which only provides basic account information (email, username, etc.). The proposal and specs require extending this model to support profile customization (full name, culinary title, biography, location, profile photo). All existing Identity-based handlers, registrations, and integrations currently reference `IdentityUser` directly.

## Goals / Non-Goals

**Goals:**
- Create an `AppIdentityUser` class that extends `IdentityUser` with profile-specific properties
- Ensure profile data is initialized on registration and persisted through the application lifecycle
- Update all Identity-related infrastructure (handlers, registrations, migrations) to use `AppIdentityUser`
- Provide GET and PUT endpoints for profile viewing and editing
- Maintain full backward compatibility with existing authentication flows

**Non-Goals:**
- Changing authentication mechanisms or token strategies
- Implementing profile image upload or storage (only URL fields for external storage)
- Adding profile search or discovery features
- Modifying existing recipe or dish management endpoints

## Decisions

### 1. Extend IdentityUser with AppIdentityUser class
**Decision**: Create a new `AppIdentityUser` class inheriting from `IdentityUser` rather than modifying the base class.

**Rationale**: This approach:
- Keeps ASP.NET Core Identity intact and testable
- Allows future users to continue using `IdentityUser` if needed
- Makes it clear where profile data is stored
- Follows established patterns in ASP.NET Core applications

**Alternatives considered**:
- Direct modification of database user table with raw SQL — less maintainable, breaks abstraction
- Using a separate profile table with FK to user — adds complexity, requires additional queries

### 2. Profile properties on AppIdentityUser
**Decision**: Store all profile fields directly on `AppIdentityUser` as nullable string and DateTime properties with sensible defaults.

**Rationale**:
- Single query to load user + profile in one operation
- No need for eager loading or separate joins
- Simpler programming model in handlers
- Profile photo stored as URL string (external storage responsibility)

**Fields**:
- `FullName` (string, max 255)
- `CulinaryTitle` (string, max 100)
- `Biography` (string, max 1000)
- `Location` (string, max 200)
- `ProfilePhotoUrl` (string, max 500)
- `CreatedAt` (DateTime, auto-set on registration)

**Alternatives considered**:
- Storing profile as JSON blob — harder to query, less type-safe
- Splitting location into city/country — overengineered for this phase

### 3. Migration strategy
**Decision**: Create a single EF Core migration to add the profile columns to `AspNetUsers` table via `AppDbContext`.

**Rationale**:
- Uses existing migration infrastructure
- Allows rollback if needed
- Clear audit trail of schema changes
- Ties the change to a specific point in version history

### 4. Handler registration and dependency injection
**Decision**: Update `Program.cs` to register `AppIdentityUser` with `MapIdentityApi<AppIdentityUser>()` and all handlers to use `UserManager<AppIdentityUser>`.

**Rationale**:
- Minimal change to existing wiring
- ASP.NET Core Identity already provides CRUD operations via `MapIdentityApi`
- All injected `UserManager` dependencies automatically get the extended type

### 5. Profile endpoint implementation
**Decision**: Create dedicated handlers `GetProfileHandler` and `UpdateProfileHandler` mapped to `/api/user/profile` (GET and PUT).

**Rationale**:
- Follows existing handler pattern in the codebase
- Separates profile operations from authentication (register/login)
- Allows for profile-specific validation and response formatting
- Clean test surface for profile-specific logic

## Risks / Trade-offs

**[Risk]** Database migration complexity in production
→ **Mitigation**: Create a simple additive migration with nullable columns (no data loss), include a pre-deployment validation test to confirm the migration runs successfully in a staging copy

**[Risk]** Null reference exceptions if profile fields are not initialized
→ **Mitigation**: Use default values (empty strings or null, consistently) in `AppIdentityUser` constructor and ensure handlers always provide a profile response, even if fields are empty

**[Risk]** Future profile complexity (e.g., address subobjects, expertise tags)
→ **Mitigation**: Current string fields are sufficient for the current spec; future extensions can add a separate `UserProfile` entity if needed without breaking the current model

**[Trade-off]** Storing all profile on `AppIdentityUser` means every user query fetches profile data
→ **Mitigation**: Profile fields are small (strings); the query cost is negligible. If profiling shows issues, a separate lazy-loaded table can be added later

## Migration Plan

1. Create `AppIdentityUser` class with profile properties
2. Add EF Core migration (`AddProfileFieldsToAppIdentityUser`)
3. Update `Program.cs` to wire `MapIdentityApi<AppIdentityUser>()`
4. Update all existing handlers to use `UserManager<AppIdentityUser>` (register, login, update profile)
5. Create `GetProfileHandler` and `UpdateProfileHandler`
6. Map new profile endpoints in feature registration
7. Create integration tests for profile GET/PUT
8. Deploy migration; validate data persistence

Rollback strategy: EF Core migration can be reversed with `Update-Database -Migration <previous>`; all code changes are backward-compatible because if rollback occurs, handlers gracefully use default profile values.

## Open Questions

None at this time. The profile fields, endpoint structure, and registration approach are clear enough to proceed with implementation.
