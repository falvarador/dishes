## Context

See proposal.md for motivation. The server already uses VSA feature folders, Identity API endpoints under `/api/user`, and join entities (`UserBadge`, `UserRecipeFavorite`, `UserFollow`) to model user relationships. Achievements should follow this established pattern. There is no existing achievement unlock table; evaluation logic will be implemented in a dedicated handler. Identity user ids are strings on `AppIdentityUser`, while recipe `CreatorId` is `Guid`; achievement evaluation must convert consistently using `Guid.TryParse`.

## Goals / Non-Goals

**Goals:**
- Add a VSA slice that evaluates unlockable achievements on-demand
- Persist achievement unlocks in a new `UserAchievement` join entity
- Reuse existing recipe, favorite, follow, and user data for achievement criteria
- Expose achievements via `/api/user/achievements` following the Identity pattern
- Ensure evaluation happens at profile, statistics, and dedicated endpoint access
- Keep authorization consistent with other `/api/user` endpoints

**Non-Goals:**
- Frontend UI for achievement display (API only)
- Background jobs, notifications, or achievement change events
- Leaderboards or public achievement rankings
- Multiple achievement levels or progressive unlocks within an achievement
- Badge difference: badges are pre-defined static entities; achievements are unlockable states

## Decisions

1. **New `UserAchievement` join entity with immutable unlock date**
   - Store `UserId` (string, matches `AppIdentityUser.Id`), `AchievementId` (string constant/enum for the four achievements), and `UnlockedAt` (immutable DateTime).
   - Unique index on `(UserId, AchievementId)` enforces at most one unlock record per user per achievement.
   - Once inserted, unlock dates cannot be modified; immutability is a database constraint (no UPDATE path).
   - Alternative: In-memory cache or derived query logic. Rejected because persistence avoids redundant evaluation and allows future analytics.

2. **Four achievements as string constants in a static configuration class**
   - Define achievements as `id`, `name`, `description`, and `icon` URL in a single `AchievementDefinitions` class.
   - Achievments ids: `TopContributor`, `MasterChef`, `HotStreak`, `VideoStar`.
   - Alternative: Database table for achievement definitions. Rejected to avoid migration overhead and keep the four achievements as a stable configuration.

3. **On-demand evaluation via a separate `EvaluateAchievementsService`**
   - Implement logic for each achievement unlock criterion (5 recipes, 50 recipes, 7-day streak, video presence).
   - Service is injected into profile, statistics, and achievements handlers.
   - Evaluation runs before response serialization for credentials endpoint; profile and statistics handlers may also invoke it (application concern, not requirement).
   - Alternative: Separate background job or event listener. Rejected because on-demand evaluation is simpler for a small set of static achievements.

4. **Endpoints for achievements**
   - `GET /api/user/achievements` → authenticated current-user achievements (mimics `/api/user/statistics` and `/api/user/badges` pattern).
   - Registered through `IdentityEndpoints` and handled by `GetUserAchievementsHandler`.
   - Alternative: Include achievements in `/api/user/profile` or `/api/user/statistics`. Rejected to keep concerns separate and allow stat page to reuse the achievements handler without duplication.

5. **Achievement response DTO includes six fields**
   - `id`, `name`, `description`, `icon`, `unlockedAt`, `createdAt`.
   - `unlockedAt` is the date the unlock occurred (immutable after insertion).
   - `createdAt` is the database record creation timestamp (same as `unlockedAt` unless manually backfilled, which is not a client concern).
   - Alternative: Minimal (name, description, icon only). Rejected because `unlockedAt` is useful for sorting, chronology, and future analytics.

6. **Streak logic: 7 consecutive calendar days**
   - Group published recipes by calendar date (UTC), count unique dates.
   - Find the longest consecutive sequence of dates within the last 90 days (to avoid infinite lookback).
   - Unlock when sequence >= 7.
   - Alternative: `CreatedAt` time-only matching (ignore date boundaries). Rejected because calendar days are more intuitive for user engagement.
   - Caveat: If a user publishes multiple recipes on the same day, that day counts once. If a day is missed, the streak resets from that point.

7. **Video presence check: any recipe with non-null `VideoUrl`**
   - Check if the user has published at least one recipe where `VideoUrl` is not null and not empty.
   - Alternative: Count video uploads via a separate `RecipeMedia` table. Rejected because `VideoUrl` already captures video presence in the current schema.

8. **Achievement immutability in UI and API response**
   - Returned achievements never include a `locked` or `progress` field; only unlocked achievements are returned.
   - Alternative: Return all achievements with progress/lock status. Rejected to keep the response simple and match the spec's intent (return unlocked only).

## Risks / Trade-offs

- **Calendar date precision**: If evaluation occurs at different times on the same calendar day, streak logic must handle UTC date boundaries consistently. Mitigation: Use `DateTime.UtcNow.Date` for all date arithmetic.
- **Denormalized evaluation**: Deriving unlocks on each request may become slow if recipe/favorite/follow counts grow very large. Mitigation: Index recipes by `CreatorId` and status, index favorites and follows for quick queries. If performance degrades, add application-level caching (not in this iteration).
- **Streak reset complexity**: A missed publishing day resets the streak. If a user has published 6 days in a row and then misses a day, they must start over. Mitigation: This is the intended behavior per spec; unclear if users should be notified of a broken streak (out of scope for this change).
- **Video Star ambiguity**: Schema has `VideoUrl` but unclear if a short clip snippet uploaded earlier counts as a "video recipe". Mitigation: Treat any non-null, non-empty `VideoUrl` as sufficient; if definition narrows, evaluation logic is a single-line change.

## Open Questions

- None. Achievement set, trigger points, and evaluation strategy are stable per clarified scope.
