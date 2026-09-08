## Context

The existing `recipes` capability covers creation, publication, and detail viewing, but it does not explicitly define creator-managed maintenance for published recipes. See `proposal.md` (Why) for motivation. The current architecture is vertical-slice based in `dishes.Server`, with command handlers as the enforcement point for authorization and validation, and corresponding unit tests in the test project.

## Goals / Non-Goals

**Goals:**
- Enforce creator ownership in update and delete flows for published recipes.
- Return forbidden results when a non-owner attempts to update or delete a published recipe.
- Add unit-test coverage for success, validation, and forbidden scenarios while keeping test structure aligned with existing architecture.

**Non-Goals:**
- Introduce new recipe management UX or new API routes.
- Refactor unrelated recipe features or persistence abstractions.
- Add integration or end-to-end tests in this change.

## Decisions

1. Enforce ownership at the command-handler/application boundary before any mutation.
   - Rationale: handlers already coordinate authorization and domain operations, making them the correct architectural point to compare `Recipe.CreatorId` with `AuthenticatedUserId`.
   - Alternative considered: endpoint/controller-only validation. Rejected because business protection could be bypassed by internal entry points.

2. Keep update/delete behavior explicit: owner succeeds, non-owner is forbidden.
   - Rationale: RF-08 requires creator-only management, so both positive and negative authorization outcomes are mandatory behavior.
   - Alternative considered: masking with not-found for non-owner. Rejected for this change because requirement calls for explicit restriction feedback.

3. Reuse existing test fixtures/builders and assert observable results.
   - Rationale: preserves consistency with existing suite and avoids brittle tests tied to private internals.
   - Alternative considered: custom setup and deep mocks per test. Rejected due to duplication and maintenance cost.

## Risks / Trade-offs

- [Risk] Ownership logic may be distributed across handler and service layers. → Mitigation: centralize the check in the same application path used by update/delete commands.
- [Risk] Behavior mismatch with current API error mapping. → Mitigation: assert the handler/service result contract and keep endpoint mapping consistent.
- [Trade-off] Unit tests provide fast confidence but do not validate full HTTP pipeline behavior. → Mitigation: keep integration coverage as future work.

## Migration Plan

- No data migration required.
- Implement ownership checks in update/delete flows first, then add/adjust tests for owner and non-owner scenarios.
- Rollback is limited to reverting handler/service and test changes if needed.
