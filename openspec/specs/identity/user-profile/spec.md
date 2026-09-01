## Purpose

Enables authenticated users to view and manage their profile information including full name, culinary title, biography, location, and profile photo.

## ADDED Requirements

### Requirement: View user profile
The system SHALL provide an endpoint that returns the authenticated user's complete profile information.

#### Scenario: Retrieve own profile
- **WHEN** an authenticated user sends `GET /api/user/profile`
- **THEN** the system returns HTTP 200 with the user's profile data including full name, culinary title, biography, location, profile photo URL, and account creation timestamp

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated client sends `GET /api/user/profile`
- **THEN** the system returns HTTP 401 Unauthorized

### Requirement: Update user profile
The system SHALL provide an endpoint that allows authenticated users to update their profile information with validation.

#### Scenario: Valid profile update
- **WHEN** an authenticated user sends `PUT /api/user/profile` with valid profile data (full name, culinary title, biography, location, profile photo URL)
- **THEN** the system updates the user's profile and returns HTTP 200 with the updated profile

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated client sends `PUT /api/user/profile`
- **THEN** the system returns HTTP 401 Unauthorized

#### Scenario: Invalid profile data
- **WHEN** an authenticated user sends `PUT /api/user/profile` with invalid data (e.g., full name exceeding 255 characters, culinary title exceeding 100 characters)
- **THEN** the system returns HTTP 400 with validation error details and does not modify the profile

### Requirement: Profile data persistence
The system SHALL ensure all profile fields are properly persisted in the user identity store.

#### Scenario: Profile data saved on registration
- **WHEN** a new user registers, the system creates an `AppIdentityUser` record with profile fields initialized to default values
- **THEN** the profile fields are correctly stored and retrievable

#### Scenario: Profile data maintained through authentication
- **WHEN** an authenticated user's profile is retrieved, all previously saved profile fields are returned
- **THEN** the profile data has been preserved and is consistent with the last update
