## Why

RF-08 requires that recipe creators can manage their own published recipes by editing, updating, and deleting them. Existing endpoints may allow updates/deletes without an explicit owner restriction, so the change must enforce authorization and verify it with unit tests aligned with the architecture.

## What Changes

- Add requirements for owner-managed recipe maintenance (edit, update, delete) for published recipes.
- Implement ownership authorization checks in update and delete flows so only the recipe creator can mutate or remove the recipe.
- Define expected system responses for successful updates/deletes and forbidden actions.
- Add unit tests for command handlers/services covering owner success paths and non-owner forbidden paths.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `recipes`: Extend requirements to include creator-only edit, update, and delete operations for published recipes.

## Impact

- Backend recipe application layer (update/delete handlers/services and ownership validation).
- Recipe domain/application unit tests for positive, validation, and authorization failure scenarios.
- API behavior for update/delete commands where forbidden responses must be returned for non-owners.
