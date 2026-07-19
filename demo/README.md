# Compliance 360 Demo Platform

Demo visual e interactiva standalone (sin backend, sin APIs, sin base de datos).

## Ejecutar localmente

Opciones:

1. Abrir `demo/index.html` con doble clic.
2. Recomendado: servir carpeta con HTTP local para comportamiento uniforme.

En PowerShell, desde la raíz:

```powershell
cd demo
python -m http.server 8800
```

Abrir: `http://localhost:8800`

## Estructura

```text
demo/
  index.html
  README.md
  DEMO_SCRIPT.md
  DEMO_MAP.md
  CERTIFICATION_MODE.md
  ROLE_TRAINING_MATRIX.md
  INPUT_DICTIONARY_BY_SCREEN.md
  assets/
    css/demo.css
    js/main.js
    data/mock-data.json
    images/
    icons/
    components/
    modules/
    wizards/
    tours/
```

## Funcionalidades incluidas

- Wizard principal (5 pasos).
- Explicación de arquitectura y multi-tenant (narrativa).
- Login enterprise simulado.
- Selector de rol con transición a dashboard.
- Módulos simulados navegables.
- Wizards simulados por proceso.
- Reportes y exportación simulada.
- Asistente IA simulado.
- Modo presentación automático.
- Controles de presentación: anterior, siguiente, pausar, continuar.
- Tour interactivo con globos.
- Favoritos de navegación persistentes.
- Mapa de dependencias entre módulos.
- Entrenamiento por rol (Fase 1: Quality Manager, Document Controller).
- Entrenamiento por rol (Fase 1, 2 y 3 implementadas).
- Diccionario funcional de inputs por pantalla clave.
- UI alineada al producto real (sidebar, topbar, hero, métricas, tablas, workspaces).
- Modo certificación por rol (checklist + quiz + badge).
- Dark/Light mode.
- Responsive desktop/tablet/mobile.

## Notas

- Todo dato es simulado.
- No hay autenticación real.
- No hay persistencia ni llamadas API.

