## Why

The system currently provides user statistics and badges, but lacks a gamification mechanism to unlock hidden achievements based on user milestones and activity streaks. This feature motivates user engagement by recognizing specific accomplishments (Top Contributor, Master Chef, Hot Streak, Video Star) and displaying them prominently in the user profile. Unlockable achievements are evaluated on-demand when users access their profile or statistics view, and results are persisted to avoid redundant calculation.

## What Changes

- New `UserAchievement` entity to track which achievements each user has unlocked and the unlock date
- New GET `/api/user/achievements` endpoint to retrieve a user's unlocked achievements with name, description, icon, and unlock date
- New `GetUserAchievements` query handler following the existing Identity vertical-slice pattern
- New `AchievementResponse` DTO for structured achievement data
- Achievements are evaluated on-demand (not via background jobs) when:
  - A user accesses their profile
  - A user views their statistics
  - A user calls the `/api/user/achievements` endpoint
- Four achievements in scope:
  - **Top Contributor**: Unlock when user has 5+ published recipes
  - **Master Chef**: Unlock when user has 50+ published recipes
  - **Hot Streak**: Unlock when user has published at least 1 recipe in each of the last 7 consecutive days
  - **Video Star**: Unlock when user has published at least 1 recipe with a video
- Achievements remain unlocked once obtained; unlock date is immutable

## Capabilities

### New Capabilities
- `identity/user-achievements`: User-facing achievements system with on-demand evaluation, persistence, and retrieval via `/api/user/achievements` endpoint

### Modified Capabilities
- None (no existing spec-level requirements are changing)

## Impact

- **Code Areas**: Identity feature folder (`Features/Identity`), database context, EF Core migrations
- **APIs**: New `/api/user/achievements` endpoint (GET, authenticated)
- **Database**: New `UserAchievement` table with foreign key to `AspNetUsers`, unique index on (UserId, AchievementId)
- **Dependencies**: Uses existing EF Core, ASP.NET Identity, and minimal APIs infrastructure
- **Compatibility**: No breaking changes; achievements are additive and evaluated on the authenticated user's context only
