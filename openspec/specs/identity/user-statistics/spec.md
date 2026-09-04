## Purpose

Provides authenticated creators with derived engagement metrics: published recipe count, likes received from favorites, and follower count.

## ADDED Requirements

### Requirement: View own creator statistics
The system SHALL provide an authenticated endpoint that returns the current user's creator statistics as non-negative integers for published recipes, likes received, and followers.

#### Scenario: Retrieve statistics for authenticated creator
- **WHEN** an authenticated user sends `GET /api/user/statistics`
- **THEN** the system returns HTTP 200 with `publishedRecipes`, `likesReceived`, and `followers`

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated client sends `GET /api/user/statistics`
- **THEN** the system returns HTTP 401 Unauthorized

### Requirement: Count published recipes owned by the user
The system SHALL count only recipes whose creator is the authenticated user and whose status is published. Draft and unpublished recipes MUST NOT be included.

#### Scenario: Mixed published and draft recipes
- **WHEN** the authenticated user owns 3 published recipes and 2 draft recipes
- **THEN** `publishedRecipes` is 3

#### Scenario: User has no published recipes
- **WHEN** the authenticated user owns no published recipes
- **THEN** `publishedRecipes` is 0

### Requirement: Count likes received from recipe favorites
The system SHALL count likes received as the number of `UserRecipeFavorite` records whose recipe belongs to the authenticated user. Ratings MUST NOT be counted as likes.

#### Scenario: Favorites on the creator's recipes
- **WHEN** other users have created 5 favorites across the authenticated user's recipes
- **THEN** `likesReceived` is 5

#### Scenario: Favorites on other creators' recipes are excluded
- **WHEN** the authenticated user has favorited other creators' recipes and nobody has favorited the authenticated user's recipes
- **THEN** `likesReceived` is 0

#### Scenario: No favorites exist
- **WHEN** none of the authenticated user's recipes have favorites
- **THEN** `likesReceived` is 0

### Requirement: Count followers of the authenticated user
The system SHALL count followers as the number of follow relationships where the authenticated user is the followed creator.

#### Scenario: Multiple distinct followers
- **WHEN** 4 distinct users follow the authenticated user
- **THEN** `followers` is 4

#### Scenario: User has no followers
- **WHEN** nobody follows the authenticated user
- **THEN** `followers` is 0

### Requirement: Statistics are isolated per user
The system SHALL return statistics derived only from the authenticated user's recipes and follow relationships.

#### Scenario: Two creators have independent metrics
- **WHEN** user A has 2 published recipes and 1 follower and user B has 5 published recipes and 3 followers
- **THEN** user A's statistics request returns A's metrics and user B's statistics request returns B's metrics
