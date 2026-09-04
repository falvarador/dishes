## Context

See proposal.md for motivation. The server already uses VSA feature folders, Identity API endpoints under `/api/user`, recipe `Status` (`Draft`/`Published`), and `UserRecipeFavorite` as the like analog. There is no follow model. Identity user ids are strings on `AppIdentityUser` while recipe `CreatorId` and favorite `UserId` are `Guid`; statistics and follows MUST convert consistently using `Guid.TryParse` of the authenticated user id.

## Goals / Non-Goals

**Goals:**
- Add a VSA slice that derives creator statistics on read
- Persist a unique follow relationship so follower counts are real
- Reuse existing recipe and favorite data rather than introducing like counters
- Keep authorization consistent with other `/api/user` endpoints

**Non-Goals:**
- Frontend UI for a statistics panel or follow button
- Denormalized counters, caching, or background aggregation
- Follower/following lists, activity feeds, or notifications
- Counting ratings as likes
- Public statistics for arbitrary user ids (this change returns only the authenticated user's metrics)

## Decisions

1. **Place statistics and follows under Identity, not Account**
   - Statistics describe a creator identity, matching `/api/user/profile` and `/api/user/badges`.
   - Account settings remain profile-field updates.
   - Alternative: Account settings payload. Rejected because it mixes mutable profile fields with derived metrics and follow mutations.

2. **Derive counts on each GET instead of stored counters**
   - Published recipes: `Recipes` where `CreatorId == currentUserGuid` and `Status == Published`.
   - Likes received: `UserRecipeFavorites` joined to recipes owned by the current user.
   - Followers: `UserFollows` where `FollowedUserId == currentUserId`.
   - Alternative: increment columns on publish/favorite/follow. Rejected for this change to avoid sync bugs; volume is expected to stay small enough for COUNT queries.

3. **New `UserFollow` entity with a unique (FollowerUserId, FollowedUserId) index**
   - Store both ids as strings to match `AppIdentityUser.Id`.
   - Unique index enforces at most one follow pair.
   - Self-follow is a handler validation, not a database constraint.
   - Alternative: many-to-many on `AppIdentityUser`. Rejected to keep Identity user mapping simple and follow the `UserBadge` / `UserRecipeFavorite` join-entity style.

4. **Follow duplicate handling returns 409 Conflict**
   - Specs allow success or conflict; 409 makes uniqueness visible to clients and keeps follower count unchanged.
   - Missing followed user: 404.
   - Self-follow: 400.
   - Unfollow when no relationship exists: 404.

5. **Endpoints**
   - `GET /api/user/statistics` → authenticated current-user metrics
   - `POST /api/user/follows/{userId}` → follow
   - `DELETE /api/user/follows/{userId}` → unfollow
   - Map through `IdentityEndpoints` and register handlers in DI like badges/profile.
   - Alternative: `/api/account/statistics`. Rejected to keep identity metrics with `/api/user`.

6. **Tests follow existing VSA patterns**
   - DTO unit tests first
   - Integration tests using `CustomWebApplicationFactory`, live `IServiceScope`, `/api/user/login`, and email-as-username Identity users
   - Cover unauthenticated access, published-vs-draft, favorites-on-own-recipes vs others, follower increments, self-follow, and isolation between users

## Risks / Trade-offs

- [Guid vs string user ids] → Parse Identity id to Guid only when querying `Recipes`/`UserRecipeFavorite`; keep follow ids as strings. Invalid id format returns 400/401 consistently with recipe handlers.
- [COUNT queries grow with data] → Accept for v1; add indexes on `Recipes.CreatorId`, `UserRecipeFavorites.RecipeId`, and `UserFollows.FollowedUserId`. Revisit denormalization later if needed.
- [Follow feature expands RF-05] → Keep list/feed/notifications out of this change so statistics remain the acceptance target.
- [Duplicate follow races] → Unique index is the source of truth; map unique-constraint failures to 409.

## Migration Plan

- Add `UserFollow` and EF configuration; create a migration when the apply phase runs.
- Deploy schema before enabling follow endpoints in production.
- Rollback: drop follow endpoints and the `UserFollows` table; statistics could remain if follow count is always 0, but the intended rollback is to remove both slices together.

## Open Questions

- Whether a later change should expose another user's public statistics. Not needed for RF-05.
