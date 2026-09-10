# RF-10: Filtros Múltiples para Recetas

## ¿Qué?

Agregar un sistema de filtros avanzado que permita a los usuarios filtrar recetas por **categoría, dificultad y tiempo de preparación** en la misma página de listado, con filtros acumulativos (AND lógico) que se reflejen en la URL y persistan en la sesión.

## ¿Por qué?

Actualmente, el listado de recetas solo tiene búsqueda por texto. Los usuarios necesitan formas más granulares de descubrir recetas que se ajusten a sus preferencias:
- **Por categoría**: Encontrar platos específicos (Main Course, Appetizers, Desserts, etc.)
- **Por dificultad**: Seleccionar recetas acordes a su nivel de habilidad (Easy, Medium, Hard)
- **Por tiempo de preparación**: Filtrar según disponibilidad de tiempo (15m a 2h+)

Esto mejora la experiencia de usuario, reduce el "friction" en la búsqueda y aumenta la probabilidad de que encuentren recetas relevantes.

## Aceptación

- Los filtros aparecen en la misma página que el listado de recetas
- Múltiples filtros funcionan con lógica AND (todas las condiciones se aplican)
- Los filtros se reflejan en la URL como query parameters
- La API backend ya soporta estos filtros en `GetRecipesList`
- El UI proporciona feedback visual claro sobre filtros activos
- Los filtros persisten al navegar o recargar la página (vía URL)
