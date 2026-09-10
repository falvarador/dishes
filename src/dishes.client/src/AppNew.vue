<script setup lang="ts">
import { computed, ref } from 'vue';
import { VueQueryDevtools } from '@tanstack/vue-query-devtools';
import { useRecipeFilters } from '@/composables/useRecipeFilters';
import RecipeFiltersPanel from '@/components/RecipeFilters/RecipeFiltersPanel.vue';
import FilterBadge from '@/components/RecipeFilters/FilterBadge.vue';
import type { FilterState } from '@/types/filters';
import type { RecipeSummaryResponse } from '@/client/models/index.js';

const {
  filters,
  recipes,
  totalCount,
  activeFilterCount,
  isLoading,
  isError,
  setFilters,
  clearFilters,
} = useRecipeFilters();

const catalogSearchInput = ref('');

function handleFiltersChange(newFilters: Partial<FilterState>) {
  setFilters(newFilters);
}

function getRecipeKey(recipe: RecipeSummaryResponse, index: number) {
  return recipe.id?.toString() ?? `${recipe.title ?? 'recipe'}-${index}`;
}

const showNoResults = computed(
  () => !isLoading.value && recipes.value.length === 0 && activeFilterCount.value > 0
);
</script>

<template>
  <div class="app-container">
    <header class="app-header">
      <h1>Dishes</h1>
    </header>

    <main class="catalog-page">
      <section class="catalog-toolbar">
        <h2>Recipe Catalog with Filters</h2>
      </section>

      <!-- Filter Badge -->
      <FilterBadge
        :active-filter-count="activeFilterCount"
        @clear="clearFilters"
      />

      <div class="content-layout">
        <!-- Filters Panel (Left) -->
        <RecipeFiltersPanel
          :is-loading="isLoading"
          :initial-filters="filters"
          @filters-change="handleFiltersChange"
        />

        <!-- Recipes List (Main) -->
        <section class="catalog-results">
          <p v-if="isLoading" class="status-message loading">Loading recipes...</p>
          <p v-else-if="isError" class="status-message error">
            Unable to load recipes right now.
          </p>
          <p v-else-if="showNoResults" class="status-message no-results">
            No recipes found matching your filters.
          </p>
          <ul v-else class="recipe-list">
            <li
              v-for="(recipe, index) in recipes"
              :key="getRecipeKey(recipe, index)"
              class="recipe-card"
            >
              <h3>{{ recipe.title }}</h3>
              <p><strong>Difficulty:</strong> {{ recipe.difficulty }}</p>
              <p v-if="recipe.rating" class="recipe-rating">
                ⭐ {{ recipe.rating }} ({{ recipe.ratingCount }} reviews)
              </p>
            </li>
          </ul>

          <p v-if="!isLoading && recipes.length > 0" class="results-info">
            Showing {{ recipes.length }} of {{ totalCount }} recipes
          </p>
        </section>
      </div>
    </main>

    <VueQueryDevtools />
  </div>
</template>

<style scoped>
.app-container {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.app-header {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  gap: 1rem;
  padding: 1.5rem;
  background-color: #f5f5f5;
  border-bottom: 1px solid #e0e0e0;
}

.app-header h1 {
  margin: 0;
  font-size: 1.8rem;
  color: #333;
}

.catalog-page {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 1.5rem;
  flex: 1;
}

.catalog-toolbar {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.catalog-toolbar h2 {
  margin: 0;
  font-size: 1.5rem;
  color: #333;
}

.content-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 2rem;
}

.catalog-results {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.status-message {
  padding: 1rem;
  border-radius: 4px;
  margin: 0;
  font-weight: 500;
}

.status-message.loading {
  background-color: #e3f2fd;
  color: #1976d2;
}

.status-message.error {
  background-color: #ffebee;
  color: #c62828;
}

.status-message.no-results {
  background-color: #fff3e0;
  color: #e65100;
}

.recipe-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.75rem;
}

.recipe-card {
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  padding: 1rem;
  background-color: white;
  transition: all 0.2s ease;
}

.recipe-card:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border-color: #bbb;
}

.recipe-card h3 {
  margin: 0 0 0.5rem 0;
  font-size: 1.1rem;
  color: #333;
}

.recipe-card p {
  margin: 0.25rem 0;
  color: #666;
  font-size: 0.95rem;
}

.recipe-rating {
  color: #f59e0b;
  font-weight: 600;
}

.results-info {
  text-align: center;
  color: #999;
  font-size: 0.85rem;
  margin-top: 1rem;
}

/* Mobile responsive */
@media (max-width: 768px) {
  .content-layout {
    grid-template-columns: 1fr;
  }

  .catalog-page {
    padding: 1rem;
  }
}
</style>
