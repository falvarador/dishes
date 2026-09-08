## ADDED Requirements

### Requirement: Creator can update a published recipe
The system SHALL allow an authenticated creator to edit and update a recipe that they originally created and published.

#### Scenario: Creator updates own published recipe
- **WHEN** the authenticated creator submits valid updates for a published recipe they own
- **THEN** the system persists the updated recipe data and returns a successful result

#### Scenario: Creator submits invalid update payload
- **WHEN** the authenticated creator submits an update with missing or invalid required values
- **THEN** the system rejects the update and returns validation errors without modifying persisted data

### Requirement: Creator can delete a published recipe
The system SHALL allow an authenticated creator to delete a published recipe that they originally created.

#### Scenario: Creator deletes own published recipe
- **WHEN** the authenticated creator requests deletion of a published recipe they own
- **THEN** the system removes the recipe and returns a successful result

### Requirement: Published recipe management enforces ownership
The system SHALL prevent users from updating or deleting published recipes created by another user.

#### Scenario: Non-owner attempts to update published recipe
- **WHEN** an authenticated user who is not the recipe creator requests an update on that published recipe
- **THEN** the system denies the operation and returns a forbidden authorization result

#### Scenario: Non-owner attempts to delete published recipe
- **WHEN** an authenticated user who is not the recipe creator requests deletion of that published recipe
- **THEN** the system denies the operation and returns a forbidden authorization result
