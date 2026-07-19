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
