## 1. Persistence

- [x] 1.1 Add `UserFollow` entity (`Id`, `FollowerUserId`, `FollowedUserId`, `CreatedAt`) and `DbSet<UserFollow>` on `AppDbContext`, then verify the solution builds
- [x] 1.2 Configure a unique index on (`FollowerUserId`, `FollowedUserId`) plus an index on `FollowedUserId`, then verify model configuration compiles
- [x] 1.3 Add an EF Core migration for `UserFollows` and verify `dotnet ef migrations list` includes it

## 2. User statistics slice

- [x] 2.1 Add `UserStatisticsResponse` DTO (`publishedRecipes`, `likesReceived`, `followers`) and verify DTO unit tests pass
- [x] 2.2 Implement `GetUserStatisticsHandler` that counts published recipes by `CreatorId`+`Published`, likes via `UserRecipeFavorite` on owned recipes, and followers via `UserFollow`, then verify handler unit tests cover zeros and mixed statuses
- [x] 2.3 Map `GET /api/user/statistics` through `IdentityEndpoints`, register the handler in DI, and verify an unauthenticated request returns 401

## 3. Follow / unfollow slice

- [x] 3.1 Implement `FollowUserHandler` for `POST /api/user/follows/{userId}` with self-follow 400, missing user 404, duplicate 409, and success 201, then verify those cases with tests
- [x] 3.2 Implement `UnfollowUserHandler` for `DELETE /api/user/follows/{userId}` with missing relationship 404, unauthenticated 401, and success 204, then verify those cases with tests
- [x] 3.3 Register follow handlers in DI and `IdentityEndpoints`, then verify the solution builds

## 4. Integration tests

- [x] 4.1 Add statistics integration tests for published-vs-draft counts, likes on own recipes vs others, follower counts, and per-user isolation, then verify they pass
- [x] 4.2 Add follow integration tests for follow, duplicate follow, self-follow, unfollow, and follower count change after follow/unfollow, then verify they pass
- [x] 4.3 Run `dishes.Server.Tests` and verify the full test project passes
