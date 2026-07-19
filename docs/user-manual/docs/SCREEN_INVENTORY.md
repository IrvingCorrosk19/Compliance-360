# SCREEN_INVENTORY

| Name | Route | Module | Objective |
|------|-------|--------|-----------|
| Inicio de sesión | `#/login` | Auth | Identificar usuario y obtener token JWT para el tenant. |
| Tenant Administration | `#/tenant-administration` | Enterprise | Gestionar usuarios, roles y perfil del tenant. |
| Security | `#/security` | Enterprise | Configuración de seguridad del tenant (MFA/SSO según permisos). |
| Audit Trail | `#/audit-trail` | Command Center | Consultar eventos de auditoría. |
| Consola de Asuntos Regulatorios | `#/regulatory` | Regulatory Affairs | Contenedor de todas las vistas RA. |
| Dashboard RA | `#/regulatory → Dashboard` | Regulatory Affairs | Métricas de productos, CT activos, en trámite, requisitos críticos, detenidos >14d, por vencer. |
| Portafolio | `#/regulatory → Portafolio` | Regulatory Affairs | Listar productos y crear producto + expediente. |
| Pipeline | `#/regulatory → Pipeline` | Regulatory Affairs | Kanban por estado del expediente + Vencido + Renovación. |
| Expedientes | `#/regulatory → Expedientes` | Regulatory Affairs | Listar y abrir el workspace del expediente. |
| Workspace del expediente | `#/regulatory → Expedientes → detalle` | Regulatory Affairs | Ejecutar prep, revisión, aprobación interna, sometimiento, observación y aprobación externa. |
| Registros CT/RS | `#/regulatory → Registros CT/RS` | Regulatory Affairs | Consultar números CT/RS, emisión y vencimiento. |
| Fabricantes | `#/regulatory → Fabricantes` | Regulatory Affairs | Alta de fabricante y certificados. |
| Licencias | `#/regulatory → Licencias` | Regulatory Affairs | Registrar licencias operativas. |
| Alertas | `#/regulatory → Alertas` | Regulatory Affairs | Evaluar alertas de vencimiento / riesgo operativo. |
| Importación | `#/regulatory → Importación` | Regulatory Affairs | Stage XLSX del libro REGUTRACK. |
| Configuración | `#/regulatory → Configuración` | Regulatory Affairs | Bootstrap regulatorio (autoridades + pack). |
| SoD Settings | `#/regulatory → SoD Settings` | Regulatory Affairs | Consultar política de segregación de funciones. |
| Alert Center · Inbox | `#/alert-center` | Alert Center | Bandeja de notificaciones persistentes por usuario (campana). Leer, filtrar, marcar, archivar y favoritos. |
| Alert Center · Template Center | `#/alert-center → Template Center` | Alert Center | Gestionar plantillas versionadas con sanitización HTML y aprobación maker-checker. |
| Alert Center · Reglas | `#/alert-center → Reglas` | Alert Center | Definir condiciones, destinatarios, canales y simulación de reglas de alerta. |
| Alert Center · Scheduler | `#/alert-center → Programaciones` | Alert Center | Programaciones durables (cron), calendarios y quiet hours. |
| Alert Center · Destinatarios | `#/alert-center → Destinatarios` | Alert Center | Directorio de resolución: Owner, Role, Group, Department, listas y externos autorizados. |
| Alert Center · Provider Center | `#/alert-center → Proveedores` | Alert Center | Configurar proveedores de entrega (SMTP/M365/SendGrid/etc.), secretos write-only y pruebas de conexión. |
| Alert Center · Operaciones | `#/alert-center → Operaciones` | Alert Center | Telemetría de cola, mensajes, DLQ, reintentos y exportación CSV. |
