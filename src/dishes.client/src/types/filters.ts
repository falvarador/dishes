/**
 * FilterState - Representa el estado de los filtros activos
 */
export interface FilterState {
  categories: string[];
  difficulty: string | null;
  prepTimeRange: [number, number]; // [min, max] in minutes
}

/**
 * RecipeFiltersParams - Parámetros para enviar a la API
 */
export interface RecipeFiltersParams {
  categories?: string;
  difficulty?: string;
  prepTimeMin?: number;
  prepTimeMax?: number;
  search?: string;
  status?: number;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}

/**
 * Categorías de receta hardcodeadas (MVP)
 */
export const RECIPE_CATEGORIES = [
  'Main Course',
  'Appetizers',
  'Desserts',
  'Breakfast',
  'Salads',
  'Soups',
  'Beverages',
  'Snacks',
];

/**
 * Dificultades disponibles
 */
export const RECIPE_DIFFICULTIES = ['Easy', 'Medium', 'Hard'];

/**
 * Opciones de tiempo de preparación (en minutos)
 */
export const PREP_TIME_OPTIONS = [
  { label: '15m', value: 15 },
  { label: '30m', value: 30 },
  { label: '1h', value: 60 },
  { label: '1.5h', value: 90 },
  { label: '2h+', value: 120 },
];
