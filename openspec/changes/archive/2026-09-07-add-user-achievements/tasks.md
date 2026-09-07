## 1. Persistence Layer

- [x] 1.1 Add `UserAchievement` entity (`Id`, `UserId` string, `AchievementId` string, `UnlockedAt` DateTime) and `DbSet<UserAchievement>` on `AppDbContext`, then verify the solution builds
- [x] 1.2 Configure a unique index on (`UserId`, `AchievementId`) and an index on `UserId` for efficient lookups, then verify model configuration compiles
- [x] 1.3 Add an EF Core migration for `UserAchievements` and verify `dotnet ef migrations list` includes it

## 2. Achievement Definitions and Evaluation Service

- [x] 2.1 Create static `AchievementDefinitions` class with four achievements (TopContributor, MasterChef, HotStreak, VideoStar) including id, name, description, and icon URL, then verify the class compiles and is accessible
- [x] 2.2 Implement `EvaluateAchievementsService` with logic for each achievement unlock criterion:
  - TopContributor: 5+ published recipes
  - MasterChef: 50+ published recipes
  - HotStreak: published ≥1 recipe in each of 7 consecutive calendar days
  - VideoStar: ≥1 published recipe with non-null VideoUrl
  - Verify each criterion is evaluated correctly with unit tests covering edge cases (zero recipes, exact thresholds, missing videos, broken streaks)
- [x] 2.3 Implement `IEvaluateAchievementsService.EvaluateAndPersistAsync(userId)` to insert newly unlocked achievements (skipping duplicates via unique index) and return all unlocked achievements, then verify unit tests cover mixed states

## 3. Achievements Response DTO

- [x] 3.1 Add `AchievementResponse` DTO with fields: `id`, `name`, `description`, `icon`, `unlockedAt`, `createdAt`, then verify DTO unit tests pass and serializes to JSON correctly

## 4. Achievements Query Handler

- [x] 4.1 Implement `GetUserAchievementsHandler` that:
  - Invokes `EvaluateAchievementsService.EvaluateAndPersistAsync(userId)` to ensure up-to-date unlocks
  - Queries `UserAchievements` for the current user
  - Maps to `AchievementResponse` DTO
  - Returns the list ordered by `UnlockedAt` descending
  - Then verify handler unit tests cover zero achievements, multiple achievements, and correct DTO field mapping

## 5. Endpoint Mapping and Dependency Injection

- [x] 5.1 Register `EvaluateAchievementsService` in DI as `IEvaluateAchievementsService` with scoped lifetime, then verify DI configuration compiles
- [x] 5.2 Register `GetUserAchievementsHandler` in DI, then verify DI configuration compiles
- [x] 5.3 Map `GET /api/user/achievements` through `IdentityEndpoints`, then verify an unauthenticated request returns 401 and an authenticated request returns 200 with achievement array

## 6. Achievement Evaluation Triggers

- [x] 6.1 Integrate achievement evaluation into the profile handler so evaluations occur when a user accesses `/api/user/profile`, then verify integration test passes
- [x] 6.2 Integrate achievement evaluation into the statistics handler so evaluations occur when a user accesses `/api/user/statistics`, then verify integration test passes

## 7. Integration Tests

- [x] 7.1 Add integration tests for GetUserAchievementsHandler covering:
  - Unauthenticated request returns 401
  - User with no unlocked achievements returns empty array
  - User with multiple achievements returns array ordered by unlock date
  - Verify all tests pass
- [x] 7.2 Add integration tests for achievement evaluation covering:
  - TopContributor unlock at recipe count 5
  - MasterChef unlock at recipe count 50
  - HotStreak unlock after 7 consecutive publishing days
  - VideoStar unlock when first video recipe is published
  - Verify all tests pass
- [x] 7.3 Add integration test for immutability: unlock date and achievement presence remain unchanged after subsequent profile/statistics requests, then verify test passes
- [x] 7.4 Add integration test for per-user isolation: two different users have independent achievement states, then verify test passes
- [x] 7.5 Run `dishes.Server.Tests` and verify the full test project passes (no failures, all new tests green)
