# Recipes Specification

## Purpose

Enables recipe creators to create and publish recipes with complete metadata including ingredients, preparation instructions, difficulty level, and other important details. Provides a detailed view of published recipes, displaying all recipe information to users.

## Requirements

### Requirement: User can create a recipe
The system SHALL allow authenticated recipe creators to create a new recipe with all required metadata fields.

#### Scenario: Creator submits valid recipe
- **WHEN** creator clicks "Create Recipe" and fills all required fields (title, description, difficulty, categories, cover photo) and clicks "Publish"
- **THEN** system validates all inputs, stores the recipe, and displays success confirmation with recipe ID

#### Scenario: Creator omits required field
- **WHEN** creator submits recipe missing a required field (e.g., title)
- **THEN** system displays validation error for the missing field and prevents submission

#### Scenario: Cover photo upload
- **WHEN** creator uploads a cover photo for the recipe
- **THEN** system validates image format (JPG, PNG), stores it, and displays preview

### Requirement: User can add ingredients to recipe
The system SHALL allow creators to add a list of ingredients with quantities and units to their recipes.

#### Scenario: Add single ingredient
- **WHEN** creator enters ingredient name, quantity, and unit (e.g., "2 cups flour") and clicks "Add Ingredient"
- **THEN** system adds ingredient to the recipe's ingredient list

#### Scenario: Modify ingredient
- **WHEN** creator clicks edit on an existing ingredient and changes the quantity
- **THEN** system updates the ingredient with new information

#### Scenario: Delete ingredient
- **WHEN** creator clicks delete on an ingredient
- **THEN** system removes the ingredient from the recipe

### Requirement: User can add preparation instructions
The system SHALL allow creators to add step-by-step preparation instructions with proper ordering.

#### Scenario: Add instruction step
- **WHEN** creator clicks "Add Step" and enters instruction text
- **THEN** system adds step with auto-incrementing number

#### Scenario: Reorder instructions
- **WHEN** creator drags instruction step to new position
- **THEN** system reorders steps and updates numbering

### Requirement: Recipe contains difficulty and category metadata
The system SHALL allow creators to assign difficulty level and categories to recipes.

#### Scenario: Set difficulty level
- **WHEN** creator selects difficulty from options (Easy, Medium, Hard, Advanced)
- **THEN** system stores selected difficulty with recipe

#### Scenario: Assign categories
- **WHEN** creator selects one or more categories (Main Course, Appetizer, Dessert, Breakfast, Seafood, Baking, etc.)
- **THEN** system stores all selected categories with recipe

### Requirement: Recipe contains preparation time
The system SHALL allow creators to specify preparation and cooking time for recipes.

#### Scenario: Set preparation time
- **WHEN** creator enters time (e.g., "35m", "2 hrs", "24 hrs") in prep time field
- **THEN** system stores time value as part of recipe metadata

### Requirement: Recipe supports descriptive tags
The system SHALL allow creators to add tags to recipes for additional discoverability.

#### Scenario: Add tags
- **WHEN** creator enters tags (e.g., "Dinner", "Essentials", "Quick") separated by commas
- **THEN** system stores tags with recipe for search and filtering

### Requirement: User can publish recipe
The system SHALL allow creators to publish their recipes to make them visible to other users.

#### Scenario: Publish recipe
- **WHEN** creator clicks "Publish" after completing all recipe fields
- **THEN** system marks recipe as published and it becomes visible to all users

#### Scenario: Save as draft
- **WHEN** creator clicks "Save as Draft" without completing all fields
- **THEN** system saves recipe as draft (unpublished) accessible only to creator

### Requirement: Recipe creation maintains data integrity
The system SHALL validate all recipe data and ensure complete information before storage.

#### Scenario: Prevent duplicate ingredients
- **WHEN** creator attempts to add the same ingredient twice
- **THEN** system either rejects duplicate or asks creator to update quantity of existing ingredient

#### Scenario: Require non-empty fields
- **WHEN** creator attempts to submit recipe with empty title or description
- **THEN** system displays validation error and prevents submission

### Requirement: User can view recipe details
The system SHALL display complete recipe information on a recipe detail page.

#### Scenario: View recipe details
- **WHEN** user clicks on a recipe from list or search results
- **THEN** system displays recipe title, description, cover photo, prep time, difficulty, categories, tags, all ingredients, and all preparation steps

