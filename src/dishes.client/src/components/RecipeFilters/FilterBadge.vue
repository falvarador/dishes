<script setup lang="ts">
interface Props {
  activeFilterCount: number;
}

interface Emits {
  (e: 'clear'): void;
}

defineProps<Props>();
defineEmits<Emits>();
</script>

<template>
  <div v-if="activeFilterCount > 0" class="filter-badge-container">
    <span class="filter-badge">
      <strong>{{ activeFilterCount }}</strong>
      {{ activeFilterCount === 1 ? 'filter' : 'filters' }} active
    </span>
    <button
      type="button"
      class="btn-clear-badge"
      @click="$emit('clear')"
      aria-label="Clear all filters"
    >
      ✕
    </button>
  </div>
</template>

<style scoped>
.filter-badge-container {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem 1rem;
  background-color: #e3f2fd;
  border: 1px solid #90caf9;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.filter-badge {
  font-size: 0.9rem;
  color: #1976d2;
  font-weight: 500;
}

.btn-clear-badge {
  background-color: transparent;
  border: none;
  color: #1976d2;
  cursor: pointer;
  font-size: 1.1rem;
  font-weight: bold;
  padding: 0;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: background-color 0.2s ease;
  margin-left: auto;
}

.btn-clear-badge:hover {
  background-color: #b3e5fc;
}

@media (max-width: 640px) {
  .filter-badge-container {
    padding: 0.4rem 0.75rem;
  }

  .filter-badge {
    font-size: 0.85rem;
  }
}
</style>
