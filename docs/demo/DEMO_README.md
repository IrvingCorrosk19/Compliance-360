# Compliance 360 — Enterprise Demo Playbook

Playbook oficial para demostración comercial frente a clientes Enterprise. Escenario: **Alimentos Premium Panamá S.A.**

## Archivos incluidos

| Archivo | Uso |
|---------|-----|
| `COMPLIANCE360_ENTERPRISE_DEMO_PLAYBOOK.docx` | Preparación completa: guion, roles, checklists, FAQ, cierre |
| `COMPLIANCE360_INTERACTIVE_DEMO_GUIDE.html` | Guía operativa en vivo (teleprompter, wizard, command palette) |
| `COMPLIANCE360_VISUAL_DEMO_GUIDE.html` | **Guía visual premium** — mockups HTML como la app + panel coach |

## Guía visual (recomendada para ensayar y presentar)

Abrir `COMPLIANCE360_VISUAL_DEMO_GUIDE.html` — replica las vistas reales de Compliance 360 en HTML estático (sin backend).

- **Izquierda:** mock de la app (login, sidebar, SuperAdmin, módulos, dashboard) con **hotspots dorados** en lo que debe señalar.
- **Derecha:** panel coach con tabs **Script · Acciones · Login · E2E**.
- **Rail de bloques 1–20** arriba para saltar en la narrativa.
- **Journey SVG** horizontal (10 fases) — clic para saltar de fase.
- **Spotlight bar** — siguiente paso destacado con botón «Siguiente paso →».
- **Progress ring** en header (% de bloques completados).
- **Modo Presentador** (`🎯 Presentar`) — maximiza script para proyección.
- **Modo Director** (`⚡ Director`) — avanza acciones automáticamente cada 3.5 s (ensayo solo).
- **Tema oscuro** (`🌙`) · **Pantalla completa** (`⛶`) para segunda pantalla.
- Overlay de **cambio de rol** cuando corresponde logout/login.
- Marca **“Vista simulada”** — no confundir con la app real.
- Atajos: `←` `→` bloques · `Espacio` siguiente acción · `G` recorrido · `J` flujo completo.

Funciona **offline**. Ideal en segundo monitor mientras la app corre en el primero, o sola para ensayo.

**Al abrir:** pantalla de bienvenida con las 10 fases clicables · botón **▶ Recorrido** (`G`) avanza bloque a bloque · **🎬 Flujo** (`J`) abre el mapa completo.

**Progreso local:** `localStorage` clave `c360_visual_demo_v2`.

## Flujo inicio a fin (10 fases · 20 bloques)

| Fase | Bloques | Rol | Qué demuestra |
|------|---------|-----|----------------|
| 1 Apertura | 1 | Presentador | Dolores del cliente — sin abrir app |
| 2 Plataforma SaaS | 2–3 | Platform Admin | SuperAdmin, tenants, multi-tenancy |
| 3 Tenant & RBAC | 4–5 | Plat → Tenant Admin | TAC, usuarios, 12 roles |
| 4 Documentos + SoD | 6–7 | Doc Control → QM | BPM-LIM-001, elaborador≠aprobador |
| 5 Auditoría | 8–9 | Auditor | AUD-INT-2026-01, hallazgo CCP |
| 6 CAPA | 10 | CAPA Manager | CAPA-TEMP-001 |
| 7 Riesgos & KPIs | 11–12 | Risk → KPI Mgr | RISK-CCP-01, KPI-NC-Rate |
| 8 Reportes | 13 | Reporting Manager | Seed + ejecutar reportes |
| 9 Infraestructura | 14–16 | Storage → Notif → Viewer | SoD storage/email, solo lectura |
| 10 Cierre | 17–20 | Tenant Admin → Presentador | Dashboard, ROI, FAQ, piloto |

