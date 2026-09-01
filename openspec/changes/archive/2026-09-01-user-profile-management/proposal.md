## Why

Users need to customize their profiles to reflect their identity and culinary expertise. Currently, the system only supports basic Identity user account management. This change extends the user model to store rich profile information and provides the necessary APIs for users to view and update their profiles.

## What Changes

- Extend `IdentityUser` with a custom `AppIdentityUser` class that includes profile-specific properties
- Add new endpoints for viewing and updating user profile information
- Create a database migration to add profile fields to the user table
- Update all Identity-related handlers and registrations to use `AppIdentityUser` instead of the base `IdentityUser`
- Ensure profile data is validated and persisted correctly through the application lifecycle

## Capabilities

### New Capabilities

- `identity/user-profile`: Profile viewing and editing operations for authenticated users, including name, culinary title, biography, location, and profile photo management

### Modified Capabilities

- `identity/user-auth`: Extend to ensure profile data is included with authentication responses and can be managed via dedicated profile endpoints

## Impact

- **Code affected**: User entity model, authentication handlers, middleware, and all places using ASP.NET Core Identity
- **Database**: New schema migration to add profile columns to AspNetUsers table
- **APIs**: New profile endpoints for GET and PUT operations
- **Dependencies**: No new external dependencies; uses existing ASP.NET Core Identity packages
- **Breaking changes**: None; base `IdentityUser` functionality remains unchanged
