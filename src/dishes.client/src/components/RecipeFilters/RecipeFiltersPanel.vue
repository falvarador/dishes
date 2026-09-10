<script setup lang="ts">
import { computed, ref, watch, withDefaults, defineProps, defineEmits } from 'vue';
import type { FilterState } from '@/types/filters';
import CategoryCheckboxGroup from './CategoryCheckboxGroup.vue';
import DifficultyChipGroup from './DifficultyChipGroup.vue';
import PrepTimeSlider from './PrepTimeSlider.vue';

interface Props {
  isLoading?: boolean;
  initialFilters?: FilterState;
}

interface Emits {
  (e: 'filters-change', filters: Partial<FilterState>): void;
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false,
});

const emit = defineEmits<Emits>();

// Estado local de filtros
const selectedCategories = ref<string[]>(
  props.initialFilters?.categories ?? []
);
const selectedDifficulty = ref<string | null>(
  props.initialFilters?.difficulty ?? null
);
const prepTimeRange = ref<[number, number]>(
  props.initialFilters?.prepTimeRange ?? [15, 120]
);

/**
 * Actualizar estado local cuando cambian props iniciales
 */
watch(
  () => props.initialFilters,
  (newFilters) => {
    if (newFilters) {
      selectedCategories.value = newFilters.categories;
      selectedDifficulty.value = newFilters.difficulty;
      prepTimeRange.value = newFilters.prepTimeRange;
    }
  },
  { deep: true }
);

/**
 * Emitir cambios cuando cambian los filtros locales
 */
watch(
  [selectedCategories, selectedDifficulty, prepTimeRange],
  () => {
    emit('filters-change', {
      categories: selectedCategories.value,
      difficulty: selectedDifficulty.value,
      prepTimeRange: prepTimeRange.value,
    });
  },
  { deep: true }
);

/**
 * Contar filtros activos
 */
const activeFilterCount = computed(() => {
  let count = 0;
  if (selectedCategories.value.length > 0) count += selectedCategories.value.length;
  if (selectedDifficulty.value) count += 1;
  if (prepTimeRange.value[0] !== 15 || prepTimeRange.value[1] !== 120) count += 1;
  return count;
});

/**
 * Limpiar todos los filtros
 */
function clearFilters() {
  selectedCategories.value = [];
  selectedDifficulty.value = null;
  prepTimeRange.value = [15, 120];
  emit('filters-change', {
    categories: [],
    difficulty: null,
    prepTimeRange: [15, 120],
  });
}
</script>

<template>
  <aside class="recipe-filters-panel">
    <div class="panel-header">
      <h2>Filters</h2>
      <span v-if="activeFilterCount > 0" class="filter-badge">
        {{ activeFilterCount }}
      </span>
    </div>

    <div v-if="isLoading" class="loading-spinner">
      <p>Applying filters...</p>
    </div>

    <div class="filters-content">
      <!-- Categories Filter -->
      <CategoryCheckboxGroup
        :selected="selectedCategories"
        @update:selected="selectedCategories = $event"
      />

      <!-- Difficulty Filter -->
      <DifficultyChipGroup
        :selected="selectedDifficulty"
        @update:selected="selectedDifficulty = $event"
      />

      <!-- Prep Time Filter -->
      <PrepTimeSlider
        :range="prepTimeRange"
        @update:range="prepTimeRange = $event"
      />
    </div>

    <!-- Clear Filters Button -->
    <button
      type="button"
      class="btn-clear-filters"
      :disabled="activeFilterCount === 0"
      @click="clearFilters"
    >
      Clear Filters
    </button>
  </aside>
</template>

<style scoped>
.recipe-filters-panel {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 1.5rem;
  background-color: #f9f9f9;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
  min-width: 250px;
  max-height: calc(100vh - 100px);
  overflow-y: auto;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.5rem;
}

.panel-header h2 {
  margin: 0;
  font-size: 1.25rem;
  color: #333;
}

.filter-badge {
  background-color: #007bff;
  color: white;
  padding: 0.25rem 0.6rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
  min-width: 24px;
  text-align: center;
}

.loading-spinner {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
  color: #666;
  font-size: 0.9rem;
}

.filters-content {
  display: flex;
  flex-direction: column;
}

.btn-clear-filters {
  padding: 0.75rem 1.25rem;
  background-color: #e74c3c;
  color: white;
  border: none;
  border-radius: 4px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s ease;
  margin-top: 1rem;
}

.btn-clear-filters:hover:not(:disabled) {
  background-color: #c0392b;
}

.btn-clear-filters:disabled {
  background-color: #d3d3d3;
  cursor: not-allowed;
  opacity: 0.6;
}

@media (max-width: 768px) {
  .recipe-filters-panel {
    min-width: auto;
    max-height: auto;
    margin-bottom: 1.5rem;
  }

  .panel-header {
    flex-direction: row;
  }
}
</style>
