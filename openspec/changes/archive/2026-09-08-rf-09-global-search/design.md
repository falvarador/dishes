## Context

The application already supports recipe CRUD and catalog listing, and the recipes capability spec exists at `openspec/specs/recipes/spec.md`. This change modifies that capability to include global search behavior from two entry points (header and catalog) while keeping existing list and detail workflows intact. See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**
- Add a unified search behavior that filters recipes by title and ingredient names.
- Reuse a single backend filtering contract so header and catalog produce consistent results.
- Preserve existing catalog behavior when search input is empty.
- Add automated tests for positive matches, union results, and no-result cases.

**Non-Goals:**
- Introducing full-text indexing engines or external search services.
- Ranking/relevance tuning beyond deterministic filtering.
- Changing recipe authoring/publishing workflows.
- Implementing advanced filters (difficulty, category, tags) as part of this change.

## Decisions

1. **Single query contract for both header and catalog search**
   - Decision: Use one recipe search query parameter and backend filtering pipeline for both UI entry points.
   - Rationale: Prevents divergent behavior between header and catalog.
   - Alternatives considered:
	 - Separate endpoint for header search: rejected due to duplicated logic and inconsistent maintenance.
	 - Frontend-only filtering: rejected because ingredient matching requires server-side relational filtering.

2. **Case-insensitive partial match on title and ingredient name**
   - Decision: Match recipes where title contains query text OR any ingredient name contains query text, case-insensitive.
   - Rationale: Aligns with user expectation for forgiving global search.
   - Alternatives considered:
	 - Exact match only: rejected as too restrictive.
	 - Prefix-only match: rejected because it misses common mid-string searches.

3. **Union semantics with deduplication**
   - Decision: Return each matching recipe once even when it matches both title and ingredient criteria.
   - Rationale: Avoids confusing duplicate entries in the catalog.
   - Alternatives considered:
	 - Separate grouped sections (title matches vs ingredient matches): rejected because it complicates catalog UX.

4. **Empty query returns default unfiltered catalog**
   - Decision: Treat empty/whitespace query as no filter.
   - Rationale: Keeps catalog browsing unchanged when users clear search.
   - Alternatives considered:
	 - Empty query returning no results: rejected as counterintuitive.

## Risks / Trade-offs

- **[Risk]** Ingredient matching may increase query complexity and impact performance on large datasets.  
  **Mitigation:** Add query projection discipline and regression tests; profile query execution if dataset grows.

- **[Risk]** Inconsistent debounce/trigger behavior between header and catalog can create UX mismatch.  
  **Mitigation:** Define a shared frontend interaction pattern during implementation (same trigger event and minimum input rules).

- **[Trade-off]** Basic contains-search is simple but less precise than ranked search.  
  **Mitigation:** Keep current scope minimal and leave relevance ranking for a future change.

## Migration Plan

1. Update backend recipe list/search behavior to support title-or-ingredient filtering.
2. Wire header and catalog search inputs to the shared query contract.
3. Add/update tests for title match, ingredient match, deduped union, empty query, and no-result behavior.
4. Run full API and UI test suites relevant to recipes and catalog search.
5. Rollback path: revert search filter integration while preserving existing catalog listing endpoint behavior.

## Open Questions

- Should search trigger on every keystroke or on explicit submit, and should both entry points behave identically by default?