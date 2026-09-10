<script setup lang="ts">
import { RECIPE_DIFFICULTIES } from '@/types/filters';

interface Props {
  selected: string | null;
}

interface Emits {
  (e: 'update:selected', value: string | null): void;
}

defineProps<Props>();
defineEmits<Emits>();
</script>

<template>
  <fieldset class="difficulty-chip-group">
    <legend class="group-title">By Difficulty</legend>

    <div class="chip-container">
      <!-- None/Clear option -->
      <button
        type="button"
        class="chip"
        :class="{ 'chip--active': selected === null }"
        @click="$emit('update:selected', null)"
      >
        None
      </button>

      <!-- Difficulty options -->
      <button
        v-for="difficulty in RECIPE_DIFFICULTIES"
        :key="difficulty"
        type="button"
        class="chip"
        :class="{ 'chip--active': selected === difficulty }"
        @click="$emit('update:selected', difficulty)"
      >
        {{ difficulty }}
      </button>
    </div>
  </fieldset>
</template>

<style scoped>
.difficulty-chip-group {
  border: none;
  padding: 0;
  margin: 0 0 1.5rem 0;
}

.group-title {
  font-weight: 600;
  font-size: 0.95rem;
  margin-bottom: 0.75rem;
  display: block;
  color: #333;
}

.chip-container {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.chip {
  padding: 0.5rem 1rem;
  border: 2px solid #d3d3d3;
  border-radius: 20px;
  background-color: #fff;
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: 500;
  transition: all 0.2s ease;
  user-select: none;
}

.chip:hover {
  border-color: #999;
  background-color: #f5f5f5;
}

.chip--active {
  background-color: #007bff;
  color: white;
  border-color: #007bff;
}

.chip--active:hover {
  background-color: #0056b3;
  border-color: #0056b3;
}

@media (max-width: 640px) {
  .chip {
    padding: 0.4rem 0.8rem;
    font-size: 0.85rem;
  }
}
</style>
