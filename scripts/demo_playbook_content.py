"""Source of truth for Compliance 360 Enterprise Demo Playbook."""

BASE_URL = "http://localhost:5272"
TENANT_ID = "ddcaf211-afe0-44a0-9c90-4fbda8fc4871"
PLATFORM_TENANT = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626"

COMPANY = {
    "name": "Alimentos Premium Panamá S.A.",
    "industry": "Manufactura de alimentos",
    "standards": "ISO 9001, ISO 22000, HACCP, BPM",
    "pain_points": [
        "Documentos en Excel sin control de versiones",
        "Auditorías manuales sin trazabilidad centralizada",
        "CAPA sin seguimiento estructurado",
        "Riesgos operativos sin matriz viva",
        "Indicadores calculados manualmente",
        "Reportes lentos para gerencia",
        "Sin segregación de funciones por rol",
        "Evidencias dispersas en carpetas y correos",
    ],
}

CREDENTIALS = {
    "Platform Administrator": ("admin@compliance360.local", "OwnerStart!2026", PLATFORM_TENANT),
    "Tenant Administrator": ("tenantadmin@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "Document Controller": ("doccontrol@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "Quality Manager": ("quality@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "Auditor": ("auditor@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "CAPA Manager": ("capa@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "Risk Manager": ("risk@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "Indicators Manager": ("indicators@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "Reporting Manager": ("reporting@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "Storage Administrator": ("storage@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "Notification Administrator": ("notifications@alimentos-premium.test", "Premium!2026", TENANT_ID),
    "Viewer": ("viewer@alimentos-premium.test", "Premium!2026", TENANT_ID),
}

ROLES_OVERVIEW = [
    ("Platform Administrator", "Administra la plataforma SaaS completa.", "Crear tenants, lifecycle, auditoría global.", "No opera datos de negocio del tenant.", "Gobierno multi-cliente y time-to-market."),
    ("Tenant Administrator", "Dueño operativo del tenant.", "Configura empresa, usuarios, roles, licencias.", "No aprueba documentos ni cierra CAPA.", "Autonomía del cliente sin depender del vendor."),
    ("Document Controller", "Elabora documentación controlada.", "Crea tipos, categorías y documentos.", "No puede aprobar (SoD).", "Control documental ISO sin Excel."),
    ("Quality Manager", "Aprueba y coordina calidad.", "Aprueba documentos, CAPA, riesgos, indicadores.", "No elabora registros operativos.", "Segregación elaborador/aprobador auditable."),
    ("Auditor", "Planifica y ejecuta auditorías.", "Crea programas y auditorías.", "No gestiona CAPA de cierre.", "Auditoría integrada con hallazgos trazables."),
    ("CAPA Manager", "Gestiona acciones correctivas.", "Crea y clasifica CAPA.", "No aprueba cierre final.", "Ciclo CAPA estructurado 5-Why/Ishikawa."),
    ("Risk Manager", "Administra registro de riesgos.", "Crea categorías, matrices y riesgos.", "No cierra riesgos sin QM.", "Matriz de riesgo viva con heat map."),
    ("Indicators Manager", "Gestiona KPIs de calidad.", "Crea indicadores, metas y umbrales.", "No altera datos de producción.", "Indicadores automáticos para gerencia."),
    ("Reporting Manager", "Centro de reportes.", "Semilla, ejecuta y exporta reportes.", "No modifica datos fuente.", "Reportes ejecutivos en minutos."),
    ("Storage Administrator", "Administra almacenamiento.", "Configura proveedores de archivos.", "No administra SMTP (SoD).", "Evidencias centralizadas y seguras."),
    ("Notification Administrator", "Administra notificaciones.", "Configura proveedores de email.", "No administra storage (SoD).", "Alertas automáticas a responsables."),
    ("Viewer", "Solo lectura transversal.", "Consulta módulos autorizados.", "No crea ni modifica nada.", "Transparencia sin riesgo operativo."),
]

# Optimal story-driven sequence (not menu order)
BLOCKS = [
    {
        "id": 1,
        "title": "Bienvenida y problema del cliente",
        "duration": "5 min",
        "role": "Presentador",
        "user": "—",
        "screen": "Slide / conversación",
        "route": "—",
        "menu": "—",
        "actions": [
            "Saludar a gerencia, calidad, IT y operaciones.",
            "Presentar el escenario: Alimentos Premium Panamá S.A.",
            "Enumerar dolores: Excel, auditorías manuales, CAPA sin trazabilidad.",
        ],
        "buttons": "—",
        "data": "—",
        "result": "Cliente identifica sus propios dolores en la narrativa.",
        "script": (
            "Buenos días. Hoy no les mostraré 'un software más'. Les mostraré cómo Alimentos Premium Panamá "
            "puede pasar de controles en Excel y carpetas compartidas a un sistema Enterprise donde cada acción "
            "queda registrada, cada rol tiene límites claros y la gerencia ve el cumplimiento en tiempo real. "
            "Imaginen su planta de manufactura de alimentos bajo ISO 9001, ISO 22000 y HACCP: documentos versionados, "
            "auditorías con evidencia, CAPA con causa raíz, riesgos con matriz viva e indicadores que alimentan "
            "reportes ejecutivos sin trabajo manual."
        ),
        "benefit": "Enmarcar la demo como transformación de negocio, no como IT.",
        "faq": [
            ("¿Esto reemplaza nuestro ERP?", "No. Compliance 360 gobierna cumplimiento, calidad y riesgos. Se integra con ERP/MES vía API."),
            ("¿Cuánto tarda implementar?", "Tenant operativo en días; módulos se activan progresivamente según madurez."),
        ],
        "warnings": "No abrir la aplicación aún. Conectar con dolores reales del cliente.",
        "checklist": ["Confirmar asistentes clave", "Validar proyector y resolución", "Mencionar empresa ficticia"],
    },
    {
        "id": 2,
        "title": "Visión general de Compliance 360",
        "duration": "5 min",
        "role": "Platform Administrator",
        "user": "admin@compliance360.local",
        "screen": "SuperAdmin Platform Center",
        "route": "#/superadmin-platform",
        "menu": "Operations → SuperAdmin Platform",
        "actions": [
            f"Abrir {BASE_URL} en pestaña 1 (aplicación).",
            "Abrir COMPLIANCE360_INTERACTIVE_DEMO_GUIDE.html en pestaña 2 (guía).",
            "Login: Tenant ID platform, admin@compliance360.local.",
            "Navegar a SuperAdmin Platform Center.",
            "Mostrar tab Executive: salud global, tenants activos.",
        ],
        "buttons": "Tab 'Executive' · Tab 'Tenants'",
        "data": f"Tenant ID plataforma: {PLATFORM_TENANT}",
        "result": "Vista de gobierno global SaaS multi-tenant.",
        "script": (
            "Compliance 360 es una plataforma Enterprise multi-tenant. Desde un único centro de gobierno "
            "ustedes administran clientes, licencias, seguridad global y auditoría de plataforma. Cada cliente "
            "—como Alimentos Premium— opera en su propio espacio aislado con usuarios, roles y datos separados. "
            "Esto es arquitectura SaaS de nivel consultoría: un producto, muchos clientes, cero mezcla de información."
        ),
        "benefit": "Confianza Enterprise: multi-tenancy, gobierno central, escala comercial.",
        "faq": [
            ("¿Dónde se aloja?", "Cloud o on-premise según contrato; PostgreSQL + API .NET 9."),
            ("¿Hay un solo SuperAdmin?", "Modelo RBAC: Platform Administrator, Operations, Security, Support con permisos explícitos."),
        ],
        "warnings": "No entrar aún a módulos de negocio; mantener narrativa plataforma.",
        "checklist": ["Login exitoso", "Tab Executive visible", "Contador de tenants activos"],
    },
    {
        "id": 3,
        "title": "Creación y activación del cliente",
        "duration": "5 min",
        "role": "Platform Administrator",
        "user": "admin@compliance360.local",
        "screen": "SuperAdmin → Tenants",
        "route": "#/superadmin-platform",
        "menu": "Tab Tenants",
        "actions": [
            "En tab Tenants, mostrar listado de tenants existentes.",
            "Señalar 'Alimentos Premium Panama S.A.' ya provisionado (tenant demo).",
            "Opcional: mostrar formulario #create-tenant-form para explicar onboarding.",
            "Explicar lifecycle: Trial → Active → Suspended.",
        ],
        "buttons": "Tab 'Tenants' · Botón crear tenant (si demo en vivo)",
        "data": "Nombre: Alimentos Premium Panamá · Slug: alimentos-premium · Tax ID único",
        "result": "Cliente ve cómo un nuevo tenant nace en minutos.",
        "script": (
            "Cuando firman contrato, creamos su tenant en segundos: identidad legal, slug, moneda, país. "
            "El tenant inicia en Trial, pasa a Active cuando ustedes están listos, y puede suspenderse "
            "por gobierno comercial sin perder datos. Hoy usamos Alimentos Premium Panamá, ya configurado "
            "para esta demostración."
        ),
        "benefit": "Time-to-value: de contrato a tenant operativo el mismo día.",
        "faq": [
            ("¿Podemos tener varios tenants?", "Sí, para holdings o ambientes dev/test/prod."),
            ("¿Qué pasa al suspender?", "Acceso bloqueado; datos preservados; reactivación inmediata."),
        ],
        "warnings": "Si crea tenant en vivo, usar Tax ID único para evitar error 400.",
        "checklist": ["Tenant Alimentos Premium visible", "Estado Active confirmado"],
    },
    {
        "id": 4,
        "title": "Administración del tenant",
        "duration": "6 min",
        "role": "Platform Administrator → Tenant Administrator",
        "user": "admin@compliance360.local luego tenantadmin@alimentos-premium.test",
        "screen": "Tenant Administration Center",
        "route": "#/tenant-administration",
        "menu": "Enterprise → Tenant Administration",
        "actions": [
            "Como Platform Admin: clic en tenant Alimentos Premium → ir a Tenant Administration.",
            "Logout. Login como Tenant Administrator.",
            f"Tenant ID: {TENANT_ID}",
            "Mostrar hero del tenant: nombre legal, estado, plan, usuarios, storage.",
            "Tab General: datos empresariales, industria alimentos.",
            "Tab Branding: logo, colores corporativos.",
        ],
        "buttons": "Tabs 'General' y 'Branding'",
        "data": "Industria: Manufactura de alimentos · País: PA · Moneda: USD",
        "result": "Perfil Enterprise completo del tenant visible.",
        "script": (
            "Este es el corazón administrativo del cliente. Aquí vive la identidad de Alimentos Premium: "
            "razón social, RUC, industria alimentaria, branding para portal y comunicaciones. "
            "La gerencia ve de un vistazo cuántos usuarios consume la licencia, cuánto storage ocupa "
            "y el estado comercial del contrato."
        ),
        "benefit": "Single pane of glass para IT y calidad: configuración sin tickets al vendor.",
        "faq": [
            ("¿Personalizamos logo y colores?", "Sí, branding por tenant incluido."),
            ("¿Quién administra esto?", "Tenant Administrator; plataforma solo para gobierno SaaS."),
        ],
        "warnings": "Cambiar tenant en localStorage si navega desde Platform Admin.",
        "checklist": ["Hero TAC visible", "Tabs General y Branding recorridos"],
    },
    {
        "id": 5,
        "title": "Usuarios, roles y permisos",
        "duration": "6 min",
        "role": "Tenant Administrator",
        "user": "tenantadmin@alimentos-premium.test",
        "screen": "TAC → Usuarios y Roles",
        "route": "#/tenant-administration",
        "menu": "Tabs 'Usuarios' y 'Roles & Permisos'",
        "actions": [
            "Tab Usuarios: listar usuarios especialistas (Document Controller, QM, Auditor, etc.).",
            "Mostrar asignación de roles por usuario.",
            "Tab Roles & Permisos: explicar catálogo RBAC oficial.",
            "Destacar: permisos pequeños, no bypass por nombre de rol.",
        ],
        "buttons": "Tabs 'users' y 'rbac'",
        "data": "—",
        "result": "Matriz de roles clara para IT y auditor externo.",
        "script": (
            "Compliance 360 no usa 'admin ve todo'. Cada persona tiene un rol con permisos explícitos: "
            "elaborar documentos, aprobar, auditar, gestionar CAPA, ver reportes. Esto es Segregación "
            "de Funciones diseñada para ISO y SOX: quien elabora no aprueba, quien almacena no envía correos, "
            "quien audita no cierra sus propias no conformidades."
        ),
        "benefit": "RBAC auditable reduce riesgo regulatorio y conflictos internos.",
        "faq": [
            ("¿Integramos con Active Directory?", "SSO OIDC/SAML/LDAP configurable por tenant (PENDING producción)."),
            ("¿Roles personalizados?", "Sí, desde catálogo oficial con permisos granulares."),
        ],
        "warnings": "No mostrar contraseñas en pantalla compartida.",
        "checklist": ["Lista de 12+ usuarios demo", "Roles asignados visibles"],
    },
    {
        "id": 6,
        "title": "Control documental",
        "duration": "7 min",
        "role": "Document Controller",
        "user": "doccontrol@alimentos-premium.test",
        "screen": "Document Management",
        "route": "#/documents",
        "menu": "Operations → Document Management",
        "actions": [
            "Logout Tenant Admin. Login Document Controller.",
            "Ir a Document Management (#/documents).",
            "Mostrar listado de documentos existentes.",
            "En Action Center (#module-action-form): completar Code y Name.",
            "Ejemplo Code: BPM-LIM-001 · Name: Límite crítico CCP Limpieza.",
            "Clic Submit del formulario.",
            "Mostrar toast de éxito y documento en tabla.",
        ],
        "buttons": "#module-action-form button[type=submit]",
        "data": "Code: BPM-LIM-001 · Name: Límite crítico CCP Limpieza de línea",
        "result": "Documento controlado creado con código único auditable.",
        "script": (
            "Adiós Excel suelto. El Document Controller crea procedimientos BPM y registros HACCP "
            "con código único, tipo y categoría. Cada documento nace en estado controlado y queda "
            "registrado en auditoría. En producción, aquí se adjuntan versiones PDF y se envían "
            "a revisión antes de publicación."
        ),
        "benefit": "Trazabilidad documental ISO 9001/22000 sin carpetas compartidas.",
        "faq": [
            ("¿Control de versiones?", "Sí vía API; UI de versiones en roadmap."),
            ("¿Firma electrónica?", "Integración third-party configurable (PENDING)."),
        ],
        "warnings": "Document Controller NO debe ver botón de aprobar.",
        "checklist": ["Formulario visible", "Toast success", "Documento en listado"],
    },
    {
        "id": 7,
        "title": "Aprobación documental (SoD)",
        "duration": "5 min",
        "role": "Quality Manager",
        "user": "quality@alimentos-premium.test",
        "screen": "Document Management (solo lectura)",
        "route": "#/documents",
        "menu": "Operations → Document Management",
        "actions": [
            "Logout Document Controller. Login Quality Manager.",
            "Ir a Document Management.",
            "Mostrar que NO aparece formulario de creación (#module-action-form ausente).",
            "Explicar: QM tiene permiso DOCUMENT.APPROVE en JWT.",
            "Narrar flujo: elaboración → revisión → aprobación → publicación.",
            "Mencionar que aprobación formal se ejecuta vía workflow API certificado.",
        ],
        "buttons": "— (solo lectura en UI actual)",
        "data": "—",
        "result": "Cliente entiende segregación elaborador vs aprobador.",
        "script": (
            "Observen: el Quality Manager entra al mismo módulo pero no puede crear documentos. "
            "Su rol es aprobar, no elaborar. Si intentara auto-aprobar, el sistema responde 403. "
            "Esto es Segregación de Funciones que un auditor externo puede verificar en segundos. "
            "El ciclo completo elaborar→aprobar está certificado a nivel API y listo para UI de workflow."
        ),
        "benefit": "Cumplimiento SoD sin procesos paralelos en email.",
        "faq": [
            ("¿QM puede editar borradores?", "No crea; puede aprobar/rechazar vía workflow."),
            ("¿Audit trail?", "Cada login, creación y decisión queda registrada."),
        ],
        "warnings": "No improvisar aprobación en UI si no hay botón; explicar API certificada.",
        "checklist": ["Sin formulario create", "Listado documentos visible"],
    },
    {
        "id": 8,
        "title": "Gestión de auditorías",
        "duration": "6 min",
        "role": "Auditor",
        "user": "auditor@alimentos-premium.test",
        "screen": "Audit Management",
        "route": "#/audits",
        "menu": "Operations → Audit Management",
        "actions": [
            "Logout QM. Login Auditor.",
            "Ir a Audit Management (#/audits).",
            "Completar Action Center: Code AUD-INT-2026-01, Name Auditoría interna BPM Planta.",
            "Submit formulario.",
            "Mostrar programa/plan/auditoría creados en backend.",
        ],
        "buttons": "#module-action-form submit",
        "data": "Code: AUD-INT-2026-01 · Name: Auditoría interna BPM Planta Panamá",
        "result": "Programa de auditoría registrado con trazabilidad.",
        "script": (
            "El Auditor planifica auditorías internas ISO y HACCP desde un solo lugar. "
            "Programa, plan y auditoría quedan vinculados. En la operación completa —certificada— "
            "se registran hallazgos, evidencias fotográficas y no conformidades que alimentan CAPA automáticamente."
        ),
        "benefit": "Auditorías digitales con evidencia centralizada, no carpetas.",
        "faq": [
            ("¿Checklists personalizados?", "Sí, vía API de checklists por programa."),
            ("¿Auditor externo?", "Rol Auditor con acceso solo lectura a evidencias autorizadas."),
        ],
        "warnings": "Auditor no debe ver formulario CAPA de gestión.",
        "checklist": ["Auditoría creada", "Toast success"],
    },
    {
        "id": 9,
        "title": "Hallazgos y no conformidades",
        "duration": "4 min",
        "role": "Auditor",
        "user": "auditor@alimentos-premium.test",
        "screen": "Audit Management + narrativa",
        "route": "#/audits",
        "menu": "Operations → Audit Management",
        "actions": [
            "Señalar auditoría recién creada en tabla.",
            "Explicar flujo API: start → finding → evidence → non-conformity.",
            "Conectar hallazgo con CAPA del siguiente bloque.",
            "Mostrar Audit Trail (#/audit-trail) brevemente si hay tiempo.",
        ],
        "buttons": "—",
        "data": "Hallazgo ejemplo: Desviación en registro de temperatura CCP",
        "result": "Cliente ve cadena auditoría → acción correctiva.",
        "script": (
            "Cuando el auditor registra un hallazgo —por ejemplo, una desviación en registro de temperatura "
            "de un punto crítico HACCP— el sistema genera la trazabilidad completa: quién, cuándo, qué área, "
            "qué evidencia. Ese hallazgo puede originar una CAPA sin perder contexto en emails."
        ),
        "benefit": "Puente automático auditoría-acción correctiva.",
        "faq": [
            ("¿Fotos como evidencia?", "Sí, vía módulo Storage + adjuntos API."),
            ("¿Clasificación NC mayor/menor?", "Configurable en workflow de hallazgos."),
        ],
        "warnings": "Hallazgo detallado es API; narrar con confianza sin inventar botones.",
        "checklist": ["Conexión auditoría→CAPA explicada"],
    },
    {
        "id": 10,
        "title": "CAPA — Acciones correctivas",
        "duration": "7 min",
        "role": "CAPA Manager",
        "user": "capa@alimentos-premium.test",
        "screen": "CAPA",
        "route": "#/capa",
        "menu": "Operations → CAPA",
        "actions": [
            "Logout Auditor. Login CAPA Manager.",
            "Ir a CAPA (#/capa).",
            "Crear CAPA: Code CAPA-TEMP-001, Name Desviación control temperatura CCP.",
            "Submit.",
            "Explicar ciclo certificado: classify → 5-Why → Ishikawa → corrective → effectiveness → QM closure.",
        ],
        "buttons": "#module-action-form submit",
        "data": "Code: CAPA-TEMP-001 · Name: Desviación control temperatura CCP",
        "result": "CAPA originada y lista para análisis de causa raíz.",
        "script": (
            "La CAPA nace del hallazgo de auditoría. El CAPA Manager clasifica, aplica 5-Why e Ishikawa "
            "para causa raíz, define acciones correctivas y verifica eficacia. El Quality Manager cierra "
            "—nunca el mismo que la originó— cumpliendo ISO 9001 cláusula 10.2."
        ),
        "benefit": "CAPA con metodología estructurada, no planillas sueltas.",
        "faq": [
            ("¿5-Why integrado?", "Sí, certificado en customer journey API."),
            ("¿Plazos y escalamiento?", "Workflow con escalate-overdue certificado."),
        ],
        "warnings": "CAPA Manager no puede aprobar cierre (SoD).",
        "checklist": ["CAPA creada", "Ciclo 5-Why mencionado"],
    },
    {
        "id": 11,
        "title": "Gestión de riesgos",
        "duration": "6 min",
        "role": "Risk Manager",
        "user": "risk@alimentos-premium.test",
        "screen": "Risk Management",
        "route": "#/risks",
        "menu": "Operations → Risk Management",
        "actions": [
            "Logout CAPA Manager. Login Risk Manager.",
            "Ir a Risk Management (#/risks).",
            "Crear riesgo: Code RISK-CCP-01, Name Falla monitoreo temperatura CCP.",
            "Submit.",
            "Mencionar heat map en Executive Dashboard.",
        ],
        "buttons": "#module-action-form submit",
        "data": "Code: RISK-CCP-01 · Name: Falla monitoreo temperatura CCP",
        "result": "Riesgo en registro con evaluación inherente/residual (API).",
        "script": (
            "Paralelo a CAPA, gestionamos riesgos operativos y de inocuidad. El Risk Manager registra "
            "el riesgo de falla en monitoreo de temperatura, evalúa inherente y residual, define tratamientos. "
            "La gerencia ve el heat map en el dashboard ejecutivo."
        ),
        "benefit": "Risk-based thinking ISO 9001:2015 integrado al día a día.",
        "faq": [
            ("¿Matriz 5x5?", "Configurable por categoría y matriz de riesgo."),
            ("¿Vinculo riesgo-CAPA?", "Sí, referencias cruzadas en módulos."),
        ],
        "warnings": "—",
        "checklist": ["Riesgo creado", "Heat map mencionado"],
    },
    {
        "id": 12,
        "title": "Indicadores de calidad",
        "duration": "5 min",
        "role": "Indicators Manager",
        "user": "indicators@alimentos-premium.test",
        "screen": "Quality Indicators",
        "route": "#/indicators",
        "menu": "Operations → Quality Indicators",
        "actions": [
            "Logout Risk Manager. Login Indicators Manager.",
            "Ir a Quality Indicators (#/indicators).",
            "Crear indicador: Code KPI-NC-Rate, Name Tasa NC por lote.",
            "Submit.",
            "Explicar metas, umbrales y mediciones periódicas.",
        ],
        "buttons": "#module-action-form submit",
        "data": "Code: KPI-NC-Rate · Name: Tasa de no conformidades por lote",
        "result": "KPI registrado con target y threshold.",
        "script": (
            "Los indicadores dejan de calcularse en Excel el viernes. Definimos fórmula, meta, umbrales "
            "y mediciones periódicas. Cuando la tasa de NC por lote supera umbral, la gerencia recibe alerta."
        ),
        "benefit": "KPIs confiables para comité de calidad y auditorías.",
        "faq": [
            ("¿Importamos datos MES?", "Vía API/mediciones manuales; conectores en roadmap."),
            ("¿Gráficos de tendencia?", "Dashboard de indicadores certificado."),
        ],
        "warnings": "—",
        "checklist": ["Indicador creado"],
    },
    {
        "id": 13,
        "title": "Centro de reportes",
        "duration": "6 min",
        "role": "Reporting Manager",
        "user": "reporting@alimentos-premium.test",
        "screen": "Report Center",
        "route": "#/reports",
        "menu": "Command Center → Report Center",
        "actions": [
            "Logout Indicators Manager. Login Reporting Manager.",
            "Ir a Report Center (#/reports).",
            "Clic botón #seed-reports (semillar reportes estándar).",
            "Esperar toast de éxito.",
            "Clic #execute-report si habilitado.",
            "Mencionar exportación PDF/Excel.",
        ],
        "buttons": "#seed-reports · #execute-report",
        "data": "—",
        "result": "Reportes ejecutivos generados en segundos.",
        "script": (
            "El Reporting Manager semilla el catálogo de reportes estándar — CAPA abiertas, riesgos críticos, "
            "estado documental — y ejecuta en un clic. Lo que antes tomaba días de consolidación en Excel "
            "ahora es un botón con trazabilidad de quién lo generó."
        ),
        "benefit": "Reporting ejecutivo self-service para gerencia y auditor externo.",
        "faq": [
            ("¿Reportes personalizados?", "Sí, templates y parámetros configurables."),
            ("¿Programación automática?", "Schedules certificados vía API."),
        ],
        "warnings": "Seed puede tardar 3-5 seg; mantener conversación mientras carga.",
        "checklist": ["Seed OK", "Execute probado"],
    },
    {
        "id": 14,
        "title": "Storage — Evidencias centralizadas",
        "duration": "5 min",
        "role": "Storage Administrator",
        "user": "storage@alimentos-premium.test",
        "screen": "Configuration (Storage)",
        "route": "#/configuration",
        "menu": "Enterprise → Configuration",
        "actions": [
            "Logout Reporting Manager. Login Storage Administrator.",
            "Ir a Configuration (#/configuration).",
            "Mostrar panel Storage Providers.",
            "Clic #create-storage-provider.",
            "Confirmar que NO aparece #create-email-provider (SoD).",
        ],
        "buttons": "#create-storage-provider",
        "data": "Provider local/Azure/S3 según ambiente",
        "result": "Proveedor de almacenamiento configurado.",
        "script": (
            "Toda evidencia — fotos de auditoría, PDFs, registros HACCP— vive en storage gobernado. "
            "El Storage Administrator configura proveedores Azure, S3 o local. Separado del administrador "
            "de correo por Segregación de Funciones."
        ),
        "benefit": "Un repositorio único de evidencias con permisos y auditoría.",
        "faq": [
            ("¿Tamaño máximo archivo?", "Definido por plan de licencia y proveedor."),
            ("¿Cifrado?", "En tránsito HTTPS; en reposo según proveedor cloud."),
        ],
        "warnings": "Producción requiere credenciales cloud reales (PENDING third-party).",
        "checklist": ["Storage provider creado", "Botón email ausente"],
    },
    {
        "id": 15,
        "title": "Notificaciones automáticas",
        "duration": "5 min",
        "role": "Notification Administrator",
        "user": "notifications@alimentos-premium.test",
        "screen": "Configuration (Email)",
        "route": "#/configuration",
        "menu": "Enterprise → Configuration",
        "actions": [
            "Logout Storage Admin. Login Notification Administrator.",
            "Ir a Configuration.",
            "Clic #create-email-provider.",
            "Confirmar que NO aparece #create-storage-provider (SoD).",
            "Explicar templates y alertas por evento.",
        ],
        "buttons": "#create-email-provider",
        "data": "SMTP dev: localhost:1025 · Producción: SendGrid/M365",
        "result": "Canal de notificaciones operativo.",
        "script": (
            "Cuando una CAPA vence, un documento requiere aprobación o un indicador supera umbral, "
            "el responsable recibe notificación automática. SMTP configurable por tenant; en producción "
            "conectamos Microsoft 365 o SendGrid."
        ),
        "benefit": "Proactividad: el sistema avisa, no depende de memoria humana.",
        "faq": [
            ("¿Microsoft 365?", "Sí, vía SMTP relay o Graph API (configuración producción)."),
            ("¿Plantillas personalizadas?", "Notification templates por tenant certificados."),
        ],
        "warnings": "Demo local usa SMTP dev; no llegará email real sin infra.",
        "checklist": ["Email provider creado", "SoD verificado"],
    },
    {
        "id": 16,
        "title": "Viewer — Transparencia sin riesgo",
        "duration": "4 min",
        "role": "Viewer",
        "user": "viewer@alimentos-premium.test",
        "screen": "Módulos operativos (solo lectura)",
        "route": "#/documents",
        "menu": "Operations → varios módulos",
        "actions": [
            "Logout Notification Admin. Login Viewer.",
            "Recorrer documents, capa, risks, indicators.",
            "Confirmar ausencia de #module-action-form en cada módulo.",
            "Intentar narrar: API devolvería 403 si intentara crear.",
        ],
        "buttons": "—",
        "data": "—",
        "result": "Visibilidad total sin permiso de modificación.",
        "script": (
            "Gerencia, consultores y auditores externos pueden tener rol Viewer: ven documentos, CAPA, "
            "riesgos e indicadores sin poder alterar nada. Transparencia total con riesgo cero de "
            "modificación accidental."
        ),
        "benefit": "Democratizar información sin comprometer integridad.",
        "faq": [
            ("¿Viewer ve datos sensibles?", "Solo módulos con permiso READ explícito."),
            ("¿Exportar?", "Solo si tiene permiso REPORT.READ."),
        ],
        "warnings": "—",
        "checklist": ["3+ módulos sin formulario create"],
    },
    {
        "id": 17,
        "title": "Dashboard ejecutivo",
        "duration": "6 min",
        "role": "Tenant Administrator o Quality Manager",
        "user": "tenantadmin@alimentos-premium.test",
        "screen": "Executive Dashboard",
        "route": "#/dashboard",
        "menu": "Command Center → Executive Dashboard",
        "actions": [
            "Login Tenant Administrator (o QM).",
            "Ir a Executive Dashboard (#/dashboard).",
            "Mostrar métricas: CAPA, riesgos, indicadores, documentos.",
            "Señalar heat map de riesgos si visible.",
            "Recorrer tiles de módulos (workspace).",
        ],
        "buttons": "—",
        "data": "—",
        "result": "Visión 360° del cumplimiento para C-level.",
        "script": (
            "Este es el tablero que el CEO y el Director de Calidad abren cada lunes. "
            "CAPA abiertas, riesgos críticos, indicadores fuera de meta, documentos por vencer — "
            "todo en una sola pantalla alimentada por datos reales del tenant, no PowerPoint."
        ),
        "benefit": "Governance visible: cumplimiento como KPI de negocio.",
        "faq": [
            ("¿Personalizable?", "Dashboard bindings por rol en roadmap."),
            ("¿Tiempo real?", "Datos actualizados al refrescar; near-real-time con integraciones."),
        ],
        "warnings": "—",
        "checklist": ["Dashboard cargado", "Métricas visibles"],
    },
    {
        "id": 18,
        "title": "Resumen de valor",
        "duration": "5 min",
        "role": "Presentador",
        "user": "—",
        "screen": "Conversación / guía HTML bloque 18",
        "route": "—",
        "menu": "—",
        "actions": [
            "Recapitular 8 dolores iniciales vs soluciones mostradas.",
            "Matriz valor: antes/después.",
            "Mencionar certificación Enterprise completada.",
        ],
        "buttons": "—",
        "data": "—",
        "result": "Cliente articula ROI en sus propias palabras.",
        "script": (
            "Recapitulemos. Ustedes llegaron con Excel, auditorías manuales y CAPA sin seguimiento. "
            "Hoy vieron: tenant Enterprise en minutos, 12 roles con segregación de funciones, "
            "documentos controlados, auditorías trazables, CAPA con 5-Why, riesgos con heat map, "
            "KPIs automáticos, reportes en un clic y dashboards para gerencia. "
            "Compliance 360 no es un módulo: es su sistema nervioso de cumplimiento."
        ),
        "benefit": "Cierre lógico antes de comercial.",
        "faq": [],
        "warnings": "No introducir features nuevas aquí.",
        "checklist": ["8 dolores mapeados a soluciones"],
    },
    {
        "id": 19,
        "title": "Preguntas frecuentes",
        "duration": "8 min",
        "role": "Presentador",
        "user": "—",
        "screen": "Guía HTML → FAQ global",
        "route": "—",
        "menu": "—",
        "actions": [
            "Abrir sección FAQ en guía HTML.",
            "Responder preguntas del cliente usando respuestas preparadas.",
        ],
        "buttons": "—",
        "data": "—",
        "result": "Objeciones comunes resueltas.",
        "script": (
            "Abramos espacio para preguntas. Las más comunes las anticipo: integración, tiempos, "
            "seguridad, costos de licencia y qué requiere configuración de su lado."
        ),
        "benefit": "Confianza por transparencia.",
        "faq": [
            ("¿Multi-plantas?", "Un tenant con empresas/unidades; múltiples tenants para separación legal."),
            ("¿Migración documentos?", "Servicio de implementación con carga masiva vía API/storage."),
            ("¿SLA y soporte?", "Según contrato Enterprise; Support Operator break-glass auditado."),
            ("¿Validación 21 CFR Part 11?", "Audit trail append-only, RBAC, MFA; validación formal en proyecto."),
        ],
        "warnings": "Si no sabe respuesta, comprometer follow-up por escrito.",
        "checklist": ["FAQ consultada"],
    },
    {
        "id": 20,
        "title": "Cierre comercial",
        "duration": "5 min",
        "role": "Presentador",
        "user": "—",
        "screen": "Conversación",
        "route": "—",
        "menu": "—",
        "actions": [
            "Proponer piloto 90 días con Alimentos Premium como tenant real.",
            "Entregar playbook y acceso demo.",
            "Definir próximos pasos: propuesta, implementación, SSO, storage prod.",
            "Agradecer y acordar fecha de follow-up.",
        ],
        "buttons": "—",
        "data": "—",
        "result": "Compromiso de siguiente paso con fecha.",
        "script": (
            "Les propongo tres pasos concretos: uno, workshop de implementación de 2 días con su equipo "
            "de calidad; dos, tenant piloto con sus documentos BPM reales; tres, conexión SSO y storage "
            "productivo en 4 semanas. ¿Agendamos la sesión de kick-off la próxima semana?"
        ),
        "benefit": "Conversión: de demo a proyecto.",
        "faq": [],
        "warnings": "No presionar; adaptar propuesta al tamaño del cliente.",
        "checklist": ["Next step acordado", "Contactos intercambiados", "Follow-up en calendario"],
    },
]

GLOBAL_FAQ = [
    ("¿Compliance 360 compite con SAP/Dynamics?", "Se complementa: gobierna cumplimiento ISO/HACCP donde ERP es transaccional."),
    ("¿Datos en Panamá?", "Hosting configurable; residencia de datos según contrato."),
    ("¿Mobile?", "Responsive web; app nativa en roadmap."),
    ("¿Academy incluida?", "Sí, certificación por rol para usuarios finales."),
    ("¿Qué queda pendiente third-party?", "SMTP producción, cloud storage, SSO corporativo, firma digital, IA."),
]

PRE_DEMO_CHECKLIST = [
    "App corriendo en http://localhost:5272 (o URL acordada)",
    "PostgreSQL activo y tenant Alimentos Premium provisionado",
    "Credenciales verificadas (ver DEMO_README.md)",
    "Navegador Chrome en pantalla compartida",
    "Guía HTML abierta en segunda pestaña",
    "Notificaciones del SO silenciadas",
    "Datos demo precargados (documentos, CAPA existentes OK)",
    "Proyector resolución 1920x1080 recomendada",
]

DURING_DEMO_CHECKLIST = [
    "Anunciar rol antes de cada login",
    "Logout visible entre roles",
    "No mostrar contraseñas en pantalla",
    "Usar guía HTML para script",
    "Marcar checklist por bloque",
    "Si algo falla, narrar valor mientras recupera",
]

POST_DEMO_CHECKLIST = [
    "Enviar resumen ejecutivo por email",
    "Compartir playbook Word",
    "Registrar preguntas pendientes",
    "Agendar follow-up con fecha",
    "Actualizar CRM/oportunidad",
]

COMMERCIAL_TIPS = [
    "Anclar cada módulo a un dolor específico de Alimentos Premium.",
    "Usar números del dashboard, no adjetivos vacíos.",
    "Invitar al Director de Calidad a operar un bloque (Document Controller).",
    "Mencionar certificación Enterprise 29/29 E2E + 23/23 journey.",
    "Ser transparente sobre UI depth vs API depth: es fortaleza Enterprise.",
]

# E2E flow metadata per block — artifact chain for functional demo
E2E_FLOW = {
    1: {"module": "—", "flowIn": "Dolor del cliente (Excel, CAPA manual)", "flowOut": "Buy-in gerencia", "artifact": "—", "verify": "Cliente asiente con dolores"},
    2: {"module": "SuperAdmin Platform", "flowIn": "—", "flowOut": "Plataforma multi-tenant visible", "artifact": "Executive dashboard", "verify": "Contador tenants activos"},
    3: {"module": "SuperAdmin → Tenants", "flowIn": "Plataforma SaaS", "flowOut": "Tenant Alimentos Premium Active", "artifact": "Tenant lifecycle", "verify": "Tenant en listado Active"},
    4: {"module": "Tenant Administration", "flowIn": "Tenant provisionado", "flowOut": "Perfil + branding tenant", "artifact": "TAC hero + tabs", "verify": "General y Branding OK"},
    5: {"module": "TAC → RBAC", "flowIn": "Tenant configurado", "flowOut": "12 roles asignados", "artifact": "Matriz usuarios/roles", "verify": "Usuarios demo visibles"},
    6: {"module": "Document Management", "flowIn": "RBAC activo", "flowOut": "Documento BPM-LIM-001", "artifact": "Controlled document", "verify": "Toast + fila en tabla"},
    7: {"module": "Document Management (SoD)", "flowIn": "BPM-LIM-001 creado", "flowOut": "SoD elaborador≠aprobador", "artifact": "Sin formulario create", "verify": "#module-action-form ausente"},
    8: {"module": "Audit Management", "flowIn": "Documentos controlados", "flowOut": "AUD-INT-2026-01", "artifact": "Audit program", "verify": "Toast + auditoría en tabla"},
    9: {"module": "Audit → Findings", "flowIn": "AUD-INT-2026-01", "flowOut": "Hallazgo temperatura CCP", "artifact": "Finding (API)", "verify": "Narrativa hallazgo→CAPA"},
    10: {"module": "CAPA", "flowIn": "Hallazgo auditoría", "flowOut": "CAPA-TEMP-001", "artifact": "Corrective action", "verify": "CAPA en listado"},
    11: {"module": "Risk Management", "flowIn": "CAPA en curso", "flowOut": "RISK-CCP-01", "artifact": "Risk register", "verify": "Riesgo en matriz"},
    12: {"module": "Quality Indicators", "flowIn": "Riesgos registrados", "flowOut": "KPI-NC-Rate", "artifact": "Quality KPI", "verify": "Indicador con target"},
    13: {"module": "Report Center", "flowIn": "KPIs + CAPA + riesgos", "flowOut": "Reportes ejecutivos", "artifact": "Seeded reports", "verify": "Seed + execute OK"},
    14: {"module": "Configuration → Storage", "flowIn": "Datos operativos", "flowOut": "Storage provider", "artifact": "File storage SoD", "verify": "create-storage OK, no email btn"},
    15: {"module": "Configuration → Email", "flowIn": "Storage configurado", "flowOut": "Email provider", "artifact": "Notification SoD", "verify": "create-email OK, no storage btn"},
    16: {"module": "Multi-module read-only", "flowIn": "Infra lista", "flowOut": "Visibilidad sin edición", "artifact": "Viewer access", "verify": "3+ módulos sin create"},
    17: {"module": "Executive Dashboard", "flowIn": "Todos los módulos", "flowOut": "Visión C-level", "artifact": "Dashboard metrics", "verify": "Métricas + heat map"},
    18: {"module": "—", "flowIn": "Demo completa", "flowOut": "ROI articulado", "artifact": "Valor resumido", "verify": "8 dolores mapeados"},
    19: {"module": "FAQ", "flowIn": "Preguntas cliente", "flowOut": "Objeciones resueltas", "artifact": "Confianza", "verify": "FAQ consultada"},
    20: {"module": "—", "flowIn": "Interés confirmado", "flowOut": "Next step acordado", "artifact": "Propuesta piloto", "verify": "Follow-up en calendario"},
}

# Narrativa completa inicio → fin para guías interactivas
JOURNEY_NARRATIVE = {
    "title": "Customer Journey E2E — Alimentos Premium Panamá S.A.",
    "subtitle": "Historia de negocio en 20 bloques · 60–90 min · 12 roles",
    "artifactChain": [
        "Dolor cliente", "Plataforma SaaS", "Tenant Active", "RBAC 12 roles",
        "BPM-LIM-001", "SoD QM", "AUD-INT-2026-01", "Hallazgo CCP",
        "CAPA-TEMP-001", "RISK-CCP-01", "KPI-NC-Rate", "Reportes ejecutivos",
        "Storage SoD", "Email SoD", "Viewer RO", "Dashboard C-level", "ROI", "FAQ", "Piloto",
    ],
    "phases": [
        {
            "id": "intro", "order": 1, "label": "Apertura comercial", "blocks": [1],
            "duration": "5 min", "role": "Presentador", "app": False,
            "goal": "Conectar dolores del cliente con la transformación Enterprise.",
            "story": "Sin abrir la app. Conversación con gerencia sobre Excel, auditorías manuales y CAPA sin trazabilidad.",
            "output": "Buy-in: el cliente reconoce sus dolores en la narrativa.",
        },
        {
            "id": "platform", "order": 2, "label": "Gobierno plataforma SaaS", "blocks": [2, 3],
            "duration": "10 min", "role": "Platform Administrator", "app": True,
            "goal": "Demostrar arquitectura multi-tenant y onboarding de clientes.",
            "story": "Login platform → SuperAdmin Executive → tab Tenants → Alimentos Premium Active.",
            "output": "Confianza Enterprise: un producto, muchos clientes aislados.",
        },
        {
            "id": "tenant", "order": 3, "label": "Tenant & RBAC", "blocks": [4, 5],
            "duration": "11 min", "role": "Platform Admin → Tenant Admin", "app": True,
            "goal": "Perfil del tenant y matriz de 12 roles con segregación de funciones.",
            "story": "TAC General/Branding → usuarios demo → explicar SoD por rol.",
            "output": "Autonomía del cliente sin depender del vendor.",
        },
        {
            "id": "docs", "order": 4, "label": "Control documental + SoD", "blocks": [6, 7],
            "duration": "10 min", "role": "Doc Controller → Quality Manager", "app": True,
            "goal": "Crear BPM-LIM-001 y demostrar que el aprobador NO elabora.",
            "story": "Document Controller crea documento → QM solo ve lista sin #module-action-form.",
            "output": "Artefacto BPM-LIM-001 + SoD auditable ISO.",
        },
        {
            "id": "audit", "order": 5, "label": "Auditoría & hallazgos", "blocks": [8, 9],
            "duration": "9 min", "role": "Auditor", "app": True,
            "goal": "Programa AUD-INT-2026-01 y hallazgo temperatura CCP.",
            "story": "Crear auditoría → narrar hallazgo vinculado a CAPA.",
            "output": "Hilo auditoría → no conformidad → acción correctiva.",
        },
        {
            "id": "capa", "order": 6, "label": "CAPA", "blocks": [10],
            "duration": "6 min", "role": "CAPA Manager", "app": True,
            "goal": "Registrar CAPA-TEMP-001 por desviación de temperatura.",
            "story": "Formulario CAPA → listado → beneficio ciclo 5-Why/Ishikawa.",
            "output": "CAPA-TEMP-001 en registro.",
        },
        {
            "id": "riskkpi", "order": 7, "label": "Riesgos & KPIs", "blocks": [11, 12],
            "duration": "10 min", "role": "Risk Manager → KPI Manager", "app": True,
            "goal": "RISK-CCP-01 en matriz + KPI-NC-Rate con meta.",
            "story": "Riesgo operativo CCP → indicador de calidad con target.",
            "output": "Matriz viva + KPI para gerencia.",
        },
        {
            "id": "reports", "order": 8, "label": "Reportes ejecutivos", "blocks": [13],
            "duration": "5 min", "role": "Reporting Manager", "app": True,
            "goal": "Seed + ejecutar reportes consolidados.",
            "story": "#seed-reports → #execute-report → narrar velocidad vs Excel.",
            "output": "Reportes PDF/Excel en minutos.",
        },
        {
            "id": "infra", "order": 9, "label": "Infraestructura SoD", "blocks": [14, 15, 16],
            "duration": "12 min", "role": "Storage → Notif → Viewer", "app": True,
            "goal": "Storage≠Email (SoD) + Viewer solo lectura transversal.",
            "story": "Storage Admin solo storage · Notif Admin solo email · Viewer sin create.",
            "output": "Evidencias centralizadas + alertas + transparencia.",
        },
        {
            "id": "close", "order": 10, "label": "Dashboard & cierre", "blocks": [17, 18, 19, 20],
            "duration": "17 min", "role": "Tenant Admin → Presentador", "app": True,
            "goal": "Visión C-level, ROI, FAQ y next step piloto.",
            "story": "Dashboard métricas → 8 dolores resueltos → objeciones → propuesta piloto.",
            "output": "Follow-up en calendario + propuesta comercial.",
        },
    ],
    "setup": [
        {"step": 1, "title": "Abrir esta guía", "detail": "Doble clic en el HTML — funciona offline, sin instalar nada."},
        {"step": 2, "title": "Ensayar el flujo", "detail": "Use «Iniciar recorrido» o avance bloque a bloque con →."},
        {"step": 3, "title": "Demo en vivo (opcional)", "detail": "Levante la app (dotnet run) en otra pestaña y replique cada bloque en localhost:5272."},
    ],
    "dualScreen": {
        "guide": "Guía HTML (esta) — script, credenciales, acciones, mock visual.",
        "app": "Compliance 360 real — http://localhost:5272 cuando presente al cliente.",
    },
}

ROLE_SHORT = {
    "Platform Administrator": "Plat.Admin",
    "Tenant Administrator": "Tenant Admin",
    "Document Controller": "Doc Control",
    "Quality Manager": "Qual.Mgr",
    "Auditor": "Auditor",
    "CAPA Manager": "CAPA Mgr",
    "Risk Manager": "Risk Mgr",
    "Indicators Manager": "KPI Mgr",
    "Reporting Manager": "Reports",
    "Storage Administrator": "Storage",
    "Notification Administrator": "Notif.",
    "Viewer": "Viewer",
    "Presentador": "—",
}

DEFAULT_RECOVERY = "Narrar el beneficio de negocio del bloque. Refresh (F5). Re-login con tab Login & Rol. Si persiste, avanzar al siguiente bloque."

BLOCK_RECOVERY = {
    2: "App no responde: dotnet run --project src/Compliance360.Web. Verificar puerto 5272.",
    4: "Platform Admin en TAC: usar tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871. Logout antes de Tenant Admin.",
    6: "Sin formulario create: logout → login doccontrol@alimentos-premium.test. Verificar rol Document Controller.",
    7: "Si QM ve formulario create: narrar SoD — elaborador≠aprobador. API certificada.",
    13: "Seed reports tarda 3-5s — mantener conversación. Si falla, narrar catálogo API.",
    14: "Storage Admin NO debe ver #create-email-provider — es SoD.",
    15: "Notification Admin NO debe ver #create-storage-provider — es SoD.",
}

BLOCK_UI_FOCUS = {
    2: ["Login page", "Tenant ID field", "Tab Executive", "Tab Tenants", "Tenant counter"],
    6: ["#/documents", "#module-action-form", "Code: BPM-LIM-001", "Submit", "Toast success"],
    7: ["#/documents", "Lista documentos", "SIN #module-action-form"],
    8: ["#/audits", "#module-action-form", "Code: AUD-INT-2026-01"],
    10: ["#/capa", "Code: CAPA-TEMP-001", "Submit"],
    13: ["#/reports", "#seed-reports", "#execute-report"],
    14: ["#/configuration", "#create-storage-provider"],
    15: ["#/configuration", "#create-email-provider"],
    17: ["#/dashboard", "Metrics tiles", "Risk heat map"],
}

ROLE_COLORS = {
    "Platform Administrator": "#1d4ed8",
    "Tenant Administrator": "#0369a1",
    "Document Controller": "#047857",
    "Quality Manager": "#7c3aed",
    "Auditor": "#b45309",
    "CAPA Manager": "#be185d",
    "Risk Manager": "#c2410c",
    "Indicators Manager": "#4338ca",
    "Reporting Manager": "#0f766e",
    "Storage Administrator": "#374151",
    "Notification Administrator": "#0e7490",
    "Viewer": "#64748b",
    "Presentador": "#5b21b6",
}


def resolve_login_role(role: str) -> str | None:
    if not role or role == "Presentador":
        return None
    if "→" in role:
        return role.split("→")[-1].strip()
    if " o " in role:
        return role.split(" o ")[0].strip()
    return role.strip()


def enrich_blocks(blocks: list) -> list:
    enriched = []
    prev_login: str | None = None
    for block in blocks:
        login_role = resolve_login_role(block["role"])
        role_changed = bool(login_role and login_role != prev_login and block["id"] > 1)
        if login_role:
            prev_login = login_role
        e2e = E2E_FLOW.get(block["id"], {})
        overview = next((r for r in ROLES_OVERVIEW if r[0] == login_role), None)
        mid_switch = None
        if "→" in block["role"] and block["role"] != block["role"].split("→")[-1].strip():
            parts = [p.strip() for p in block["role"].split("→")]
            if len(parts) == 2:
                mid_switch = {"from": parts[0], "to": parts[1]}
        enriched.append({
            **block,
            "loginRole": login_role,
            "roleShort": ROLE_SHORT.get(login_role or block["role"], block["role"][:12]),
            "roleColor": ROLE_COLORS.get(login_role or block["role"], "#64748b"),
            "roleChanged": role_changed,
            "prevLoginRole": enriched[-1]["loginRole"] if role_changed and enriched else None,
            "midBlockSwitch": mid_switch,
            "flowIn": e2e.get("flowIn", "—"),
            "flowOut": e2e.get("flowOut", "—"),
            "artifact": e2e.get("artifact", "—"),
            "module": e2e.get("module", "—"),
            "verify": e2e.get("verify", "—"),
            "roleCan": overview[2] if overview else "",
            "roleCannot": overview[3] if overview else "",
            "uiFocus": BLOCK_UI_FOCUS.get(block["id"], [x for x in [block.get("route"), block.get("buttons")] if x and x != "—"]),
            "recovery": BLOCK_RECOVERY.get(block["id"], DEFAULT_RECOVERY),
            "durationMin": int(str(block.get("duration", "5")).split()[0]) if block.get("duration") else 5,
        })
    return enriched