**Cadena E2E de artefactos:** Dolor → Plataforma → Tenant → RBAC → BPM-LIM-001 → SoD → AUD → Hallazgo → CAPA → Riesgo → KPI → Reportes → Storage → Email → Viewer → Dashboard → ROI → Piloto

## Cómo abrir el HTML interactivo (Ultra v4)

1. Doble clic en `COMPLIANCE360_INTERACTIVE_DEMO_GUIDE.html` (Chrome o Edge recomendado).
2. No requiere internet ni instalación: todo CSS/JS está embebido.
3. Colocar la guía en una **segunda pestaña** o **segundo monitor**.
4. La aplicación Compliance 360 va en otra pestaña: `http://localhost:5272`
5. Navegación E2E (guía **Ultra** v4):
   - **Sidebar** con rol en cada bloque, cronómetro global y preview del siguiente bloque.
   - **Stepper de fases** + **journey de roles** + **pipeline de artefactos** (clic → salta al bloque).
   - **Panel login sticky** (desktop) — tenant, email, password, copiar todo, abrir ruta.
   - **Tabs:** Login & Rol · Flujo E2E · Acciones · **UI Focus** · Qué decir · **Recovery** · FAQ · Checklist.
   - **Modo wizard** (`W`) — una acción a la vez en overlay grande.
   - **Teleprompter fullscreen** (`F`) — script a pantalla completa.
   - **Modo Presentador** — script ampliado + spotlight en acción actual.
   - **Modo Director** — avanza acciones automáticamente (ensayo).
   - **Journey SVG** por bloque — mapa de 10 fases clickeable.
   - **Spotlight bar** — «Siguiente paso» con pulso visual.
   - **Progress ring** en sidebar (% completado).
   - **Command palette** (`Ctrl+K`) — saltar a bloque, rol, recurso o acción.
   - **Tracker de sesión** — muestra rol logueado en la app (auto-actualiza al cambiar bloque).
   - **Presupuesto de tiempo** por bloque (elapsed vs duración planificada).
   - **Tema oscuro** (`D`) para segunda pantalla en salas con poca luz.
   - Recursos: Mapa E2E · Roles · **Journey de roles** · FAQ · Checklists · Cierre.
   - Atajos: `L` login · `A` acciones · `S` script · `R` recovery · `U` UI focus · `F` teleprompter · `W` wizard · `G` recorrido guiado · `J` flujo completo · `O` abrir app · `Ctrl+K` palette · `←` `→` bloques.
6. Marque checklists por bloque; el progreso se guarda en `localStorage` (clave `c360_demo_progress_v4`).
7. **Reiniciar demo** borra progreso, notas y sesión locales.

## Cómo usar el Word

1. Abrir `COMPLIANCE360_ENTERPRISE_DEMO_PLAYBOOK.docx` en Microsoft Word o LibreOffice.
2. Leer la portada y el escenario de Alimentos Premium Panamá S.A.
3. Revisar la tabla de **12 roles** antes de la demo.
4. Estudiar los **20 bloques** en orden narrativo (no es el orden del menú de la app).
5. Para cada bloque: memorizar el **Customer Script**, las acciones en pantalla y las advertencias.
6. Usar los **Anexos** para checklists y FAQ global.
7. Imprimir o exportar a PDF el Anexo A (credenciales) solo para uso interno — no proyectar contraseñas.

## Preparación antes de la demo (60–90 min)

### Infraestructura

```powershell
# Desde la raíz del repositorio
dotnet run --project src/Compliance360.Web
```

Verificar que responde: `http://localhost:5272`

PostgreSQL debe estar activo con el tenant de demo provisionado (Development Bootstrap).

### Credenciales de demo

**Base URL:** `http://localhost:5272`  
**Tenant operativo:** `ddcaf211-afe0-44a0-9c90-4fbda8fc4871`

