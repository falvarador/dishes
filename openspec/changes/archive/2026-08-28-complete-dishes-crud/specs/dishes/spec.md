## Purpose

Provides CRUD (create, read, update, delete) operations for `Dish` records so dishes can be managed independently and associated with reusable ingredients.

## ADDED Requirements

### Requirement: List dishes
The system SHALL provide an endpoint that returns all dishes, including their associated ingredients.

#### Scenario: Retrieve all dishes
- **WHEN** a client sends `GET /api/dishes`
- **THEN** the system returns HTTP 200 with a JSON array of all dishes, each including its ingredients

### Requirement: Get dish by id
The system SHALL provide an endpoint that returns a single dish by its identifier.

#### Scenario: Dish exists
- **WHEN** a client sends `GET /api/dishes/{id}` for an existing dish id
- **THEN** the system returns HTTP 200 with the matching dish

#### Scenario: Dish does not exist
- **WHEN** a client sends `GET /api/dishes/{id}` for an id that does not exist
- **THEN** the system returns HTTP 404

### Requirement: Create dish
The system SHALL provide an endpoint to create a new dish with a required, non-empty name of at most 200 characters.

#### Scenario: Valid dish created
- **WHEN** a client sends `POST /api/dishes` with a valid name
- **THEN** the system creates the dish, returns HTTP 201, and includes the created resource with a generated id

#### Scenario: Invalid dish rejected
- **WHEN** a client sends `POST /api/dishes` with a missing or empty name, or a name longer than 200 characters
- **THEN** the system returns HTTP 400 with validation error details and does not create a record

### Requirement: Update dish
The system SHALL provide an endpoint to update the name of an existing dish.

#### Scenario: Existing dish updated
- **WHEN** a client sends `PUT /api/dishes/{id}` with a valid name for an existing dish
- **THEN** the system updates the dish and returns HTTP 200 with no data loss to related ingredients

#### Scenario: Update non-existent dish
- **WHEN** a client sends `PUT /api/dishes/{id}` for an id that does not exist
- **THEN** the system returns HTTP 404

#### Scenario: Update with invalid data
- **WHEN** a client sends `PUT /api/dishes/{id}` with a missing or empty name, or a name longer than 200 characters
- **THEN** the system returns HTTP 400 and does not modify the existing record

### Requirement: Delete dish
The system SHALL provide an endpoint to delete an existing dish.

#### Scenario: Existing dish deleted
- **WHEN** a client sends `DELETE /api/dishes/{id}` for an existing dish
- **THEN** the system removes the dish and returns HTTP 204

#### Scenario: Delete non-existent dish
- **WHEN** a client sends `DELETE /api/dishes/{id}` for an id that does not exist
- **THEN** the system returns HTTP 404
