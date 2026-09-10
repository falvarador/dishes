# Diseño: Filtros Múltiples para Recetas (RF-10)

## Arquitectura de Alto Nivel

```
┌─────────────────────────────────────────────────────────┐
│ RecipeListPage                                          │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌──────────────────┐  ┌──────────────────────────┐  │
│  │ RecipeFilters    │  │ RecipeListView           │  │
│  │ (Panel Left)     │  │                          │  │
│  ├──────────────────┤  ├──────────────────────────┤  │
│  │ Categories       │  │ [Filter Badge]           │  │
│  │ Difficulty       │  │ [Search + Clear Btn]     │  │
│  │ Prep Time        │  │                          │  │
│  │ [Apply Filters]  │  │ [RecipeCard x N]         │  │
│  │ [Clear Filters]  │  │                          │  │
│  └──────────────────┘  │ [Pagination]             │  │
│                        └──────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## Componentes Frontend

### 1. **RecipeFiltersPanel** (Nueva componente)
- **Responsabilidad**: Renderizar UI de filtros y manejar cambios de estado local
- **Props**:
  - `onFiltersChange: (filters: FilterState) => void`
  - `initialFilters?: FilterState`
- **Estado Local**:
  - `selectedCategories: string[]`
  - `selectedDifficulty: string | null`
  - `prepTimeRange: [number, number]` (en minutos)
- **Componentes internos**:
  - `CategoryCheckboxGroup`: Renderiza checkboxes para cada categoría
  - `DifficultyChipGroup`: Renderiza chips (Easy, Medium, Hard)
  - `PrepTimeSlider`: Slider de rango custom

### 2. **RecipeListPage** (Modificar existente)
- **Responsabilidad**: Orquestar filtros, estado URL y API calls
- **Cambios**:
  - Leer query params al montar (React Router `useSearchParams`)
  - Pasar `initialFilters` a `RecipeFiltersPanel`
  - Escuchar cambios de filtros y actualizar URL
  - Llamar API con query params actualizados

### 3. **useRecipeFilters** (Hook custom - Nueva)
- **Responsabilidad**: Sincronizar estado local ↔️ URL params ↔️ API
- **Lógica**:
  ```typescript
  const {
	filters,
	isLoading,
	recipes,
	totalCount,
	setFilters,
	clearFilters
  } = useRecipeFilters();
  ```
- **Internamente**:
  - Parsea URL params
  - Valida valores
  - Llama `GetRecipesList` API
  - Maneja errores

---

## Gestión de Estado

### URL Query Params → Estado Filtros

```
URL: ?categories=Main%20Course,Appetizers&difficulty=Easy&prepTimeMin=15&prepTimeMax=120

↓ parsear

FilterState {
  categories: ["Main Course", "Appetizers"],
  difficulty: "Easy",
  prepTimeRange: [15, 120]
}

↓ validar + normalizar

↓ usar en API call
```

### Cambio en FilterPanel → Actualizar URL

```
Usuario marca checkbox "Main Course"

↓ local state update en RecipeFiltersPanel

↓ onFiltersChange callback fired

↓ RecipeListPage recibe nuevo estado

↓ actualizar URL params con setSearchParams()

↓ hook useRecipeFilters detecta cambio

↓ refetch API con nuevos params
```

---

## Especificación de API

### Endpoint (Existente)
```
GET /api/recipes
Query Params:
  - status?: 0|1|2 (Draft, Published, Archived)
  - difficulty?: "Easy" | "Medium" | "Hard"
  - prepTime?: "15m" | "30m" | "1h" | "1.5h" | "2h+"
  - search?: string
  - categories?: string[] (si RF-10 adiciona soporte backend)
  - page?: number (default 1)
  - pageSize?: number (default 10)
  - sortBy?: "CreatedAt" | ... (default "CreatedAt")
  - sortOrder?: "asc" | "desc" (default "desc")
```

### Backend Considerations
- **Revisar `GetRecipesListHandler`**: ¿Ya filtra por `difficulty` y `prepTime`?
- **Si necesita extension**: Agregar soporte para filtro de `categories` (INNER JOIN con tabla RecipeCategory si existe)
- **No implementar** cambios backend en este ciclo si ya hay filtros básicos; solo UI.

---

## Flujo de Datos (Secuencia)

```
[ Usuario marca checkbox ]
		 ↓
[ RecipeFilterPanel: setState ]
		 ↓
[ Callback onFiltersChange() ]
		 ↓
[ RecipeListPage: setSearchParams(url) ]
		 ↓
[ URL cambió: ?categories=... ]
		 ↓
[ useRecipeFilters hook detecta cambio ]
		 ↓
[ Valida + normaliza query params ]
		 ↓
[ Llamar API: GET /api/recipes?categories=...&... ]
		 ↓
[ Recibir recipes[] ]
		 ↓
[ Renderizar en RecipeListView ]
```

---

## Decisiones de Diseño

| Aspecto | Decisión | Razón |
|--------|----------|-------|
| **Ubicación filtros** | Panel lado izquierdo (desktop) / arriba (mobile) | UX común, visible |
| **Aplicación filtros** | Automática al cambiar (sin botón Apply) | Feedback inmediato |
| **Tipo dificultad** | Solo una selección (radio, no checkbox) | Mutuamente excluyente |
| **Persistencia** | Via URL query params | Bookmarkable, shareable |
| **Validación** | Cliente + servidor | Defense in depth |
| **Lazy load categorías** | No (hardcode categorías comunes) | MVP simple, sin extra API |

---

## Casos de Uso Edge

1. **URL con params inválidos**: Ignorar inválidos, usar defaults
2. **Recargar página con filtros**: Restaurar desde URL automáticamente
3. **Múltiples categorías sin dificultad**: Fully supported (AND lógico)
4. **Prep time min > max**: Validar en cliente, rechazar
5. **Categoria inexistente**: Ignorar, no mostrar error

---

## Dependencias y Riesgos

| Item | Impacto | Mitigación |
|-----|---------|------------|
| Backend no filtra por categoría | Alto | Revisar `RecipeIngredient` vs `RecipeCategory` en modelo; hardcode si no existe relación |
| Demasiados params en URL | Medio | Reemplazar con object param encoding si URL crece |
| Performance con muchas recetas | Bajo | Usar pagination (ya implementada) |
| Incompatibilidad React Router v5 | Medio | Usar v6+ o adapter shimming |