| Rol | Email | Password |
|-----|-------|----------|
| Platform Administrator | admin@compliance360.local | OwnerStart!2026 |
| Tenant Administrator | tenantadmin@alimentos-premium.test | Premium!2026 |
| Document Controller | doccontrol@alimentos-premium.test | Premium!2026 |
| Quality Manager | quality@alimentos-premium.test | Premium!2026 |
| Auditor | auditor@alimentos-premium.test | Premium!2026 |
| CAPA Manager | capa@alimentos-premium.test | Premium!2026 |
| Risk Manager | risk@alimentos-premium.test | Premium!2026 |
| Indicators Manager | indicators@alimentos-premium.test | Premium!2026 |
| Reporting Manager | reporting@alimentos-premium.test | Premium!2026 |
| Storage Administrator | storage@alimentos-premium.test | Premium!2026 |
| Notification Administrator | notifications@alimentos-premium.test | Premium!2026 |
| Viewer | viewer@alimentos-premium.test | Premium!2026 |

Platform Admin usa tenant platform: `dc7c46ee-cb25-4ed5-b0b4-800788f7f626`

### Datos que deben estar listos

- Tenant **Alimentos Premium** con usuarios y roles precargados (Development Bootstrap).
- Documentos de ejemplo (BPM, procedimientos HACCP) — opcional crear uno en vivo en Bloque 6.
- Al menos una auditoría y un CAPA existentes aceleran bloques 8–10.
- Indicadores y reportes: Reporting Manager puede ejecutar **Seed Reports** si la lista está vacía.

### Checklist previo (resumen)

- [ ] App corriendo y login verificado con 2–3 roles
- [ ] Guía HTML abierta y probada
- [ ] Notificaciones del SO silenciadas
- [ ] Resolución 1920×1080 en proyector
- [ ] Contraseñas **no** visibles en pantalla compartida (usar gestor o copiar antes)
- [ ] Segunda persona de apoyo IT (opcional) por si hay caída de red

## Secuencia narrativa (por qué no sigue el menú)

La demo cuenta una historia de negocio:

1. Dolor del cliente → 2. Visión plataforma → 3–5. Gobierno y RBAC → 6–7. Control documental + SoD → 8–9. Auditoría y hallazgos → 10. CAPA → 11. Riesgos → 12. Indicadores → 13. Reportes → 14–15. Storage y notificaciones → 16. Viewer → 17. Dashboard ejecutivo → 18–20. Valor, FAQ y cierre

Prioridad: **valor para el cliente**, no recorrido exhaustivo de menús.

## Dependencias third-party pendientes (transparencia comercial)

Mencionar con honestidad en Bloque 19 (FAQ):

| Componente | Estado demo | Producción |
|------------|-------------|------------|
| SMTP / Email | Proveedor local o mock | Configurar SMTP corporativo |
| Cloud storage | Local filesystem | Azure Blob / S3 / GCS |
| SSO / SAML | Login local | Azure AD / Okta |
| Firma digital | No incluida en demo | Integración por proyecto |
| IA / analytics | Roadmap | Según contrato |

La plataforma está **certificada funcionalmente** (238 unit tests, 29/29 E2E, 23/23 customer journey). La demo muestra producto listo para piloto; la configuración enterprise es trabajo de implementación, no de desarrollo core.

## Regenerar los entregables

Si se actualiza el contenido en `scripts/demo_playbook_content.py`:

```powershell
cd "c:\Proyectos\Compliance 360"
.venv\Scripts\python.exe scripts\generate_enterprise_demo_playbook.py
```

Requisito: `python-docx` en el entorno virtual del proyecto.

## Soporte durante la demo

- Si un módulo falla: narrar el **beneficio de negocio** del bloque mientras recupera (logout/login, refresh).
- Si el cliente pregunta algo no documentado: anotar en **Notas del presentador** (HTML) y comprometer respuesta por escrito.
- No improvisar permisos ni funcionalidades no certificadas.

---

*Confidential — Commercial Use Only*
