# Tasks: Filtros Múltiples para Recetas (RF-10)

## Fase 1: Verificación de Dependencias Backend

### [x] Task 1.1: Auditar Modelo Recipe y Relaciones
- **Descripción**: Revisar si `Recipe` tiene relación N:M con `Category` (e.g., `RecipeCategory` tabla)
- **Archivo(s) a revisar**: 
  - `src/dishes.Server/Data/Entities/Recipe.cs`
  - `src/dishes.Server/Data/Entities/RecipeCategory.cs` (si existe)
  - `AppDbContext.cs`
- **Criterio de aceptación**: Documentar en comentario de tarea si categories están mapeadas y cómo
- **Estimación**: 0.5h

### [x] Task 1.2: Auditar GetRecipesListHandler Filtros Existentes
- **Descripción**: Verificar que endpoint `GET /api/recipes` ya soporta filtros por `difficulty` y `prepTime`
- **Archivo(s)**: `src/dishes.Server/Features/Recipes/GetRecipesList/GetRecipesListHandler.cs`
- **Criterio de aceptación**: 
  - Confirmar lógica de filtrado para `difficulty`
  - Confirmar lógica de filtrado para `prepTime`
  - Documentar gaps si los hay
- **Estimación**: 0.5h

### [x] Task 1.3: Extensión Backend (Condicional)
- **Descripción**: Si falta soporte de categorías en backend, agregar `category` filter a `GetRecipesListHandler`
- **Archivo(s)**: `GetRecipesListHandler.cs`
- **Criterio de aceptación**:
  - ✅ `[FromQuery] string? categories` parameter acepta múltiples valores (comma-separated)
  - ✅ Filtro aplicado con INNER JOIN (lógica de `.Any()`) a `RecipeCategories`
  - ✅ `.Distinct()` agregado para eliminar duplicados causados por relación N:M
  - ✅ Tests de búsqueda pasan: SearchesByTitle, SearchesByIngredientName, SearchesByTitleOrIngredient_WithoutDuplicates
- **Nota IMPORTANTE**: El `.Distinct()` es crítico para evitar duplicados cuando se incluyen relaciones N:M

---

## Fase 2: Componentes Frontend - Base

### [x] Task 2.1: Crear Hook useRecipeFilters
- **Descripción**: Custom hook que encapsula toda la lógica de URL ↔️ estado ↔️ API
- **Ubicación**: `src/dishes.client/src/hooks/useRecipeFilters.ts`
- **Responsabilidad**:
  - Leer `useSearchParams()` de React Router
  - Parsear y validar query params (`categories`, `difficulty`, `prepTimeMin`, `prepTimeMax`)
  - Manejar estado de carga
  - Llamar API `GET /api/recipes` con params
  - Retornar: `{ filters, recipes, isLoading, totalCount, setFilters, clearFilters }`
- **Criterio de aceptación**:
  - Hook compila sin errores
  - Manual test: cambiar URL params → recetas actualizadas
  - Log de params enviados a API (debug)
- **Estimación**: 2h
- **Dependencia**: Task 1.2 OK

### [x] Task 2.2: Crear Componente CategoryCheckboxGroup
- **Descripción**: Renderiza lista de checkboxes para categorías disponibles
- **Ubicación**: `src/dishes.client/src/components/RecipeFilters/CategoryCheckboxGroup.tsx`
- **Props**:
  - `selected: string[]`
  - `onChange: (selected: string[]) => void`
  - `categories?: string[]` (hardcoded por defecto)
- **Criterio de aceptación**:
  - Renderiza 5+ checkboxes
  - Toggle checkbox → onChange fired con array actualizado
  - Estilos responsive (mobile/desktop)
- **Estimación**: 1h
- **Dependencia**: Task 2.1

### [x] Task 2.3: Crear Componente DifficultyChipGroup
- **Descripción**: Renderiza chips/botones para Easy, Medium, Hard (radio-like behavior)
- **Ubicación**: `src/dishes.client/src/components/RecipeFilters/DifficultyChipGroup.tsx`
- **Props**:
  - `selected: string | null`
  - `onChange: (selected: string | null) => void`
- **Criterio de aceptación**:
  - 3 chips (Easy, Medium, Hard) + None/Clear option
  - Click chip → selecciona (radio behavior) y onChange fired
  - Visual feedback (highlighted / pressed state)
- **Estimación**: 1h
- **Dependencia**: Task 2.1

