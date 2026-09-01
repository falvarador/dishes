## Purpose

Provides user registration and authentication operations that include profile data handling for a complete user identity system.

## ADDED Requirements

### Requirement: Register user with profile
The system SHALL provide an endpoint that creates a new user account with initial profile information.

#### Scenario: Successful registration with profile data
- **WHEN** a client sends `POST /api/auth/register` with email, password, and initial profile data (full name, culinary title, biography, location)
- **THEN** the system creates a new `AppIdentityUser` account and returns HTTP 201 with the created user's profile

#### Scenario: Missing required fields
- **WHEN** a client sends `POST /api/auth/register` with missing email or password
- **THEN** the system returns HTTP 400 with validation error details and does not create an account

#### Scenario: Duplicate email registration
- **WHEN** a client sends `POST /api/auth/register` with an email already registered in the system
- **THEN** the system returns HTTP 409 Conflict and does not create a duplicate account

### Requirement: Authenticate user and return profile
The system SHALL provide an endpoint that authenticates a user and returns their complete profile in the response.

#### Scenario: Successful login
- **WHEN** a client sends `POST /api/auth/login` with valid email and password
- **THEN** the system authenticates the user and returns HTTP 200 with the user's profile (including name, culinary title, biography, location, and profile photo)

#### Scenario: Invalid credentials
- **WHEN** a client sends `POST /api/auth/login` with an invalid email or incorrect password
- **THEN** the system returns HTTP 401 Unauthorized without logging the user in

#### Scenario: Nonexistent user
- **WHEN** a client sends `POST /api/auth/login` with an email that is not registered
- **THEN** the system returns HTTP 401 Unauthorized

### Requirement: User identity model includes profile fields
The system SHALL use an `AppIdentityUser` class extending ASP.NET Core Identity's `IdentityUser` with profile-specific properties.

#### Scenario: Profile fields available on user model
- **WHEN** any handler or service accesses a user loaded from the identity store
- **THEN** the user object includes profile fields: full name, culinary title, biography, location, profile photo URL, and created timestamp

#### Scenario: Profile fields initialized on account creation
- **WHEN** a new user is registered
- **THEN** all profile fields are initialized to appropriate defaults and are immediately available