#### Scenario: Recipe attribution
- **WHEN** user views recipe details
- **THEN** system displays creator name and creation date

### Requirement: Recipe detail displays ingredients clearly
The system SHALL display all ingredients with their quantities and units in an organized format.

#### Scenario: View ingredient list
- **WHEN** user views recipe details
- **THEN** system displays complete ingredient list with quantities and units in a readable format

#### Scenario: Ingredient scaling
- **WHEN** user selects a serving multiplier (e.g., "× 2")
- **THEN** system recalculates and displays adjusted ingredient quantities

### Requirement: Recipe detail displays instructions step-by-step
The system SHALL display preparation instructions in numbered order for easy following.

#### Scenario: View instructions
- **WHEN** user views recipe details
- **WHEN** user scrolls to instructions section
- **THEN** system displays numbered steps in correct order with clear formatting

### Requirement: Recipe detail displays all metadata
The system SHALL display all recipe metadata clearly on the detail page.

#### Scenario: View recipe metadata
- **WHEN** user views recipe details
- **THEN** system displays prep time, difficulty level, and all associated categories and tags

### Requirement: Creator can update a published recipe
The system SHALL allow an authenticated creator to edit and update a recipe that they originally created and published.

#### Scenario: Creator updates own published recipe
- **WHEN** the authenticated creator submits valid updates for a published recipe they own
- **THEN** the system persists the updated recipe data and returns a successful result

#### Scenario: Creator submits invalid update payload
- **WHEN** the authenticated creator submits an update with missing or invalid required values
- **THEN** the system rejects the update and returns validation errors without modifying persisted data

### Requirement: Creator can delete a published recipe
The system SHALL allow an authenticated creator to delete a published recipe that they originally created.

#### Scenario: Creator deletes own published recipe
- **WHEN** the authenticated creator requests deletion of a published recipe they own
- **THEN** the system removes the recipe and returns a successful result

### Requirement: Published recipe management enforces ownership
The system SHALL prevent users from updating or deleting published recipes created by another user.

#### Scenario: Non-owner attempts to update published recipe
- **WHEN** an authenticated user who is not the recipe creator requests an update on that published recipe
- **THEN** the system denies the operation and returns a forbidden authorization result

#### Scenario: Non-owner attempts to delete published recipe
- **WHEN** an authenticated user who is not the recipe creator requests deletion of that published recipe
- **THEN** the system denies the operation and returns a forbidden authorization result

### Requirement: User can search recipes globally by title or ingredients
The system SHALL provide a global recipe search capability available from the header and the recipe catalog, matching recipes by title or ingredient names.

#### Scenario: Search by recipe title from header
- **WHEN** a user enters text in the header search bar that matches a recipe title
- **THEN** the system returns recipes whose titles contain the search text

#### Scenario: Search by ingredient from catalog
- **WHEN** a user enters text in the catalog search bar that matches any ingredient name used in a recipe
- **THEN** the system returns recipes that include matching ingredients

#### Scenario: Search returns combined matches
- **WHEN** a user enters text that matches both recipe titles and ingredient names
- **THEN** the system returns the union of matching recipes without duplicates

#### Scenario: Empty search query in catalog
- **WHEN** a user clears the search input in the catalog search bar
- **THEN** the system shows the default unfiltered recipe catalog

#### Scenario: No matching recipes
- **WHEN** a user submits a search query with no title or ingredient matches
- **THEN** the system returns an empty result set and the UI indicates no recipes were found

### Requirement: User can perform actions on recipe detail page
The system SHALL provide actionable options for users viewing recipes.

#### Scenario: Save recipe to favorites
- **WHEN** authenticated user clicks "Save to Favorites" button
- **THEN** system saves recipe to user's favorites list

#### Scenario: Share recipe
- **WHEN** user clicks "Share" button
- **THEN** system provides options to share via link, social media, or copy URL

#### Scenario: Print recipe
- **WHEN** user clicks "Print Recipe" button
- **THEN** system opens print-friendly version with optimized layout

### Requirement: Recipe detail page is responsive
The system SHALL display recipe details properly on all device sizes.

#### Scenario: View on mobile
- **WHEN** user accesses recipe on mobile device
- **THEN** system displays ingredients and instructions in readable format optimized for mobile
