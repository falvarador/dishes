## Purpose

Provides CRUD (create, read, update, delete) operations for `Ingredient` records so ingredients can be managed independently of dishes and reused across multiple dishes.

## Requirements

### Requirement: List ingredients
The system SHALL provide an endpoint that returns all ingredients.

#### Scenario: Retrieve all ingredients
- **WHEN** a client sends `GET /api/ingredients`
- **THEN** the system returns HTTP 200 with a JSON array of all ingredients

### Requirement: Get ingredient by id
The system SHALL provide an endpoint that returns a single ingredient by its identifier.

#### Scenario: Ingredient exists
- **WHEN** a client sends `GET /api/ingredients/{id}` for an existing ingredient id
- **THEN** the system returns HTTP 200 with the matching ingredient

#### Scenario: Ingredient does not exist
- **WHEN** a client sends `GET /api/ingredients/{id}` for an id that does not exist
- **THEN** the system returns HTTP 404

### Requirement: Create ingredient
The system SHALL provide an endpoint to create a new ingredient with a required, non-empty name of at most 200 characters.

#### Scenario: Valid ingredient created
- **WHEN** a client sends `POST /api/ingredients` with a valid name
- **THEN** the system creates the ingredient, returns HTTP 201, and includes the created resource with a generated id

#### Scenario: Invalid ingredient rejected
- **WHEN** a client sends `POST /api/ingredients` with a missing or empty name, or a name longer than 200 characters
- **THEN** the system returns HTTP 400 with validation error details and does not create a record

### Requirement: Update ingredient
The system SHALL provide an endpoint to update the name of an existing ingredient.

#### Scenario: Existing ingredient updated
- **WHEN** a client sends `PUT /api/ingredients/{id}` with a valid name for an existing ingredient
- **THEN** the system updates the ingredient and returns HTTP 200 (or 204) with no data loss to related dishes

#### Scenario: Update non-existent ingredient
- **WHEN** a client sends `PUT /api/ingredients/{id}` for an id that does not exist
- **THEN** the system returns HTTP 404

#### Scenario: Update with invalid data
- **WHEN** a client sends `PUT /api/ingredients/{id}` with a missing or empty name, or a name longer than 200 characters
- **THEN** the system returns HTTP 400 and does not modify the existing record

### Requirement: Delete ingredient
The system SHALL provide an endpoint to delete an existing ingredient.

#### Scenario: Existing ingredient deleted
- **WHEN** a client sends `DELETE /api/ingredients/{id}` for an existing ingredient
- **THEN** the system removes the ingredient and returns HTTP 204

#### Scenario: Delete non-existent ingredient
- **WHEN** a client sends `DELETE /api/ingredients/{id}` for an id that does not exist
- **THEN** the system returns HTTP 404
