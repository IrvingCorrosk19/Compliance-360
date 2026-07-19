# FIELD_DICTIONARY

| Screen | Field | Type | Required | Purpose | Validation | Example | Role |
|--------|-------|------|----------|---------|------------|---------|------|
| login | Correo | email | True | Identifica al usuario en el tenant. | invalid: texto sin @ | ra.spec@cert.local | * |
| login | Contraseña | password | True | Autenticación. | invalid: vacía | (la asignada por TAC) | * |
| regulatory-portfolio | Marca | text | True | Marca comercial del dispositivo. | invalid: (vacío) | DEMO | regulatory-specialist, regulatory-administrator |
| regulatory-portfolio | Nombre regulatorio | text | True | Nombre oficial del producto para expediente y CT/RS. Puede diferir del comercial. | invalid: (vacío) | PRODUCTO DEMO | regulatory-specialist, regulatory-administrator |
| regulatory-portfolio | Código catálogo | code | True | Código único por tenant. Duplicado genera error. | invalid: código ya existente | CAT-123456 | regulatory-specialist, regulatory-administrator |
| regulatory-portfolio | Clase de riesgo | select | True | Clase A, B o C del dispositivo. No es el módulo Risk Management. Influye en Requirement Pack. | invalid: valor fuera de A/B/C | A | regulatory-specialist, regulatory-administrator |
| regulatory-portfolio | País | text | True | Código de país del producto (UI demo usa PA). | invalid: (vacío) | PA | regulatory-specialist |
| regulatory-manufacturers | Nombre legal fabricante | text | True | Razón social del fabricante. | invalid: (vacío) | Acme Medical Co. | regulatory-specialist, regulatory-administrator |
| regulatory-manufacturers | País | text | True | País del fabricante. | invalid: (vacío) | CN | regulatory-specialist, regulatory-administrator |
| regulatory-dossier-detail | Descripción de la observación | textarea | True | Texto de la observación emitida por la autoridad. | invalid: (vacío) | Falta actualización de literatura técnica | regulatory-manager |
| regulatory-dossier-detail | Número CT/RS | text | True | Número emitido por la autoridad. No lo genera Compliance 360. | invalid: (vacío) | MQ-4521-07-26 | regulatory-manager, quality-manager |
| regulatory-dossier-detail | Vencimiento ISO | date | True | Fecha de vencimiento del CT/RS (YYYY-MM-DD). | invalid: 2029/07/13 o fecha anterior a emisión | 2029-07-13 | regulatory-manager, quality-manager |
| regulatory-licenses | Compañía | text | True | Nombre de la empresa titular de la licencia operativa. | invalid: (vacío) | Irving Corro S.A | regulatory-administrator |
| regulatory-licenses | Tipo de licencia | text | True | Tipo/descripción de la licencia operativa. | invalid: (vacío) | Distribución de dispositivos médicos | regulatory-administrator |
| regulatory-dashboard | Opportunity Amount | currency | False | Valor comercial asociado al producto/expediente. No cambia la decisión de autoridad; alimenta priorización y dashboard. | invalid: texto no numérico | 15000.00 | regulatory-specialist, regulatory-manager |
| regulatory-dossier-detail | Fecha máxima de recepción | datetime | False | Límite para recibir documentos del fabricante. Puede generar alertas si se vence en estado Espera docs fábrica. | invalid: fecha mal formada | 2026-08-01T00:00:00Z | regulatory-specialist |
