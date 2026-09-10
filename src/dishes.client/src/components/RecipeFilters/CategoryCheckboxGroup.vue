<script setup lang="ts">
import { computed } from 'vue';
import { RECIPE_CATEGORIES } from '@/types/filters';

interface Props {
  selected: string[];
  categories?: string[];
}

interface Emits {
  (e: 'update:selected', value: string[]): void;
}

defineProps<Props>();
defineEmits<Emits>();

const categories = computed(() => {
  return (props: Props) => props.categories || RECIPE_CATEGORIES;
});

const handleToggle = (category: string, emit: any, selected: string[]) => {
  if (selected.includes(category)) {
    emit('update:selected', selected.filter(c => c !== category));
  } else {
    emit('update:selected', [...selected, category]);
  }
};
</script>

<template>
  <fieldset class="category-checkbox-group">
    <legend class="group-title">By Category</legend>

    <div class="checkbox-container">
      <div
        v-for="category in (categories || RECIPE_CATEGORIES)"
        :key="category"
        class="checkbox-item"
      >
        <input
          :id="`category-${category}`"
          type="checkbox"
          :checked="selected.includes(category)"
          @change="
            (e: any) => {
              const newSelected = e.target.checked
                ? [...selected, category]
                : selected.filter(c => c !== category);
              $emit('update:selected', newSelected);
            }
          "
        />
        <label :for="`category-${category}`">
          {{ category }}
        </label>
      </div>
    </div>
  </fieldset>
</template>

<style scoped>
.category-checkbox-group {
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

.checkbox-container {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.checkbox-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.checkbox-item input[type='checkbox'] {
  cursor: pointer;
  width: 18px;
  height: 18px;
}

.checkbox-item label {
  cursor: pointer;
  user-select: none;
  font-size: 0.9rem;
}

.checkbox-item label:hover {
  text-decoration: underline;
}

@media (max-width: 640px) {
  .checkbox-container {
    display: grid;
    grid-template-columns: 1fr 1fr;
  }
}
</style>
