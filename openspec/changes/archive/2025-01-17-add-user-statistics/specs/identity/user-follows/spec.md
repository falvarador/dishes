## Purpose

Lets authenticated users follow and unfollow other creators so the system can calculate a creator's follower count.

## ADDED Requirements

### Requirement: Follow a creator
The system SHALL allow an authenticated user to follow another existing user.

#### Scenario: Follow another user
- **WHEN** an authenticated user sends `POST /api/user/follows/{userId}` for a different existing user
- **THEN** the system creates the follow relationship and returns HTTP 201 or 200

#### Scenario: Follow a user that does not exist
- **WHEN** an authenticated user sends `POST /api/user/follows/{userId}` for an unknown user id
- **THEN** the system returns HTTP 404 Not Found and does not create a follow relationship

#### Scenario: Unauthenticated follow request
- **WHEN** an unauthenticated client sends `POST /api/user/follows/{userId}`
- **THEN** the system returns HTTP 401 Unauthorized

### Requirement: Prevent self-follow
The system SHALL reject attempts by a user to follow themselves.

#### Scenario: User follows own id
- **WHEN** an authenticated user sends `POST /api/user/follows/{userId}` using their own user id
- **THEN** the system returns HTTP 400 Bad Request and does not create a follow relationship

### Requirement: Follow relationship is unique
The system SHALL keep at most one follow relationship per follower and followed user pair.

#### Scenario: Follow the same creator twice
- **WHEN** an authenticated user follows a creator they already follow
- **THEN** the system does not create a duplicate relationship and returns a success or conflict response without increasing the follower count

### Requirement: Unfollow a creator
The system SHALL allow an authenticated user to remove an existing follow relationship.

#### Scenario: Unfollow a followed creator
- **WHEN** an authenticated user who already follows a creator sends `DELETE /api/user/follows/{userId}`
- **THEN** the system removes the follow relationship and returns HTTP 204 or 200

#### Scenario: Unfollow a creator not currently followed
- **WHEN** an authenticated user sends `DELETE /api/user/follows/{userId}` for a user they do not follow
- **THEN** the system returns HTTP 404 Not Found

#### Scenario: Unauthenticated unfollow request
- **WHEN** an unauthenticated client sends `DELETE /api/user/follows/{userId}`
- **THEN** the system returns HTTP 401 Unauthorized
