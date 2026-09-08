## 1. Backend search by title and ingredients

- [x] 1.1 Add a shared recipe search query parameter to the recipes listing flow and verify the recipes endpoint accepts search input without breaking existing list behavior.
- [x] 1.2 Implement case-insensitive partial match filtering by recipe title OR ingredient name with deduplicated results and verify automated tests cover title-only, ingredient-only, and combined match scenarios.
- [x] 1.3 Ensure empty or whitespace search input returns the default unfiltered catalog result and verify with a test asserting parity with baseline listing output.

## 2. Header and catalog search UI integration

- [x] 2.1 Add/enable search bar in the application header wired to the shared search query contract and verify search is triggered only on explicit submit (Enter/search action).
- [x] 2.2 Add/enable search bar in the catalog view wired to the same query contract and verify submit-only behavior matches the header interaction pattern.
- [x] 2.3 Implement no-results UI state for unmatched queries in catalog/search results and verify the empty-result message is shown when API returns no matches.

## 3. Validation and regression coverage

- [x] 3.1 Add or update integration/API tests for submit-driven global search behavior (including no request sent until submit) and verify all new tests pass.
- [x] 3.2 Run relevant test suites for recipes search and catalog flows and verify no regressions in existing recipe listing behavior.
- [x] 3.3 Run the full affected test project(s) and verify global search changes are stable across backend and frontend boundaries.