## Why

Creators currently have no aggregated view of their reach. RF-05 requires showing published-recipe count, accumulated likes, and follower count so cooks can measure engagement. Likes already exist as recipe favorites; followers do not, so the product also needs a follow relationship now.

## What Changes

- Add an authenticated creator statistics endpoint that returns:
  - Published Recipes: count of recipes owned by the user with published status
  - Likes Received: count of `UserRecipeFavorite` records on the creator's recipes
  - Followers: count of users following the creator
- Add follow/unfollow so Followers can be calculated:
  - follow a creator
  - unfollow a creator
  - a user cannot follow themselves
  - following the same creator twice is idempotent or rejected with a clear client error
- Keep statistics read-only and derived from existing data; do not store denormalized counters in this change.
- No **BREAKING** API changes to existing recipe, favorite, profile, or account-settings endpoints.

## Capabilities

### New Capabilities
- `identity/user-statistics`: Authenticated retrieval of a creator's published-recipe, likes-received, and follower metrics
- `identity/user-follows`: Authenticated follow/unfollow of another creator and uniqueness of the follow relationship

### Modified Capabilities
- None. Existing recipe publish and favorite behaviors stay the same; statistics only read them.

## Impact

- Server vertical slice under Identity (and possibly Account) following VSA handlers/DTOs/endpoints
- New persistence for follow relationships (`UserFollow` or equivalent)
- New authorized endpoints for statistics and follow/unfollow
- Existing `Recipes` (published status) and `UserRecipeFavorite` remain the source of Published Recipes and Likes Received
- Unit and integration tests for DTO mapping, aggregation, authorization, self-follow, and independent user metrics
- No frontend work in this change unless a later apply request includes it
