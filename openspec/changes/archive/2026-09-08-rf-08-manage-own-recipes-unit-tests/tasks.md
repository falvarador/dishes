## 1. Ownership enforcement in published recipe commands

- [x] 1.1 Locate update and delete command handlers/services for recipes and verify current owner identity source used by the application layer
- [x] 1.2 Implement ownership guard in update flow (`CreatorId == AuthenticatedUserId`) and verify non-owner update returns forbidden result
- [x] 1.3 Implement ownership guard in delete flow (`CreatorId == AuthenticatedUserId`) and verify non-owner delete returns forbidden result
- [x] 1.4 Verify owner update/delete behavior remains successful after authorization guard changes

## 2. Unit tests for update flow

- [x] 2.1 Add/update test for owner successfully updating a published recipe and verify the update success test passes
- [x] 2.2 Add/update test for invalid update payload rejection and verify validation/assertion test passes
- [x] 2.3 Add/update test for non-owner update attempt returning forbidden result and verify authorization test passes

## 3. Unit tests for delete flow and regression

- [x] 3.1 Add/update test for owner successfully deleting a published recipe and verify deletion success test passes
- [x] 3.2 Add/update test for non-owner delete attempt returning forbidden result and verify authorization test passes
- [x] 3.3 Run targeted recipe unit tests and verify all modified/new tests pass
- [x] 3.4 Run relevant test project suite and verify no regressions in existing tests
