<script setup lang="ts">
import { PREP_TIME_OPTIONS } from '@/types/filters';

interface Props {
  range: [number, number]; // [min, max] in minutes
}

interface Emits {
  (e: 'update:range', value: [number, number]): void;
}

defineProps<Props>();
defineEmits<Emits>();

/**
 * Encontrar el label para un valor de tiempo de preparación
 */
function getTimeLabel(value: number): string {
  const option = PREP_TIME_OPTIONS.find(o => o.value === value);
  return option?.label || `${Math.round(value / 60)}h`;
}

/**
 * Manejar cambio del slider mínimo
 */
function handleMinChange(e: Event) {
  const newMin = parseInt((e.target as HTMLInputElement).value, 10);
  if (newMin <= props.range[1]) {
    emit('update:range', [newMin, props.range[1]]);
  }
}

/**
 * Manejar cambio del slider máximo
 */
function handleMaxChange(e: Event) {
  const newMax = parseInt((e.target as HTMLInputElement).value, 10);
  if (newMax >= props.range[0]) {
    emit('update:range', [props.range[0], newMax]);
  }
}
</script>

<template>
  <fieldset class="prep-time-slider-group">
    <legend class="group-title">By Prep Time</legend>

    <div class="range-labels">
      <span class="range-min">{{ getTimeLabel(range[0]) }}</span>
      <span class="range-max">{{ getTimeLabel(range[1]) }}</span>
    </div>

    <div class="slider-container">
      <input
        type="range"
        class="slider slider-min"
        min="15"
        max="120"
        :value="range[0]"
        @input="handleMinChange"
      />
      <input
        type="range"
        class="slider slider-max"
        min="15"
        max="120"
        :value="range[1]"
        @input="handleMaxChange"
      />
    </div>

    <div class="track"></div>

    <div class="presets">
      <button
        v-for="option in PREP_TIME_OPTIONS"
        :key="option.value"
        type="button"
        class="preset-btn"
        :class="{ 'preset-btn--active': range[0] === option.value && range[1] === option.value }"
        @click="$emit('update:range', [option.value, option.value])"
      >
        {{ option.label }}
      </button>
    </div>
  </fieldset>
</template>

<style scoped>
.prep-time-slider-group {
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

.range-labels {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.5rem;
  font-size: 0.85rem;
  color: #666;
}

.slider-container {
  position: relative;
  height: 30px;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
}

.slider {
  position: absolute;
  width: 100%;
  height: 5px;
  border-radius: 5px;
  background: transparent;
  pointer-events: none;
  -webkit-appearance: none;
  appearance: none;
}

/* WebKit browsers (Chrome, Safari) */
.slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: #007bff;
  cursor: pointer;
  pointer-events: auto;
  border: 2px solid white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

/* Firefox */
.slider::-moz-range-thumb {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: #007bff;
  cursor: pointer;
  pointer-events: auto;
  border: 2px solid white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.slider-min {
  z-index: 5;
}

.slider-max {
  z-index: 4;
}

.track {
  position: absolute;
  width: 100%;
  height: 5px;
  border-radius: 5px;
  background: #e0e0e0;
  top: 50%;
  transform: translateY(-50%);
  z-index: 1;
  pointer-events: none;
}

.presets {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.preset-btn {
  padding: 0.4rem 0.8rem;
  border: 1px solid #d3d3d3;
  border-radius: 4px;
  background-color: #f9f9f9;
  cursor: pointer;
  font-size: 0.85rem;
  transition: all 0.2s ease;
}

.preset-btn:hover {
  border-color: #999;
  background-color: #f0f0f0;
}

.preset-btn--active {
  background-color: #007bff;
  color: white;
  border-color: #007bff;
}

.preset-btn--active:hover {
  background-color: #0056b3;
  border-color: #0056b3;
}

@media (max-width: 640px) {
  .preset-btn {
    padding: 0.3rem 0.6rem;
    font-size: 0.8rem;
  }
}
</style>
