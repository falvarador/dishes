## Context

The application currently uses a vertical-slice / feature-folder architecture with ASP.NET Core minimal APIs and EF Core with SQLite. The existing Ingredients and Dishes features follow this established pattern with handlers, DTOs, and integration-style testing. The data model includes entities like `Dish` and `Ingredient` with relationships managed through EF Core.

## Goals / Non-Goals

**Goals:**
- Implement recipe creation with full metadata (title, description, prep time, difficulty, categories, tags, ingredients, instructions)
- Implement recipe detail view with all information displayed clearly
- Follow existing architectural patterns (vertical slices, handlers, DTOs)
- Provide comprehensive unit/integration tests with good code coverage
- Support image upload and storage for recipe cover photos
- Ensure data integrity with validation

**Non-Goals:**
- User authentication/authorization (handled by security module)
- Advanced features like ratings, reviews, or comments (separate module)
- Complex search/filtering (can be added later)
- Frontend framework (backend API only for this module)
- Recipe editing/deletion (initial implementation focuses on creation and viewing)

## Decisions

### Recipe Data Model
**Decision:** Create `Recipe` entity with related tables for ingredients, instructions, categories, and tags.
**Rationale:** Normalized schema allows flexibility in managing ingredients and instructions independently, supports many-to-many relationships for categories and tags, and maintains data integrity.
**Alternatives Considered:**
- Store ingredients and instructions as JSON in Recipe entity → simpler initially but harder to query/modify
- Single flat table with all data → violates normalization, difficult to scale

### Image Storage
**Decision:** Store recipe cover photos in file system (or cloud storage blob like Azure Blob/S3) with path reference in Recipe entity.
**Rationale:** Separates large binary data from database, allows CDN integration, reduces database size.
**Alternatives Considered:**
- Store images as BLOB in database → simpler initially but impacts performance and backup size
- External service like Cloudinary → adds dependency and cost

### API Endpoints
**Decision:** Follow existing REST minimal API pattern with endpoints:
- `POST /api/recipes` - Create recipe
- `GET /api/recipes/{id}` - Get recipe detail
- `PUT /api/recipes/{id}` - Update recipe
- `DELETE /api/recipes/{id}` - Delete recipe
**Rationale:** Consistent with existing Dishes/Ingredients endpoints, predictable for clients.

### Validation
**Decision:** Use DataAnnotations for RecipeRequest DTO validation, matching existing pattern used for DishRequest and IngredientRequest.
**Rationale:** Simple, built-in, no additional dependencies, consistent with current codebase.

### Ingredient/Instruction Linking
**Decision:** Store ingredients and instructions as separate entities with foreign keys to Recipe (one-to-many relationships).
**Rationale:** Allows independent management of ingredients/instructions, easier to update or delete single items, maintains referential integrity.

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| Image uploads may consume significant storage | Implement file size limits (e.g., max 5MB per image), encourage format optimization, consider cloud storage with lifecycle policies |
| Recipe entity complexity | Keep entity focused, use value objects for enums (Difficulty, Category), proper EF Core configuration |
| Category and tag management | Start with hardcoded enums for categories; tags as free text; can add tag management UI later if needed |
| Concurrent edits and data loss | Implement optimistic concurrency checking with EF Core RowVersion or timestamp field |
| Performance with large ingredient lists | Index Recipe.Id on Ingredient and Instruction tables, paginate ingredient display if needed |
| N+1 query problem | Use `.Include()` to eager-load related ingredients/instructions when fetching recipes |

## Open Questions

- Should recipe editing/deletion be implemented in this module or deferred to a separate change? (Assuming creation-only for now, can add later)
- Should we implement soft-delete for recipes (mark as archived) or hard-delete? (Will use hard-delete initially, can extend later)
- What image storage backend should be used—local file system or cloud (Azure Blob/S3)? (Assuming local file system for MVP, cloud can be added later)