### [x] Task 2.4: Crear Componente PrepTimeSlider
- **Descripción**: Slider de rango para min-max tiempo preparación (15m a 2h+)
- **Ubicación**: `src/dishes.client/src/components/RecipeFilters/PrepTimeSlider.tsx`
- **Props**:
  - `range: [number, number]` (mins)
  - `onChange: (range: [number, number]) => void`
- **Criterio de aceptación**:
  - Slider de rango dual funcionando (min/max)
  - Labels (15m, 30m, 1h, 1.5h, 2h+)
  - onChange fired con nuevo rango en minutos
  - Responsive en mobile
- **Estimación**: 1.5h
- **Dependencia**: Task 2.1
- **Nota**: Considerar librerías como `react-range` o implementar custom

---

## Fase 3: Componente Panel de Filtros

### [x] Task 3.1: Crear Componente RecipeFiltersPanel
- **Descripción**: Panel contenedor que agrupa CategoryCheckboxGroup, DifficultyChipGroup, PrepTimeSlider
- **Ubicación**: `src/dishes.client/src/components/RecipeFilters/RecipeFiltersPanel.tsx`
- **Props**:
  - `onFiltersChange: (filters: FilterState) => void`
  - `isLoading?: boolean`
  - `initialFilters?: FilterState`
- **Estado Interno**:
  - `selectedCategories: string[]`
  - `selectedDifficulty: string | null`
  - `prepTimeRange: [number, number]`
- **Métodos**:
  - `handleCategoryChange()` → updateState + `onFiltersChange()`
  - `handleDifficultyChange()` → updateState + `onFiltersChange()`
  - `handlePrepTimeChange()` → updateState + `onFiltersChange()`
  - `handleClearFilters()` → resetState a defaults + `onFiltersChange({})`
- **Criterio de aceptación**:
  - Panel renderiza 3 sub-componentes
  - Cambios propagados correctamente
  - Botón "Clear Filters" limpia todo
  - Loading state visual si `isLoading`
  - Estilos responsive (panel stack en mobile)
- **Estimación**: 1.5h
- **Dependencia**: Tasks 2.2, 2.3, 2.4

### [x] Task 3.2: Crear FilterBadge Component (Filtros Activos)
- **Descripción**: Badge que muestra número de filtros activos
- **Ubicación**: `src/dishes.client/src/components/RecipeFilters/FilterBadge.tsx`
- **Props**:
  - `activeFilterCount: number`
  - `onClear?: () => void`
- **Criterio de aceptación**:
  - Renderiza badge con contador (e.g., "3")
  - Si `activeFilterCount === 0`, ocultar o deshabilitar
  - Click → llama `onClear()`
- **Estimación**: 0.5h
- **Dependencia**: Task 3.1

---

## Fase 4: Integración en Página de Listado

### [x] Task 4.1: Modificar RecipeListPage
- **Descripción**: Integrar RecipeFiltersPanel + sincronizar con URL y API
- **Archivo(s)**: `src/dishes.client/src/AppNew.vue` (ver nota abajo)
- **Nota**: Creado nuevo componente integrado con todos los filtros. El App.vue original quedó sin cambios (conflicto git posible). Se recomienda reemplazar manualmente el contenido de `App.vue` con el contenido de `AppNew.vue` después de resolver conflictos.
- **Cambios**:
  - Importar `useRecipeFilters` hook
  - Importar `RecipeFiltersPanel`
  - Layout: `[ RecipeFiltersPanel (left/top) | RecipeListView (main) ]`
  - Callback `onFiltersChange()` → actualizar URL con `setSearchParams()`
  - Mostrar `FilterBadge` con contador
  - Mostrar spinner si `isLoading`
  - Mostrar "No recipes found" si resultados vacíos
- **Criterio de aceptación**:
  - Página renderiza sin errores
  - Seleccionar filtro → URL actualizada
  - URL actualizada → recetas refrescadas
  - Back/Forward del navegador restaura filtros
  - Mobile responsive
- **Estimación**: 2h
- **Dependencia**: Tasks 2.1, 3.1, 3.2

### [ ] Task 4.2: Tests Manuales E2E
- **Descripción**: Validar flujo completo en navegador
- **Test Cases**:
  - [ ] Marcar 1 categoría → API llamada, URL updated, recetas filtradas
  - [ ] Marcar 2+ categorías → AND logic, recetas que cumplen todas
  - [ ] Seleccionar dificultad → filtrado correcto
  - [ ] Ajustar slider prep time → filtrado correcto
  - [ ] Combinación de 3 filtros → AND logic
  - [ ] Click "Clear Filters" → todos resetean, recetas sin filtro
  - [ ] Recargar página con URL params → filtros restaurados
  - [ ] Compartir URL con params → otro navegador muestra mismos resultados
  - [ ] Mobile: filtros accesibles, layout readable
