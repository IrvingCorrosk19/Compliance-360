# Manual interactivo — Administrador del Tenant

Guía de capacitación autocontenida para el rol **Administrador del Tenant** (*Tenant Administrator*) en Compliance 360.

## Abrir el manual

1. Abra `index.html` con doble clic en el explorador, **o**
2. Desde el navegador: `file:///…/docs/tenant-administrator-manual/index.html`

No requiere servidor, base de datos ni instalación de paquetes.

## Regenerar

```bash
python scripts/generate_tenant_admin_manual.py
```

## Fuente de verdad

La ruta solicitada `NuevoAnalisis/backoffice-web` **no existe** en este repositorio. El inventario se basó en:

- `src/Compliance360.Domain/Identity/RoleCatalog.cs`
- `src/Compliance360.Web/wwwroot/app.js` (administración del tenant, inicio de sesión, menú)
- Análisis en `analysis/`

## Contenido

| Archivo | Descripción |
|---------|-------------|
| `index.html` | Manual interactivo (portada, mapa, recorrido, pantallas) |
| `analysis/TENANT_ADMIN_SCREEN_INVENTORY.md` | Inventario de pantallas y pestañas |
| `analysis/TENANT_ADMIN_PERMISSION_MATRIX.md` | Matriz de permisos |
| `analysis/TENANT_ADMIN_TRACEABILITY.md` | Trazabilidad código ↔ documentación |

## Funciones interactivas

- Índice lateral y progreso (guardado en el navegador)
- Modo claro / oscuro
- Buscador y filtros (Básico / Intermedio / Avanzado; tipo de acción)
- Recorrido guiado de 9 pasos
- Recreaciones de inicio de sesión, administración, usuarios y roles
- Mensajes de éxito / error simulados

## Aclaración importante

Las vistas del manual son **simuladas**. No modifican datos reales. Practique primero aquí y luego use la aplicación en `http://localhost:5272`.

Algunas etiquetas de la aplicación real están en inglés; este manual las explica en español e indica el nombre que verá en pantalla cuando aplique.
