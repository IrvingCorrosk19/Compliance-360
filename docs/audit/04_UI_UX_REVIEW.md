# Revisión UI/UX

## Impresión honesta

UI **70/100**, UX **67/100**, responsive **84/100**, accesibilidad **61/100**. Al abrirlo transmite un SaaS B2B moderno y denso. No parece un proyecto universitario ni un CRUD barato. Tampoco alcanza el refinamiento, consistencia y onboarding de Salesforce, ServiceNow o Veeva.

## Componentes

| Área | Evidencia | Evaluación | Recomendación | Prioridad |
|---|---|---|---|---|
| Login | `wwwroot/index.html`, `app.js`, locales | Identidad visual clara y flujo tenant-aware | Eliminar últimos textos hardcoded y verificar autofill/a11y | P1 |
| Home/Dashboard | rutas SPA y dashboard regulatorio | Métricas útiles, alta densidad | Jerarquía por tarea y widgets configurables | P1 |
| Menús | RBAC basado en claims | Oculta acciones no permitidas | Añadir búsqueda y “favoritos”; validar backend siempre | P2 |
| Formularios | helpers `inputField`, formularios de tenant/regulatory | Reutilización aceptable | Errores inline, resumen de validación y dirty-state | P1 |
| Tablas | múltiples paneles administrativos | Potentes, densas en móvil | Column chooser y vista card móvil | P2 |
| Botones/modales | componentes CSS/JS propios | Mejor que `alert`; estados loading presentes | Focus trap, escape, restore-focus y confirmación uniforme | P0 |
| Iconografía | mezcla de símbolos/recursos | Funcional pero inconsistente | Sistema único de iconos accesibles | P2 |
| Tipografía/espaciado | CSS global y módulos | Profesional; densidad irregular | Escala tipográfica y spacing tokens obligatorios | P2 |
| Dark mode | estilos y prueba por rol | Implementado | Verificar contraste WCAG AA por token | P1 |
| Animaciones | `prefers-reduced-motion` parcial | Correcta intención | Cobertura global y evitar movimiento no esencial | P1 |
| Wizard | flujo dossier | Dominio complejo visible al usuario | Checklist progresivo, guardado y próximos pasos | P0 |

## Pruebas reales

- `responsive-i18n.spec.ts`: **9/9 roles PASS** en producción.
- RA-ADM y Quality Manager: PASS.
- TAC: primera ejecución falló al observar un valor anterior de branding; repetición aislada PASS. Esto evidencia sincronización no determinista, no una falla persistente confirmada.
- Platform Administrator: pantalla de acceso denegado por ausencia de rol productivo.
- `manual-roles-browser.spec.ts`: PASS en 1.0 min contra producción. Recorrió la matriz TAC + nueve roles regulatorios, comparó sidebars y tabs con el manual, ejecutó permisos negativos, capturó pantallas y confirmó ausencia de errores de consola/página. Evidencia: `docs/certification/evidence/manual-vs-system-role-matrix.json`.

## Nuevo usuario

Un usuario regulatorio entrenado requerirá aproximadamente 1–2 días para tareas básicas y varias semanas para dominar excepciones, SoD, reapertura y override. Un usuario sin experiencia se perderá en:

1. diferencia entre estados V1/V2;
2. quién puede ejecutar el siguiente paso;
3. requisitos documentales antes de avanzar;
4. navegación entre producto, dossier, corrección e historial.

Prioridad UX: mostrar “estado actual → próximo paso → actor requerido → requisito bloqueante”, con ayuda contextual basada en rol.

## Accesibilidad

Hay señales positivas (`:focus-visible`, responsive y reduced motion), pero no existe evidencia completa de WCAG 2.2 AA, lector de pantalla, navegación 100% teclado, contraste automatizado o auditoría axe en todas las rutas. No debe certificarse accesibilidad por inspección CSS parcial.