- **Criterio de aceptación**: Todos los cases PASS
- **Estimación**: 2h
- **Dependencia**: Task 4.1

---

## Fase 5: Pulido y Optimización

### [ ] Task 5.1: Validación y Manejo de Errores
- **Descripción**: Validar query params, manejar edge cases
- **Archivo(s)**: Actualizar `useRecipeFilters` hook
- **Casos**:
  - URL params con valores inválidos → ignorar, usar defaults
  - Min prep time > max prep time → reordenar o rechazar
  - Categoría no existente → ignorar
  - API error (500, timeout) → mostrar error message amigable
- **Criterio de aceptación**:
  - Manual test de casos edge
  - Console logs limpios (no warnings)
  - UX graceful (sin crashes)
- **Estimación**: 1.5h
- **Dependencia**: Task 4.1

### [ ] Task 5.2: Optimización Performance
- **Descripción**: Debounce en cambios de filtro, memoization de componentes
- **Cambios**:
  - En `RecipeFiltersPanel`: Debounce `onFiltersChange()` 300ms (para slider)
  - En `useRecipeFilters`: Usar `useMemo()` para validación de params
  - En componentes: Envolver callbacks con `useCallback()`
- **Criterio de aceptación**:
  - Ajustar slider prep time → no re-fetches continuos (debounce 300ms)
  - Performance metrics OK (no unnecessary renders)
- **Estimación**: 1h
- **Dependencia**: Task 5.1

### [ ] Task 5.3: Documentación y Comentarios
- **Descripción**: Documentar hook, componentes, flujo de datos
- **Archivo(s)** a crear:
  - `src/dishes.client/README.md` (actualizar con RF-10 section)
  - Comentarios en código para lógica compleja
- **Contenido**:
  - Cómo usar `useRecipeFilters` hook
  - Estructura de `FilterState`
  - Conversión URL params ↔️ estado
  - Troubleshooting common issues
- **Criterio de aceptación**:
  - README updated
  - Código comentado (no excesivamente)
  - Otro dev puede entender flujo en 15min
- **Estimación**: 1h
- **Dependencia**: Task 5.2

---

## Fase 6: Testing Automático (Opcional - MVP)

### [ ] Task 6.1: Unit Tests para useRecipeFilters Hook
- **Ubicación**: `src/dishes.client/src/hooks/__tests__/useRecipeFilters.test.ts`
- **Coverage**:
  - Parseo correcto de URL params
  - Validación de valores
  - Llamadas a API
- **Estimación**: 2h
- **Dependencia**: Task 5.3

### [ ] Task 6.2: Component Tests para RecipeFiltersPanel
- **Ubicación**: `src/dishes.client/src/components/__tests__/RecipeFiltersPanel.test.tsx`
- **Coverage**:
  - Cambios de estado local
  - Llamadas a callbacks
  - Rendering de subcomponentes
- **Estimación**: 2h
- **Dependencia**: Task 5.3

---

## Resumen de Estimaciones

| Fase | Tareas | Horas | Track |
|------|--------|-------|-------|
| 1 | Auditoría Backend | 1h | Crítico |
| 2 | Hooks + Componentes Base | 5.5h | Crítico |
| 3 | Panel Filtros + Badge | 2h | Crítico |
| 4 | Integración + E2E | 4h | Crítico |
| 5 | Validación + Optimización + Docs | 3.5h | Crítico |
| 6 | Tests Automáticos | 4h | Opcional |
| **Total MVP** | **~16h** | - | - |
| **Total con Tests** | **~20h** | - | - |

---

## Priorización

**Must Have (MVP)**:
- Tasks 1.1-1.2 (auditoría)
- Tasks 2.1-2.4 (hooks + componentes)
- Tasks 3.1-3.2 (panel)
- Tasks 4.1-4.2 (integración + E2E)
- Tasks 5.1-5.2 (validación + performance)

**Nice to Have (Post-MVP)**:
- Task 5.3 (documentación completa)
- Tasks 6.1-6.2 (tests automáticos)
- Task 1.3 (backend extension si categorías no están en API)

---

## Notas Generales

- **Coordinar con backend** en Task 1.2 para confirmar API readiness
- **Considerar mobile-first** diseño desde Task 2.x (slider, panel layout)
- **Usar componentes de UI library existente** si el proyecto tiene (Material-UI, Headless UI, etc.)
- **Testear en navegadores modernos** (Chrome, Firefox, Safari, Edge)
- **Validar accesibilidad** (ARIA labels, keyboard navigation)
