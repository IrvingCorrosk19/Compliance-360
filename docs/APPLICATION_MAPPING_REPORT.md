# APPLICATION_MAPPING_REPORT

## Backend Existente

Compliance 360 cuenta con backend modular en .NET 9, Minimal APIs, EF Core/PostgreSQL, JWT, RBAC, auditoria append-only y politicas multitenant.

Modulos backend certificados:

- Tenant Management: tenants, companies, settings, branding, subscription.
- Identity/Security: login, refresh, logout, password, MFA, RBAC, permissions.
- Document Management: tipos, categorias, documentos, versiones, aprobaciones, obsolescencia, permisos.
- Workflow Engine: workflows, steps, transitions, rules, instances, assignments, escalations, reminders.
- Technical Sheets: products, sheets, versions, ingredients, nutrients, certifications, approvals, PDF metadata.
- Supplier Management: suppliers, documents, validation, rejection, evaluations, homologation, alerts, suspension.
- Audit Management: programs, checklists, plans, audits, schedules, participants, areas, findings, evidence, observations, nonconformities, recommendations, dashboard, export.
- CAPA: creation, classification, owners, approvers, root cause, actions, evidence, follow-up, effectiveness, dashboard, export.
- Risk Management: categories, matrix, assessment, treatments, controls, reviews, indicators, heat map, dashboard, export.
- Quality Indicators: categories, formulas, targets, thresholds, periods, processes, measurements, results, trends, dashboard, export.
- Reporting Engine: definitions, templates, parameters, executions, schedules, subscriptions, exports, permissions, dashboard datasets.
- AuditLog and Notifications foundations.

## APIs y Permisos

API base: `/api/v1`.

Politicas principales:

- `Tenant.Manage`, `Identity.Manage`, `Rbac.Manage`, `Audit.Read`, `AuditManagement.Manage`.
- `Document.Manage`, `Workflow.Manage`, `TechnicalSheet.Manage`, `Supplier.Manage`.
- `Capa.Manage`, `Capa.Read`, `Capa.Approve`, `Capa.Close`.
- `Risk.Manage`, `Risk.Read`, `Risk.Approve`, `Risk.Close`.
- `Indicator.Manage`, `Indicator.Read`, `Indicator.Approve`, `Indicator.Export`.
- `Report.Manage`, `Report.Read`, `Report.Execute`, `Report.Export`, `Report.Schedule`.

## UI Implementada en Fase Actual

Se crea una SPA en `src/Compliance360.Web/wwwroot` servida por `Compliance360.Web`.

Pantallas implementadas:

- Login real contra `/api/v1/auth/login`.
- Executive Dashboard conectado a health, audit dashboard, CAPA dashboard, risk dashboard, quality indicators dashboard y reporting datasets.
- Compliance Dashboard usando los mismos datos ejecutivos reales.
- Document Management listado tenant-scoped y Action Center para crear documentos con tipo/categoria base.
- Technical Sheets listado tenant-scoped y Action Center para crear producto/ficha tecnica.
- Supplier Management listado tenant-scoped y Action Center para crear proveedores reales.
- Audit Management listado tenant-scoped y Action Center para crear programa, plan y auditoria.
- CAPA listado tenant-scoped, dashboard y Action Center para crear CAPAs reales.
- Risk Management listado, dashboard, heat map y Action Center para crear categoria, matriz y riesgo.
- Quality Indicators listado, dashboard, tendencias y Action Center para crear categoria, indicador, meta y umbrales.
- Report Center con catalogo estandar, seed idempotente de reportes, datasets, reportes configurados, ejecucion, exportacion y programacion.
- Audit Trail con busqueda de eventos.
- Shell de navegacion para Administration, Configuration, Security, Template Builder, Regulatory, Training, Supplier Portal y Customer Portal.

## UI/UX Cubierta

- Layout principal con sidebar, header, breadcrumbs, busqueda global, quick actions, profile/logout, tenant chip y theme selector.
- Modo claro/oscuro.
- Responsive layout.
- Estados de carga, vacio, error y exito.
- Toast notifications.
- Navegacion por teclado mediante controles HTML nativos y estilos focus-visible.

## Brechas Restantes

- Template Builder visual drag-and-drop avanzado.
- Regulatory Management backend y UI profundos.
- Training Management backend y UI profundos.
- Supplier Portal externo separado.
- Customer Portal externo separado.
- Observability avanzada con metrics/tracing externos.
- Auditorias finales de performance, accesibilidad y disaster recovery.

## Clasificacion Actual

Estado despues de esta fase: RELEASE CANDIDATE funcional de aplicacion enterprise core.

Evidencia verificada:

- `dotnet build Compliance360.sln`: exitoso, 0 warnings, 0 errores.
- `dotnet test Compliance360.sln --no-build`: exitoso, 169 tests passed.
- Validacion navegador: login, dashboard, Swagger Authorize, Supplier, Document, Technical Sheets, Audit, CAPA, Risk, Quality Indicators y Report Center.
- Acciones reales validadas: creacion tenant-scoped por modulo, seed de 24 reportes estandar, ejecucion de reporte, exportacion y programacion.
- Correcciones aplicadas: Swagger Bearer, seed idempotente de reportes y normalizacion EF/PostgreSQL para hijos append-only de Quality Indicators.

Razon: el core enterprise ya es navegable, consume APIs reales y permite operaciones principales end-to-end. Para declarar producto final absoluto aun deben completarse los modulos externos/avanzados listados en brechas.
