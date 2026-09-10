import { computed, ref, watch } from 'vue';
import { useSearchParams } from 'vue-router';
import { useQuery } from '@tanstack/vue-query';
import type { FilterState, RecipeFiltersParams } from '@/types/filters';
import { createDishesClient } from '@/client/dishesClient';
import { AnonymousAuthenticationProvider } from '@microsoft/kiota-abstractions';
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';
import type { RecipeSummaryResponse } from '@/client/models/index';

/**
 * Hook composable para manejar filtros de recetas
 * Sincroniza estado local <-> URL params <-> API calls
 */
export function useRecipeFilters() {
  // Inicializar cliente Kiota
  const authProvider = new AnonymousAuthenticationProvider();
  const adapter = new FetchRequestAdapter(authProvider);
  adapter.baseUrl = window.location.origin;
  const client = createDishesClient(adapter);

  // Estado local de filtros
  const filters = ref<FilterState>({
    categories: [],
    difficulty: null,
    prepTimeRange: [15, 120],
  });

  // Leer query params de la URL
  const searchParams = useSearchParams();

  /**
   * Parsear query params de URL y actualizar estado local
   */
  function parseFiltersFromURL(): FilterState {
    const categories = searchParams.get('categories')
      ? searchParams.get('categories')!.split(',').filter(c => c.trim())
      : [];

    const difficulty = searchParams.get('difficulty') || null;

    const prepTimeMin = parseInt(searchParams.get('prepTimeMin') || '15', 10);
    const prepTimeMax = parseInt(searchParams.get('prepTimeMax') || '120', 10);

    return {
      categories,
      difficulty,
      prepTimeRange: [prepTimeMin, prepTimeMax],
    };
  }

  /**
   * Inicializar filtros desde URL
   */
  filters.value = parseFiltersFromURL();

  /**
   * Generar query params a partir del estado de filtros
   */
  function generateQueryParams(): RecipeFiltersParams {
    const params: RecipeFiltersParams = {
      page: 1,
      pageSize: 20,
      status: 1, // Solo recetas publicadas
    };

    if (filters.value.categories.length > 0) {
      params.categories = filters.value.categories.join(',');
    }

    if (filters.value.difficulty) {
      params.difficulty = filters.value.difficulty;
    }

    if (filters.value.prepTimeRange[0] > 0 || filters.value.prepTimeRange[1] < 120) {
      params.prepTimeMin = filters.value.prepTimeRange[0];
      params.prepTimeMax = filters.value.prepTimeRange[1];
    }

    return params;
  }

  /**
   * Actualizar URL con nuevos query params
   */
  function updateURL() {
    const newParams = new URLSearchParams();

    if (filters.value.categories.length > 0) {
      newParams.set('categories', filters.value.categories.join(','));
    }

    if (filters.value.difficulty) {
      newParams.set('difficulty', filters.value.difficulty);
    }

    if (filters.value.prepTimeRange[0] !== 15 || filters.value.prepTimeRange[1] !== 120) {
      newParams.set('prepTimeMin', filters.value.prepTimeRange[0].toString());
      newParams.set('prepTimeMax', filters.value.prepTimeRange[1].toString());
    }

    // Actualizar URL sin recargar página
    const newUrl = newParams.toString()
      ? `${window.location.pathname}?${newParams.toString()}`
      : window.location.pathname;
    window.history.replaceState({}, '', newUrl);
  }

  /**
   * Fetch de recetas con filtros actuales
   */
  const fetchRecipes = async () => {
    const queryParameters = generateQueryParams();
    console.debug('[useRecipeFilters] Fetching with params:', queryParameters);

    try {
      const response = await client.api.recipes.get({ queryParameters: queryParameters as any });
      return response;
    } catch (error) {
      console.error('[useRecipeFilters] API Error:', error);
      throw error;
    }
  };

  /**
   * Query hook para obtener recetas
   */
  const { isFetching, isError, data } = useQuery({
    queryKey: ['recipes-list', filters],
    queryFn: fetchRecipes,
    staleTime: 5 * 60 * 1000, // 5 minutos
  });

  /**
   * Lista de recetas
   */
  const recipes = computed(() => data.value?.data ?? []);

  /**
   * Total de recetas
   */
  const totalCount = computed(() => data.value?.totalCount ?? 0);

  /**
   * Contar filtros activos
   */
  const activeFilterCount = computed(() => {
    let count = 0;
    if (filters.value.categories.length > 0) count += filters.value.categories.length;
    if (filters.value.difficulty) count += 1;
    if (filters.value.prepTimeRange[0] !== 15 || filters.value.prepTimeRange[1] !== 120) count += 1;
    return count;
  });

  /**
   * Actualizar filtros y sincronizar URL
   */
  function setFilters(newFilters: Partial<FilterState>) {
    filters.value = {
      ...filters.value,
      ...newFilters,
    };
    updateURL();
  }

  /**
   * Limpiar todos los filtros
   */
  function clearFilters() {
    filters.value = {
      categories: [],
      difficulty: null,
      prepTimeRange: [15, 120],
    };
    // Limpiar URL
    window.history.replaceState({}, '', window.location.pathname);
  }

  /**
   * Watch: si URL cambia externamente (ej: back/forward), actualizar estado local
   */
  watch(
    () => window.location.search,
    () => {
      filters.value = parseFiltersFromURL();
    }
  );

  return {
    filters: computed(() => filters.value),
    recipes,
    totalCount,
    activeFilterCount,
    isLoading: computed(() => isFetching.value),
    isError,
    setFilters,
    clearFilters,
  };
}
