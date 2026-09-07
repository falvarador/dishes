## Purpose

Provides authenticated users with a curated set of unlockable achievements that recognize milestones and engagement streaks (Top Contributor, Master Chef, Hot Streak, Video Star). Achievements are evaluated on-demand when users access their profile or statistics, persisted to avoid redundant calculation, and displayed chronologically by unlock date.

## ADDED Requirements

### Requirement: View own user achievements
The system SHALL provide an authenticated endpoint that returns the current user's unlocked achievements with name, description, icon URL, and unlock date.

#### Scenario: Retrieve achievements for authenticated user
- **WHEN** an authenticated user sends `GET /api/user/achievements`
- **THEN** the system returns HTTP 200 with an array of achievement objects, each containing name, description, icon, and unlockDate

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated client sends `GET /api/user/achievements`
- **THEN** the system returns HTTP 401 Unauthorized

#### Scenario: User has no unlocked achievements
- **WHEN** an authenticated user has not yet unlocked any achievements
- **THEN** the system returns HTTP 200 with an empty array

### Requirement: Achievement unlock is permanent
The system SHALL persist achievement unlocks and MUST NOT remove a user's achievement or change its unlock date once obtained.

#### Scenario: Achievement remains unlocked
- **WHEN** a user unlocks the Master Chef achievement on 2025-01-15
- **THEN** subsequent checks always show Master Chef with unlockDate 2025-01-15, regardless of subsequent activity

#### Scenario: Achievement is not removed if criteria no longer met
- **WHEN** a user unlocks Hot Streak by publishing in 7 consecutive days, then does not publish for 30 days
- **THEN** Hot Streak remains unlocked in the user's achievement list

### Requirement: Top Contributor achievement
The system SHALL automatically unlock the Top Contributor achievement when the user publishes their 5th recipe.

#### Scenario: Unlock at recipe count threshold
- **WHEN** an authenticated user publishes their 5th recipe
- **THEN** the system evaluates achievements on next profile/statistics access and Top Contributor becomes unlocked with current date as unlockDate

#### Scenario: Already unlocked before threshold check
- **WHEN** a user has already unlocked Top Contributor
- **THEN** the achievement is not re-unlocked and the original unlock date is preserved

### Requirement: Master Chef achievement
The system SHALL automatically unlock the Master Chef achievement when the user publishes their 50th recipe.

#### Scenario: Unlock at recipe count threshold
- **WHEN** an authenticated user publishes their 50th recipe
- **THEN** the system evaluates achievements on next profile/statistics access and Master Chef becomes unlocked with current date as unlockDate

#### Scenario: Requires published recipes only
- **WHEN** a user has 50+ draft recipes but only 49 published recipes
- **THEN** Master Chef is not unlocked

### Requirement: Hot Streak achievement
The system SHALL automatically unlock the Hot Streak achievement when the user publishes at least one recipe in each of 7 consecutive calendar days.

#### Scenario: Unlock after 7 consecutive publishing days
- **WHEN** an authenticated user publishes at least one recipe on each day from 2025-01-01 through 2025-01-07 (7 consecutive days)
- **THEN** the system evaluates achievements on next profile/statistics access and Hot Streak becomes unlocked with current date (day 7 or later) as unlockDate

#### Scenario: Streak is broken by a missing day
- **WHEN** a user publishes on 2025-01-01 through 2025-01-06 (6 days) but does not publish on 2025-01-07
- **THEN** Hot Streak is not unlocked; the streak resets

#### Scenario: Multiple consecutive days from same recipe
- **WHEN** a user publishes multiple recipes on the same day
- **THEN** that calendar day counts as one day toward the streak (not multiple contributions)

### Requirement: Video Star achievement
The system SHALL automatically unlock the Video Star achievement when the user publishes at least one recipe with an associated video.

#### Scenario: Unlock when first video-enabled recipe is published
- **WHEN** an authenticated user publishes a recipe with a video attachment
- **THEN** the system evaluates achievements on next profile/statistics access and Video Star becomes unlocked with current date as unlockDate

#### Scenario: Multiple videos or later videos do not re-unlock
- **WHEN** a user has already unlocked Video Star and publishes a second video-enabled recipe
- **THEN** Video Star remains unlocked with the original unlock date preserved

### Requirement: Achievements are evaluated on-demand
The system SHALL evaluate achievements when:
1. An authenticated user accesses their own profile page
2. An authenticated user accesses their own statistics page
3. An authenticated user calls GET /api/user/achievements

#### Scenario: Evaluation occurs at profile access
- **WHEN** an authenticated user accesses their profile page
- **THEN** the system evaluates all achievement unlock criteria and persists any newly unlocked achievements

#### Scenario: Evaluation occurs at statistics access
- **WHEN** an authenticated user accesses their statistics page
- **THEN** the system evaluates all achievement unlock criteria and persists any newly unlocked achievements

#### Scenario: Evaluation occurs at endpoint call
- **WHEN** an authenticated user calls GET /api/user/achievements
- **THEN** the system evaluates all achievement unlock criteria, persists any newly unlocked achievements, and returns the current (possibly updated) achievement list

### Requirement: Achievements are isolated per user
The system SHALL return achievements derived only from the authenticated user's own recipes, publishing activity, and unlock records.

#### Scenario: Two users have independent achievement states
- **WHEN** user A has unlocked Top Contributor and Master Chef, and user B has unlocked only Hot Streak
- **THEN** user A's achievements request returns A's two achievements and user B's request returns B's one achievement

#### Scenario: No cross-contamination from other users' activity
- **WHEN** user A requests achievements while user B has recently achieved Video Star
- **THEN** user A's achievements list is unaffected by user B's activity

### Requirement: Achievement response structure
The system SHALL return each achievement as a JSON object with the following properties:
- `id` (string, UUID): Unique identifier for the achievement definition
- `name` (string): Human-readable achievement name (e.g., "Top Contributor")
- `description` (string): Brief explanation of the achievement
- `icon` (string, URL): PNG or SVG icon URL for visual display
- `unlockedAt` (ISO 8601 datetime string): Date and time the user unlocked the achievement
- `createdAt` (ISO 8601 datetime string): Date and time the achievement record was created

#### Scenario: JSON schema compliance
- **WHEN** an authenticated user calls GET /api/user/achievements with one unlocked achievement
- **THEN** the response contains fields id, name, description, icon, unlockedAt, and createdAt in valid JSON format

#### Scenario: Empty list format
- **WHEN** an authenticated user with no unlocked achievements calls GET /api/user/achievements
- **THEN** the response is a valid JSON array (empty or with zero elements)
