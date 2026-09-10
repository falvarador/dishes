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

  // Pagination state
  const limit = ref(12); // Default page size
  const offset = ref(0); // Default offset (start at 0)

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
   * Parse pagination parameters from URL
   */
  function parsePaginationFromURL(): { limit: number; offset: number } {
    const urlLimit = parseInt(searchParams.get('limit') || '12', 10);
    const urlOffset = parseInt(searchParams.get('offset') || '0', 10);
    return { limit: Math.max(1, Math.min(100, urlLimit)), offset: Math.max(0, urlOffset) };
  }

  /**
   * Inicializar filtros desde URL
   */
  filters.value = parseFiltersFromURL();
  const { limit: urlLimit, offset: urlOffset } = parsePaginationFromURL();
  limit.value = urlLimit;
  offset.value = urlOffset;

  /**
   * Generar query params a partir del estado de filtros
   */
  function generateQueryParams(): RecipeFiltersParams {
    const params: RecipeFiltersParams = {
      limit: limit.value,
      offset: offset.value,
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

    // Add pagination params
    if (offset.value > 0) {
      newParams.set('offset', offset.value.toString());
    }
    if (limit.value !== 12) {
      newParams.set('limit', limit.value.toString());
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
    queryKey: ['recipes-list', filters, limit, offset],
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
   * Pagination metadata
   */
  const pagination = computed(() => data.value?.pagination ?? { total: 0, limit: 12, offset: 0, hasMore: false });

  /**
   * Compute if the next page is available
   */
  const canGoNext = computed(() => pagination.value?.hasMore ?? false);

  /**
   * Compute if the previous page is available
   */
  const canGoPrevious = computed(() => offset.value > 0);

  /**
   * Current page number (1-indexed)
   */
  const currentPageNumber = computed(() => Math.floor(offset.value / limit.value) + 1);

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
   * Actualizar filtros y sincronizar URL (reset pagination to first page)
   */
  function setFilters(newFilters: Partial<FilterState>) {
    filters.value = {
      ...filters.value,
      ...newFilters,
    };
    // Reset pagination to first page when filters change
    offset.value = 0;
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
    offset.value = 0;
    // Limpiar URL
    window.history.replaceState({}, '', window.location.pathname);
  }

  /**
   * Go to next page
   */
  function nextPage() {
    if (canGoNext.value) {
      offset.value += limit.value;
      updateURL();
    }
  }

  /**
   * Go to previous page
   */
  function previousPage() {
    if (canGoPrevious.value) {
      offset.value = Math.max(0, offset.value - limit.value);
      updateURL();
    }
  }

  /**
   * Load more (append next page to existing recipes)
   */
  function loadMore() {
    if (canGoNext.value) {
      offset.value += limit.value;
      updateURL();
    }
  }

  /**
   * Watch: si URL cambia externamente (ej: back/forward), actualizar estado local
   */
  watch(
    () => window.location.search,
    () => {
      filters.value = parseFiltersFromURL();
      const { limit: urlLimit, offset: urlOffset } = parsePaginationFromURL();
      limit.value = urlLimit;
      offset.value = urlOffset;
    }
  );

  return {
    filters: computed(() => filters.value),
    recipes,
    totalCount,
    pagination,
    limit: computed(() => limit.value),
    offset: computed(() => offset.value),
    canGoNext,
    canGoPrevious,
    currentPageNumber,
    activeFilterCount,
    isLoading: computed(() => isFetching.value),
    isError,
    setFilters,
    clearFilters,
    nextPage,
    previousPage,
    loadMore,
  };
}
