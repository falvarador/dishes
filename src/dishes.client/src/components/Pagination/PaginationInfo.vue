<template>
  <div v-if="totalCount > 0" class="pagination-info">
    <p class="text-sm text-gray-600 dark:text-gray-400">
      Showing <span class="font-semibold">{{ startIndex }}</span>–<span class="font-semibold">{{ endIndex }}</span> of
      <span class="font-semibold">{{ totalCount }}</span> recipes
    </p>
  </div>
  <div v-else class="pagination-info">
    <p class="text-sm text-gray-600 dark:text-gray-400">No recipes found</p>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  offset: number;
  limit: number;
  totalCount: number;
}

const props = withDefaults(defineProps<Props>(), {
  offset: 0,
  limit: 12,
  totalCount: 0,
});

// Calculate start and end indices (1-indexed to show to users)
const startIndex = computed(() => {
  if (props.totalCount === 0) return 0;
  return props.offset + 1;
});

const endIndex = computed(() => {
  const end = props.offset + props.limit;
  return Math.min(end, props.totalCount);
});
</script>

<style scoped>
.pagination-info {
  padding: 0.5rem 0;
  text-align: center;
}
</style>
