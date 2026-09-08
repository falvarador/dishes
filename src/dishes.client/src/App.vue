<script setup lang="ts">
import { computed, ref } from 'vue'
import { VueQueryDevtools } from '@tanstack/vue-query-devtools'
import { useQuery } from '@tanstack/vue-query'
import { AnonymousAuthenticationProvider } from '@microsoft/kiota-abstractions'
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary'
import { createDishesClient } from '@/client/dishesClient.js'
import type { RecipeSummaryResponse } from '@/client/models/index.js'

const authProvider = new AnonymousAuthenticationProvider()
const adapter = new FetchRequestAdapter(authProvider)
adapter.baseUrl = window.location.origin
const client = createDishesClient(adapter)

const headerSearchInput = ref('')
const catalogSearchInput = ref('')
const activeSearchQuery = ref('')
const activeSearchSource = ref<'header' | 'catalog'>('catalog')

function applySearch(searchSource: 'header' | 'catalog', query: string) {
  activeSearchSource.value = searchSource
  activeSearchQuery.value = query.trim()
}

function submitHeaderSearch() {
  applySearch('header', headerSearchInput.value)
}

function submitCatalogSearch() {
  applySearch('catalog', catalogSearchInput.value)
}

const fetchRecipes = async () => {
  const queryParameters = activeSearchQuery.value
    ? { page: '1', pageSize: '20', search: activeSearchQuery.value }
    : { page: '1', pageSize: '20' }

  return await client.api.recipes.get({ queryParameters })
}

const { isFetching, isError, data } = useQuery({
  queryKey: ['recipes-catalog', activeSearchQuery],
  queryFn: fetchRecipes,
})

const recipes = computed(() => data.value?.data ?? [])

const showNoResults = computed(() =>
  activeSearchQuery.value.length > 0 && !isFetching.value && recipes.value.length === 0
)

const searchSourceLabel = computed(() =>
  activeSearchSource.value === 'header' ? 'header search' : 'catalog search'
)

function getRecipeKey(recipe: RecipeSummaryResponse, index: number) {
  return recipe.id?.toString() ?? `${recipe.title ?? 'recipe'}-${index}`
}
</script>

<template>
  <header class="app-header">
    <h1>Dishes</h1>

    <form class="search-form" @submit.prevent="submitHeaderSearch">
      <label for="header-search" class="search-label">Search recipes</label>
      <input
        id="header-search"
        v-model="headerSearchInput"
        type="search"
        placeholder="Search by title or ingredient"
      />
      <button type="submit">Search</button>
    </form>
  </header>

  <main class="catalog-page">
    <section class="catalog-toolbar">
      <h2>Recipe Catalog</h2>
      <form class="search-form" @submit.prevent="submitCatalogSearch">
        <label for="catalog-search" class="search-label">Find recipes in catalog</label>
        <input
          id="catalog-search"
          v-model="catalogSearchInput"
          type="search"
          placeholder="Search by title or ingredient"
        />
        <button type="submit">Search</button>
      </form>

      <p v-if="activeSearchQuery" class="search-summary">
        Showing results for "{{ activeSearchQuery }}" from {{ searchSourceLabel }}.
      </p>
    </section>

    <section class="catalog-results">
      <p v-if="isFetching">Loading recipes...</p>
      <p v-else-if="isError">Unable to load recipes right now.</p>
      <p v-else-if="showNoResults">No recipes found matching your search.</p>
      <ul v-else class="recipe-list">
        <li v-for="(recipe, index) in recipes" :key="getRecipeKey(recipe, index)" class="recipe-card">
          <h3>{{ recipe.title }}</h3>
          <p><strong>Difficulty:</strong> {{ recipe.difficulty }}</p>
        </li>
      </ul>
    </section>
  </main>

  <VueQueryDevtools />
</template>

<style scoped>
.app-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 1rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.catalog-page {
  display: grid;
  gap: 1.5rem;
}

.catalog-toolbar {
  display: grid;
  gap: 1rem;
}

.search-form {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  flex-wrap: wrap;
}

.search-label {
  min-width: 11rem;
  font-weight: 600;
}

input[type='search'] {
  min-width: 18rem;
  padding: 0.5rem;
}

button {
  padding: 0.5rem 0.9rem;
  cursor: pointer;
}

.search-summary {
  margin: 0;
}

.recipe-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.75rem;
}

.recipe-card {
  border: 1px solid #d3d3d3;
  border-radius: 0.5rem;
  padding: 0.75rem 1rem;
}

.recipe-card h3 {
  margin: 0 0 0.5rem;
}
</style>
