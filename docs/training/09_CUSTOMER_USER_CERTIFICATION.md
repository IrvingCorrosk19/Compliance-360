# Compliance 360 Academy

## Customer User Certification

## Portada

| Campo | Valor |
| --- | --- |
| Rol | Customer User |
| Nivel | Beginner / External User |
| Duración | 10 horas |
| Objetivo | Formar usuarios cliente para consultar reportes, documentos y estado de cumplimiento autorizado. |
| Prerrequisitos | Conocer el contrato de servicio y alcance de acceso. |
| Ruta de aprendizaje | Fundamentos -> Seguridad -> Módulos -> Operación -> Escenarios -> Evaluación -> Certificación |
| Certificación asociada | Compliance 360 Certified User |
| Estado | Markdown maestro. No generar Word hasta aprobación. |

---

# CAPÍTULO 1 - Introducción al Rol

## ¿Quién es?

El `Customer User` es un perfil formal de Compliance 360 Academy. Su entrenamiento está diseñado para que pueda usar la plataforma sin revisar código fuente, entendiendo módulos, permisos, responsabilidades, riesgos y límites reales del producto.

## ¿Qué responsabilidades tiene?

| Responsabilidad | Dueño | Prioridad | Evidencia esperada |
| --- | --- | --- | --- |
| Consultar reportes | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |
| Revisar evidencias autorizadas | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |
| Solicitar información | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |
| Validar entregables | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |
| Participar en comités | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |

## ¿Qué puede hacer?

- Consultar reportes
- Revisar evidencias autorizadas
- Solicitar información
- Validar entregables
- Participar en comités

## ¿Qué no puede hacer?

- Modificar configuración
- Ver otros tenants
- Aprobar datos internos sin permiso
- Extraer información no autorizada

## Flujo operativo del rol

```mermaid
flowchart TD
    Login[Login + MFA] --> Dashboard[Dashboard Customer User]
    Dashboard --> S1[Revisar dashboard]
    S1 --> S2[Consultar reportes]
    S2 --> S3[Validar entregables]
    S3 --> S4[Registrar dudas]
    S4 --> S5[Descargar evidencia autorizada]
    S5 --> Evidence[Audit Trail y evidencia]
    Evidence --> Outcome[Resultado controlado para CustomerUser]
```

## Matriz de responsabilidades

| Responsabilidad | Dueño | Prioridad | Evidencia esperada |
| --- | --- | --- | --- |
| Consultar reportes | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |
| Revisar evidencias autorizadas | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |
| Solicitar información | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |
| Validar entregables | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |
| Participar en comités | Customer User | Alta | Evidencia en Audit Trail / reporte / registro |

## Matriz RACI

| Proceso | Customer User | Tenant Admin | Quality Manager | Support Engineer | Consultora Admin |
| --- | --- | --- | --- | --- | --- |
| Consultar reporte | R/A | I | I | C | C |
| Revisar documento | R/A | I | I | C | C |
| Ver indicador | R/A | I | I | C | C |
| Validar CAPA | R/A | I | I | C | C |
| Solicitar aclaración | R/A | I | I | C | C |
| Revisar dashboard | R/A | I | I | C | C |

---

# CAPÍTULO 2 - Módulos que utiliza

## Módulos asignados al rol

| Módulo | Para qué sirve | Cuándo lo usa |
| --- | --- | --- |
| Customer Portal | Sirve para customer portal | Se usa cuando el rol necesita operar o consultar esta capacidad |
| Reporting Engine | Sirve para reporting engine | Se usa cuando el rol necesita operar o consultar esta capacidad |
| Dashboard | Sirve para dashboard | Se usa cuando el rol necesita operar o consultar esta capacidad |
| Document Management | Sirve para document management | Se usa cuando el rol necesita operar o consultar esta capacidad |
| Audit Trail | Sirve para audit trail | Se usa cuando el rol necesita operar o consultar esta capacidad |

## Matriz de módulos

| Módulo | Tipo de uso | Frecuencia | Nota de estado |
| --- | --- | --- | --- |
| Customer Portal | Uso principal | Diario/Semanal | Ver estado real en Handbook |
| Reporting Engine | Uso principal | Diario/Semanal | Ver estado real en Handbook |
| Dashboard | Uso principal | Diario/Semanal | Ver estado real en Handbook |
| Document Management | Uso principal | Diario/Semanal | Ver estado real en Handbook |
| Audit Trail | Uso principal | Diario/Semanal | Ver estado real en Handbook |

## Diagrama de responsabilidades

```mermaid
flowchart LR
    R[Customer User] --> A[Accountable en responsabilidades asignadas]
    R --> C[Consultado por soporte/implementacion]
    R --> I[Informado por dashboards/reportes]
    A --> Audit[Audit Trail]
    C --> Evidence[Evidencia y comentarios]
    I --> Decisions[Decisiones y seguimiento]
```

---

# CAPÍTULO 3 - Configuración Inicial

## Objetivo

Preparar el acceso y el entorno de trabajo del rol `Customer User` para operar sin fricción.

## Paso a paso

1. Crear o validar usuario en el tenant correcto.
2. Asignar rol y permisos correspondientes.
3. Activar MFA si el tenant lo requiere.
4. Validar acceso a dashboard.
5. Validar acceso a módulos asignados.
6. Probar operación mínima permitida.
7. Confirmar que Audit Trail registra eventos clave.
8. Documentar restricciones del rol.

## Pantalla por pantalla

| Pantalla | Acción esperada | Resultado |
| --- | --- | --- |
| Login | Ingresar credenciales y completar MFA si aplica | Sesión activa |
| Dashboard | Revisar indicadores y alertas | Prioridades visibles |
| Módulos asignados | Validar acceso según matriz | Acceso autorizado |
| Reportes | Consultar datos según permiso | Reporte visible |
| Audit Trail | Confirmar trazabilidad si aplica | Evento registrado |

## Proceso por proceso

Cada proceso debe ejecutarse con tenant, permiso y evidencia correctos. Si aparece `401`, el usuario debe renovar sesión. Si aparece `403`, debe solicitar ajuste de rol, no intentar rodear el control.

---

# CAPÍTULO 4 - Operación Diaria

## ¿Qué hace al iniciar sesión?

| Tarea | Frecuencia | Resultado esperado |
| --- | --- | --- |
| Revisar dashboard | Diario | Validar resultado en dashboard/audit trail |
| Consultar reportes | Diario | Validar resultado en dashboard/audit trail |
| Validar entregables | Diario | Validar resultado en dashboard/audit trail |
| Registrar dudas | Diario | Validar resultado en dashboard/audit trail |
| Descargar evidencia autorizada | Diario | Validar resultado en dashboard/audit trail |

## ¿Qué revisa?

- Estado general del dashboard.
- Tareas asignadas.
- Alertas relacionadas con sus módulos.
- Reportes o indicadores relevantes.
- Evidencia pendiente o procesos vencidos.

## ¿Qué tareas ejecuta?

- Revisar dashboard
- Consultar reportes
- Validar entregables
- Registrar dudas
- Descargar evidencia autorizada

## ¿Qué indicadores debe monitorear?

| Indicador | Uso | Acción esperada |
| --- | --- | --- |
| Reportes disponibles | Monitorear tendencia | Escalar desviaciones |
| KPIs críticos | Monitorear tendencia | Escalar desviaciones |
| CAPA abiertas | Monitorear tendencia | Escalar desviaciones |
| Entregables vencidos | Monitorear tendencia | Escalar desviaciones |
| Evidencias pendientes | Monitorear tendencia | Escalar desviaciones |

---

# CAPÍTULO 5 - Procesos Paso a Paso

### 5.1 Consultar reporte

**Objetivo:** ejecutar el proceso `Consultar reporte` de forma trazable, segura y alineada con el rol `Customer User`.

**Prerrequisitos:**

- Usuario activo en el tenant correcto.
- Permisos del rol validados antes de iniciar.
- Datos base cargados: documentos, usuarios, módulos o providers según aplique.
- MFA completado si el tenant lo requiere.

**Pasos:**

1. Iniciar sesión en Compliance 360.
2. Confirmar tenant, rol activo y permisos visibles.
3. Abrir el módulo relacionado con `Consultar reporte`.
4. Revisar dashboard, filtros y estado actual antes de modificar información.
5. Crear, actualizar, aprobar, consultar o ejecutar la acción permitida por el rol.
6. Adjuntar evidencia cuando el proceso lo requiera.
7. Validar resultado esperado y registrar observaciones.
8. Revisar que el evento quede trazado en Audit Trail o en el dashboard correspondiente.

**Resultado esperado:** el proceso queda actualizado, con responsable, evidencia, estado visible y trazabilidad.

**Errores comunes:**

- Ejecutar el proceso en el tenant equivocado.
- Usar un rol sin permiso suficiente y recibir `403 Forbidden`.
- Omitir evidencia antes de aprobación/cierre.
- Confundir módulos core operativos con workspaces genéricos.

**Buenas prácticas:**

- Documentar decisiones en comentarios o evidencia.
- Validar datos antes de aprobar.
- Usar reportes para confirmar impacto.
- Escalar a soporte con correlation id cuando exista error técnico.

### 5.2 Revisar documento

**Objetivo:** ejecutar el proceso `Revisar documento` de forma trazable, segura y alineada con el rol `Customer User`.

**Prerrequisitos:**

- Usuario activo en el tenant correcto.
- Permisos del rol validados antes de iniciar.
- Datos base cargados: documentos, usuarios, módulos o providers según aplique.
- MFA completado si el tenant lo requiere.

**Pasos:**

1. Iniciar sesión en Compliance 360.
2. Confirmar tenant, rol activo y permisos visibles.
3. Abrir el módulo relacionado con `Revisar documento`.
4. Revisar dashboard, filtros y estado actual antes de modificar información.
5. Crear, actualizar, aprobar, consultar o ejecutar la acción permitida por el rol.
6. Adjuntar evidencia cuando el proceso lo requiera.
7. Validar resultado esperado y registrar observaciones.
8. Revisar que el evento quede trazado en Audit Trail o en el dashboard correspondiente.

**Resultado esperado:** el proceso queda actualizado, con responsable, evidencia, estado visible y trazabilidad.

**Errores comunes:**

- Ejecutar el proceso en el tenant equivocado.
- Usar un rol sin permiso suficiente y recibir `403 Forbidden`.
- Omitir evidencia antes de aprobación/cierre.
- Confundir módulos core operativos con workspaces genéricos.

**Buenas prácticas:**

- Documentar decisiones en comentarios o evidencia.
- Validar datos antes de aprobar.
- Usar reportes para confirmar impacto.
- Escalar a soporte con correlation id cuando exista error técnico.

### 5.3 Ver indicador

**Objetivo:** ejecutar el proceso `Ver indicador` de forma trazable, segura y alineada con el rol `Customer User`.

**Prerrequisitos:**

- Usuario activo en el tenant correcto.
- Permisos del rol validados antes de iniciar.
- Datos base cargados: documentos, usuarios, módulos o providers según aplique.
- MFA completado si el tenant lo requiere.

**Pasos:**

1. Iniciar sesión en Compliance 360.
2. Confirmar tenant, rol activo y permisos visibles.
3. Abrir el módulo relacionado con `Ver indicador`.
4. Revisar dashboard, filtros y estado actual antes de modificar información.
5. Crear, actualizar, aprobar, consultar o ejecutar la acción permitida por el rol.
6. Adjuntar evidencia cuando el proceso lo requiera.
7. Validar resultado esperado y registrar observaciones.
8. Revisar que el evento quede trazado en Audit Trail o en el dashboard correspondiente.

**Resultado esperado:** el proceso queda actualizado, con responsable, evidencia, estado visible y trazabilidad.

**Errores comunes:**

- Ejecutar el proceso en el tenant equivocado.
- Usar un rol sin permiso suficiente y recibir `403 Forbidden`.
- Omitir evidencia antes de aprobación/cierre.
- Confundir módulos core operativos con workspaces genéricos.

**Buenas prácticas:**

- Documentar decisiones en comentarios o evidencia.
- Validar datos antes de aprobar.
- Usar reportes para confirmar impacto.
- Escalar a soporte con correlation id cuando exista error técnico.

### 5.4 Validar CAPA

**Objetivo:** ejecutar el proceso `Validar CAPA` de forma trazable, segura y alineada con el rol `Customer User`.

**Prerrequisitos:**

- Usuario activo en el tenant correcto.
- Permisos del rol validados antes de iniciar.
- Datos base cargados: documentos, usuarios, módulos o providers según aplique.
- MFA completado si el tenant lo requiere.

**Pasos:**

1. Iniciar sesión en Compliance 360.
2. Confirmar tenant, rol activo y permisos visibles.
3. Abrir el módulo relacionado con `Validar CAPA`.
4. Revisar dashboard, filtros y estado actual antes de modificar información.
5. Crear, actualizar, aprobar, consultar o ejecutar la acción permitida por el rol.
6. Adjuntar evidencia cuando el proceso lo requiera.
7. Validar resultado esperado y registrar observaciones.
8. Revisar que el evento quede trazado en Audit Trail o en el dashboard correspondiente.

**Resultado esperado:** el proceso queda actualizado, con responsable, evidencia, estado visible y trazabilidad.

**Errores comunes:**

- Ejecutar el proceso en el tenant equivocado.
- Usar un rol sin permiso suficiente y recibir `403 Forbidden`.
- Omitir evidencia antes de aprobación/cierre.
- Confundir módulos core operativos con workspaces genéricos.

**Buenas prácticas:**

- Documentar decisiones en comentarios o evidencia.
- Validar datos antes de aprobar.
- Usar reportes para confirmar impacto.
- Escalar a soporte con correlation id cuando exista error técnico.

### 5.5 Solicitar aclaración

**Objetivo:** ejecutar el proceso `Solicitar aclaración` de forma trazable, segura y alineada con el rol `Customer User`.

**Prerrequisitos:**

- Usuario activo en el tenant correcto.
- Permisos del rol validados antes de iniciar.
- Datos base cargados: documentos, usuarios, módulos o providers según aplique.
- MFA completado si el tenant lo requiere.

**Pasos:**

1. Iniciar sesión en Compliance 360.
2. Confirmar tenant, rol activo y permisos visibles.
3. Abrir el módulo relacionado con `Solicitar aclaración`.
4. Revisar dashboard, filtros y estado actual antes de modificar información.
5. Crear, actualizar, aprobar, consultar o ejecutar la acción permitida por el rol.
6. Adjuntar evidencia cuando el proceso lo requiera.
7. Validar resultado esperado y registrar observaciones.
8. Revisar que el evento quede trazado en Audit Trail o en el dashboard correspondiente.

**Resultado esperado:** el proceso queda actualizado, con responsable, evidencia, estado visible y trazabilidad.

**Errores comunes:**

- Ejecutar el proceso en el tenant equivocado.
- Usar un rol sin permiso suficiente y recibir `403 Forbidden`.
- Omitir evidencia antes de aprobación/cierre.
- Confundir módulos core operativos con workspaces genéricos.

**Buenas prácticas:**

- Documentar decisiones en comentarios o evidencia.
- Validar datos antes de aprobar.
- Usar reportes para confirmar impacto.
- Escalar a soporte con correlation id cuando exista error técnico.

### 5.6 Revisar dashboard

**Objetivo:** ejecutar el proceso `Revisar dashboard` de forma trazable, segura y alineada con el rol `Customer User`.

**Prerrequisitos:**

- Usuario activo en el tenant correcto.
- Permisos del rol validados antes de iniciar.
- Datos base cargados: documentos, usuarios, módulos o providers según aplique.
- MFA completado si el tenant lo requiere.

**Pasos:**

1. Iniciar sesión en Compliance 360.
2. Confirmar tenant, rol activo y permisos visibles.
3. Abrir el módulo relacionado con `Revisar dashboard`.
4. Revisar dashboard, filtros y estado actual antes de modificar información.
5. Crear, actualizar, aprobar, consultar o ejecutar la acción permitida por el rol.
6. Adjuntar evidencia cuando el proceso lo requiera.
7. Validar resultado esperado y registrar observaciones.
8. Revisar que el evento quede trazado en Audit Trail o en el dashboard correspondiente.

**Resultado esperado:** el proceso queda actualizado, con responsable, evidencia, estado visible y trazabilidad.

**Errores comunes:**

- Ejecutar el proceso en el tenant equivocado.
- Usar un rol sin permiso suficiente y recibir `403 Forbidden`.
- Omitir evidencia antes de aprobación/cierre.
- Confundir módulos core operativos con workspaces genéricos.

**Buenas prácticas:**

- Documentar decisiones en comentarios o evidencia.
- Validar datos antes de aprobar.
- Usar reportes para confirmar impacto.
- Escalar a soporte con correlation id cuando exista error técnico.

---

# CAPÍTULO 6 - Escenarios Reales

### 6.1 Escenario: Reporte mensual

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `Reporte mensual` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

### 6.2 Escenario: Auditoría de seguimiento

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `Auditoría de seguimiento` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

### 6.3 Escenario: Evidencia solicitada

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `Evidencia solicitada` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

### 6.4 Escenario: KPI fuera de meta

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `KPI fuera de meta` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

### 6.5 Escenario: CAPA crítica

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `CAPA crítica` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

### 6.6 Escenario: Documento vigente

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `Documento vigente` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

### 6.7 Escenario: Comité con consultora

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `Comité con consultora` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

### 6.8 Escenario: Entrega contractual

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `Entrega contractual` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

### 6.9 Escenario: Consulta de riesgos

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `Consulta de riesgos` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

### 6.10 Escenario: Cierre de ciclo

**Contexto empresarial:** el rol `Customer User` enfrenta el caso `Cierre de ciclo` dentro de un tenant productivo o de capacitación.

**Objetivo del escenario:** resolver la situación sin romper trazabilidad, seguridad ni segregación de funciones.

**Acciones esperadas:**

1. Identificar el módulo principal del caso.
2. Revisar permisos y alcance del rol.
3. Consultar registros existentes, dashboard o reportes relacionados.
4. Ejecutar la acción permitida por el rol.
5. Adjuntar evidencia o comentario de soporte.
6. Confirmar estado final y responsables.
7. Escalar si el proceso requiere permisos de otro rol.

**Criterios de éxito:**

- El caso queda resuelto o escalado formalmente.
- No se realizan acciones fuera del permiso del rol.
- Existe evidencia o trazabilidad suficiente para auditoría.
- El estado final es comprensible para negocio, soporte e implementación.

**Riesgo si se opera mal:** pérdida de evidencia, aprobación indebida, error de tenant, incumplimiento de ISO 9001/BPM/HACCP o confusión comercial sobre el estado real del producto.

---

# CAPÍTULO 7 - Mejores Prácticas

## ISO 9001

- Mantener evidencia trazable de cambios, aprobaciones y cierres.
- Usar roles claros para evitar conflictos de interés.
- Medir desempeño con indicadores y revisar tendencias.
- Gestionar no conformidades mediante CAPA con causa raíz y efectividad.

## ISO 22000, HACCP y BPM

- Controlar documentos de inocuidad y BPM como documentos vigentes.
- Vincular proveedores críticos con certificaciones y evaluaciones.
- Registrar riesgos de inocuidad con controles y tratamiento.
- Mantener evidencias de acciones preventivas y correctivas.

## Buenas prácticas SaaS

- No compartir usuarios.
- Activar MFA para roles sensibles.
- Usar permisos mínimos necesarios.
- Validar providers de email/storage antes de producción.
- Escalar errores técnicos con evidencia, hora, tenant y correlation id.

---

# CAPÍTULO 8 - Errores Comunes

| Error | Consecuencia | Prevención |
| --- | --- | --- |
| Operar en tenant incorrecto | Riesgo de privacidad y datos cruzados | Confirmar tenant al iniciar |
| Usar rol con permisos excesivos | Falta de segregación | Revisar matriz RBAC |
| Cerrar sin evidencia | Debilidad ante auditoría | Adjuntar evidencia antes de cierre |
| Ignorar módulos en estado workspace | Promesa comercial incorrecta | Aplicar regla de honestidad |
| No revisar Audit Trail | Falta de trazabilidad | Validar eventos clave |
| No probar providers | Fallas de email/storage en producción | Ejecutar tests de conexión |
| Compartir credenciales | Riesgo de seguridad | Usuario individual por persona |
| Omitir MFA | Mayor exposición de acceso | Activar MFA en roles críticos |
| No documentar decisiones | Soporte y auditoría débiles | Registrar comentarios |
| No escalar a tiempo | SLA incumplido | Clasificar severidad |

---

# CAPÍTULO 9 - Checklist Operativo

## Checklist diario

- Confirmar acceso y tenant correcto.
- Revisar dashboard y tareas asignadas.
- Revisar alertas de módulos asignados.
- Ejecutar procesos prioritarios.
- Escalar bloqueos con evidencia.

## Checklist semanal

- Revisar reportes relevantes.
- Revisar tareas vencidas.
- Validar indicadores del rol.
- Confirmar que los procesos críticos tienen responsables.
- Revisar errores recurrentes.

## Checklist mensual

- Preparar comité o revisión ejecutiva.
- Auditar permisos del rol si aplica.
- Revisar tendencias y desviaciones.
- Confirmar cierre de procesos críticos.
- Documentar oportunidades de mejora.

## Checklist trimestral

- Revisar matriz de responsabilidades.
- Validar capacitación de usuarios.
- Revisar efectividad de controles.
- Actualizar procesos según cambios normativos.
- Preparar evidencia para auditorías.

---

# CAPÍTULO 10 - Evaluación Teórica

**Instrucciones:** responder 50 preguntas de opción múltiple. Puntaje recomendado: 2 puntos por pregunta. Mínimo de aprobación: 80/100.

### Pregunta 1

**Situación:** el rol `Customer User` debe trabajar con `Customer Portal` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Customer Portal.
B. Compartir credenciales para acelerar el trabajo sobre Customer Portal.
C. Omitir Audit Trail porque el proceso Customer Portal es interno.
D. Prometer funcionalidad no implementada para Customer Portal sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 2

**Situación:** el rol `Customer User` debe trabajar con `Reporting Engine` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Reporting Engine.
B. Validar permisos, tenant, evidencia y estado antes de operar Reporting Engine.
C. Omitir Audit Trail porque el proceso Reporting Engine es interno.
D. Prometer funcionalidad no implementada para Reporting Engine sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 3

**Situación:** el rol `Customer User` debe trabajar con `Dashboard` en Compliance 360.

A. Omitir Audit Trail porque el proceso Dashboard es interno.
B. Compartir credenciales para acelerar el trabajo sobre Dashboard.
C. Validar permisos, tenant, evidencia y estado antes de operar Dashboard.
D. Prometer funcionalidad no implementada para Dashboard sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 4

**Situación:** el rol `Customer User` debe trabajar con `Document Management` en Compliance 360.

A. Prometer funcionalidad no implementada para Document Management sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre Document Management.
C. Omitir Audit Trail porque el proceso Document Management es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar Document Management.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 5

**Situación:** el rol `Customer User` debe trabajar con `Audit Trail` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Audit Trail.
B. Compartir credenciales para acelerar el trabajo sobre Audit Trail.
C. Omitir Audit Trail porque el proceso Audit Trail es interno.
D. Prometer funcionalidad no implementada para Audit Trail sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 6

**Situación:** el rol `Customer User` debe trabajar con `Consultar reporte` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Consultar reporte.
B. Validar permisos, tenant, evidencia y estado antes de operar Consultar reporte.
C. Omitir Audit Trail porque el proceso Consultar reporte es interno.
D. Prometer funcionalidad no implementada para Consultar reporte sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 7

**Situación:** el rol `Customer User` debe trabajar con `Revisar documento` en Compliance 360.

A. Omitir Audit Trail porque el proceso Revisar documento es interno.
B. Compartir credenciales para acelerar el trabajo sobre Revisar documento.
C. Validar permisos, tenant, evidencia y estado antes de operar Revisar documento.
D. Prometer funcionalidad no implementada para Revisar documento sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 8

**Situación:** el rol `Customer User` debe trabajar con `Ver indicador` en Compliance 360.

A. Prometer funcionalidad no implementada para Ver indicador sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre Ver indicador.
C. Omitir Audit Trail porque el proceso Ver indicador es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar Ver indicador.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 9

**Situación:** el rol `Customer User` debe trabajar con `Validar CAPA` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Validar CAPA.
B. Compartir credenciales para acelerar el trabajo sobre Validar CAPA.
C. Omitir Audit Trail porque el proceso Validar CAPA es interno.
D. Prometer funcionalidad no implementada para Validar CAPA sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 10

**Situación:** el rol `Customer User` debe trabajar con `Solicitar aclaración` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Solicitar aclaración.
B. Validar permisos, tenant, evidencia y estado antes de operar Solicitar aclaración.
C. Omitir Audit Trail porque el proceso Solicitar aclaración es interno.
D. Prometer funcionalidad no implementada para Solicitar aclaración sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 11

**Situación:** el rol `Customer User` debe trabajar con `Revisar dashboard` en Compliance 360.

A. Omitir Audit Trail porque el proceso Revisar dashboard es interno.
B. Compartir credenciales para acelerar el trabajo sobre Revisar dashboard.
C. Validar permisos, tenant, evidencia y estado antes de operar Revisar dashboard.
D. Prometer funcionalidad no implementada para Revisar dashboard sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 12

**Situación:** el rol `Customer User` debe trabajar con `RBAC` en Compliance 360.

A. Prometer funcionalidad no implementada para RBAC sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre RBAC.
C. Omitir Audit Trail porque el proceso RBAC es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar RBAC.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 13

**Situación:** el rol `Customer User` debe trabajar con `MFA` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar MFA.
B. Compartir credenciales para acelerar el trabajo sobre MFA.
C. Omitir Audit Trail porque el proceso MFA es interno.
D. Prometer funcionalidad no implementada para MFA sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 14

**Situación:** el rol `Customer User` debe trabajar con `Audit Trail` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Audit Trail.
B. Validar permisos, tenant, evidencia y estado antes de operar Audit Trail.
C. Omitir Audit Trail porque el proceso Audit Trail es interno.
D. Prometer funcionalidad no implementada para Audit Trail sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 15

**Situación:** el rol `Customer User` debe trabajar con `Estado real del módulo` en Compliance 360.

A. Omitir Audit Trail porque el proceso Estado real del módulo es interno.
B. Compartir credenciales para acelerar el trabajo sobre Estado real del módulo.
C. Validar permisos, tenant, evidencia y estado antes de operar Estado real del módulo.
D. Prometer funcionalidad no implementada para Estado real del módulo sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 16

**Situación:** el rol `Customer User` debe trabajar con `Buenas prácticas ISO` en Compliance 360.

A. Prometer funcionalidad no implementada para Buenas prácticas ISO sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre Buenas prácticas ISO.
C. Omitir Audit Trail porque el proceso Buenas prácticas ISO es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar Buenas prácticas ISO.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 17

**Situación:** el rol `Customer User` debe trabajar con `Customer Portal` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Customer Portal.
B. Compartir credenciales para acelerar el trabajo sobre Customer Portal.
C. Omitir Audit Trail porque el proceso Customer Portal es interno.
D. Prometer funcionalidad no implementada para Customer Portal sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 18

**Situación:** el rol `Customer User` debe trabajar con `Reporting Engine` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Reporting Engine.
B. Validar permisos, tenant, evidencia y estado antes de operar Reporting Engine.
C. Omitir Audit Trail porque el proceso Reporting Engine es interno.
D. Prometer funcionalidad no implementada para Reporting Engine sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 19

**Situación:** el rol `Customer User` debe trabajar con `Dashboard` en Compliance 360.

A. Omitir Audit Trail porque el proceso Dashboard es interno.
B. Compartir credenciales para acelerar el trabajo sobre Dashboard.
C. Validar permisos, tenant, evidencia y estado antes de operar Dashboard.
D. Prometer funcionalidad no implementada para Dashboard sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 20

**Situación:** el rol `Customer User` debe trabajar con `Document Management` en Compliance 360.

A. Prometer funcionalidad no implementada para Document Management sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre Document Management.
C. Omitir Audit Trail porque el proceso Document Management es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar Document Management.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 21

**Situación:** el rol `Customer User` debe trabajar con `Audit Trail` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Audit Trail.
B. Compartir credenciales para acelerar el trabajo sobre Audit Trail.
C. Omitir Audit Trail porque el proceso Audit Trail es interno.
D. Prometer funcionalidad no implementada para Audit Trail sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 22

**Situación:** el rol `Customer User` debe trabajar con `Consultar reporte` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Consultar reporte.
B. Validar permisos, tenant, evidencia y estado antes de operar Consultar reporte.
C. Omitir Audit Trail porque el proceso Consultar reporte es interno.
D. Prometer funcionalidad no implementada para Consultar reporte sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 23

**Situación:** el rol `Customer User` debe trabajar con `Revisar documento` en Compliance 360.

A. Omitir Audit Trail porque el proceso Revisar documento es interno.
B. Compartir credenciales para acelerar el trabajo sobre Revisar documento.
C. Validar permisos, tenant, evidencia y estado antes de operar Revisar documento.
D. Prometer funcionalidad no implementada para Revisar documento sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 24

**Situación:** el rol `Customer User` debe trabajar con `Ver indicador` en Compliance 360.

A. Prometer funcionalidad no implementada para Ver indicador sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre Ver indicador.
C. Omitir Audit Trail porque el proceso Ver indicador es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar Ver indicador.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 25

**Situación:** el rol `Customer User` debe trabajar con `Validar CAPA` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Validar CAPA.
B. Compartir credenciales para acelerar el trabajo sobre Validar CAPA.
C. Omitir Audit Trail porque el proceso Validar CAPA es interno.
D. Prometer funcionalidad no implementada para Validar CAPA sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 26

**Situación:** el rol `Customer User` debe trabajar con `Solicitar aclaración` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Solicitar aclaración.
B. Validar permisos, tenant, evidencia y estado antes de operar Solicitar aclaración.
C. Omitir Audit Trail porque el proceso Solicitar aclaración es interno.
D. Prometer funcionalidad no implementada para Solicitar aclaración sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 27

**Situación:** el rol `Customer User` debe trabajar con `Revisar dashboard` en Compliance 360.

A. Omitir Audit Trail porque el proceso Revisar dashboard es interno.
B. Compartir credenciales para acelerar el trabajo sobre Revisar dashboard.
C. Validar permisos, tenant, evidencia y estado antes de operar Revisar dashboard.
D. Prometer funcionalidad no implementada para Revisar dashboard sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 28

**Situación:** el rol `Customer User` debe trabajar con `RBAC` en Compliance 360.

A. Prometer funcionalidad no implementada para RBAC sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre RBAC.
C. Omitir Audit Trail porque el proceso RBAC es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar RBAC.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 29

**Situación:** el rol `Customer User` debe trabajar con `MFA` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar MFA.
B. Compartir credenciales para acelerar el trabajo sobre MFA.
C. Omitir Audit Trail porque el proceso MFA es interno.
D. Prometer funcionalidad no implementada para MFA sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 30

**Situación:** el rol `Customer User` debe trabajar con `Audit Trail` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Audit Trail.
B. Validar permisos, tenant, evidencia y estado antes de operar Audit Trail.
C. Omitir Audit Trail porque el proceso Audit Trail es interno.
D. Prometer funcionalidad no implementada para Audit Trail sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 31

**Situación:** el rol `Customer User` debe trabajar con `Estado real del módulo` en Compliance 360.

A. Omitir Audit Trail porque el proceso Estado real del módulo es interno.
B. Compartir credenciales para acelerar el trabajo sobre Estado real del módulo.
C. Validar permisos, tenant, evidencia y estado antes de operar Estado real del módulo.
D. Prometer funcionalidad no implementada para Estado real del módulo sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 32

**Situación:** el rol `Customer User` debe trabajar con `Buenas prácticas ISO` en Compliance 360.

A. Prometer funcionalidad no implementada para Buenas prácticas ISO sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre Buenas prácticas ISO.
C. Omitir Audit Trail porque el proceso Buenas prácticas ISO es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar Buenas prácticas ISO.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 33

**Situación:** el rol `Customer User` debe trabajar con `Customer Portal` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Customer Portal.
B. Compartir credenciales para acelerar el trabajo sobre Customer Portal.
C. Omitir Audit Trail porque el proceso Customer Portal es interno.
D. Prometer funcionalidad no implementada para Customer Portal sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 34

**Situación:** el rol `Customer User` debe trabajar con `Reporting Engine` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Reporting Engine.
B. Validar permisos, tenant, evidencia y estado antes de operar Reporting Engine.
C. Omitir Audit Trail porque el proceso Reporting Engine es interno.
D. Prometer funcionalidad no implementada para Reporting Engine sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 35

**Situación:** el rol `Customer User` debe trabajar con `Dashboard` en Compliance 360.

A. Omitir Audit Trail porque el proceso Dashboard es interno.
B. Compartir credenciales para acelerar el trabajo sobre Dashboard.
C. Validar permisos, tenant, evidencia y estado antes de operar Dashboard.
D. Prometer funcionalidad no implementada para Dashboard sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 36

**Situación:** el rol `Customer User` debe trabajar con `Document Management` en Compliance 360.

A. Prometer funcionalidad no implementada para Document Management sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre Document Management.
C. Omitir Audit Trail porque el proceso Document Management es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar Document Management.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 37

**Situación:** el rol `Customer User` debe trabajar con `Audit Trail` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Audit Trail.
B. Compartir credenciales para acelerar el trabajo sobre Audit Trail.
C. Omitir Audit Trail porque el proceso Audit Trail es interno.
D. Prometer funcionalidad no implementada para Audit Trail sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 38

**Situación:** el rol `Customer User` debe trabajar con `Consultar reporte` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Consultar reporte.
B. Validar permisos, tenant, evidencia y estado antes de operar Consultar reporte.
C. Omitir Audit Trail porque el proceso Consultar reporte es interno.
D. Prometer funcionalidad no implementada para Consultar reporte sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 39

**Situación:** el rol `Customer User` debe trabajar con `Revisar documento` en Compliance 360.

A. Omitir Audit Trail porque el proceso Revisar documento es interno.
B. Compartir credenciales para acelerar el trabajo sobre Revisar documento.
C. Validar permisos, tenant, evidencia y estado antes de operar Revisar documento.
D. Prometer funcionalidad no implementada para Revisar documento sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 40

**Situación:** el rol `Customer User` debe trabajar con `Ver indicador` en Compliance 360.

A. Prometer funcionalidad no implementada para Ver indicador sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre Ver indicador.
C. Omitir Audit Trail porque el proceso Ver indicador es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar Ver indicador.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 41

**Situación:** el rol `Customer User` debe trabajar con `Validar CAPA` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Validar CAPA.
B. Compartir credenciales para acelerar el trabajo sobre Validar CAPA.
C. Omitir Audit Trail porque el proceso Validar CAPA es interno.
D. Prometer funcionalidad no implementada para Validar CAPA sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 42

**Situación:** el rol `Customer User` debe trabajar con `Solicitar aclaración` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Solicitar aclaración.
B. Validar permisos, tenant, evidencia y estado antes de operar Solicitar aclaración.
C. Omitir Audit Trail porque el proceso Solicitar aclaración es interno.
D. Prometer funcionalidad no implementada para Solicitar aclaración sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 43

**Situación:** el rol `Customer User` debe trabajar con `Revisar dashboard` en Compliance 360.

A. Omitir Audit Trail porque el proceso Revisar dashboard es interno.
B. Compartir credenciales para acelerar el trabajo sobre Revisar dashboard.
C. Validar permisos, tenant, evidencia y estado antes de operar Revisar dashboard.
D. Prometer funcionalidad no implementada para Revisar dashboard sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 44

**Situación:** el rol `Customer User` debe trabajar con `RBAC` en Compliance 360.

A. Prometer funcionalidad no implementada para RBAC sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre RBAC.
C. Omitir Audit Trail porque el proceso RBAC es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar RBAC.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 45

**Situación:** el rol `Customer User` debe trabajar con `MFA` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar MFA.
B. Compartir credenciales para acelerar el trabajo sobre MFA.
C. Omitir Audit Trail porque el proceso MFA es interno.
D. Prometer funcionalidad no implementada para MFA sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 46

**Situación:** el rol `Customer User` debe trabajar con `Audit Trail` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Audit Trail.
B. Validar permisos, tenant, evidencia y estado antes de operar Audit Trail.
C. Omitir Audit Trail porque el proceso Audit Trail es interno.
D. Prometer funcionalidad no implementada para Audit Trail sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 47

**Situación:** el rol `Customer User` debe trabajar con `Estado real del módulo` en Compliance 360.

A. Omitir Audit Trail porque el proceso Estado real del módulo es interno.
B. Compartir credenciales para acelerar el trabajo sobre Estado real del módulo.
C. Validar permisos, tenant, evidencia y estado antes de operar Estado real del módulo.
D. Prometer funcionalidad no implementada para Estado real del módulo sin aclaración.

**Respuesta correcta:** C

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 48

**Situación:** el rol `Customer User` debe trabajar con `Buenas prácticas ISO` en Compliance 360.

A. Prometer funcionalidad no implementada para Buenas prácticas ISO sin aclaración.
B. Compartir credenciales para acelerar el trabajo sobre Buenas prácticas ISO.
C. Omitir Audit Trail porque el proceso Buenas prácticas ISO es interno.
D. Validar permisos, tenant, evidencia y estado antes de operar Buenas prácticas ISO.

**Respuesta correcta:** D

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 49

**Situación:** el rol `Customer User` debe trabajar con `Customer Portal` en Compliance 360.

A. Validar permisos, tenant, evidencia y estado antes de operar Customer Portal.
B. Compartir credenciales para acelerar el trabajo sobre Customer Portal.
C. Omitir Audit Trail porque el proceso Customer Portal es interno.
D. Prometer funcionalidad no implementada para Customer Portal sin aclaración.

**Respuesta correcta:** A

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

### Pregunta 50

**Situación:** el rol `Customer User` debe trabajar con `Reporting Engine` en Compliance 360.

A. Compartir credenciales para acelerar el trabajo sobre Reporting Engine.
B. Validar permisos, tenant, evidencia y estado antes de operar Reporting Engine.
C. Omitir Audit Trail porque el proceso Reporting Engine es interno.
D. Prometer funcionalidad no implementada para Reporting Engine sin aclaración.

**Respuesta correcta:** B

**Explicación:** la respuesta correcta protege multitenancy, RBAC, trazabilidad, evidencia y honestidad del producto. Las demás opciones introducen riesgos de seguridad, auditoría, soporte o venta incorrecta.

---

# CAPÍTULO 11 - Evaluación Práctica

### 11.1 Ejercicio práctico: Consultar reporte

**Caso:** ejecutar `Consultar reporte` en un tenant de entrenamiento llamado `Academy Demo Tenant`.

**Tareas dentro de Compliance 360:**

1. Iniciar sesión con usuario de entrenamiento del rol `Customer User`.
2. Navegar al módulo correspondiente.
3. Ejecutar el proceso de punta a punta, respetando permisos.
4. Registrar evidencia o comentario.
5. Consultar dashboard, reporte o audit trail para validar resultado.

**Entregable del estudiante:** captura o descripción del resultado, estado final, evidencia creada y lección aprendida.

**Criterio de aprobación:** proceso completo sin violar permisos, sin usar tenant incorrecto y con trazabilidad visible.

### 11.2 Ejercicio práctico: Revisar documento

**Caso:** ejecutar `Revisar documento` en un tenant de entrenamiento llamado `Academy Demo Tenant`.

**Tareas dentro de Compliance 360:**

1. Iniciar sesión con usuario de entrenamiento del rol `Customer User`.
2. Navegar al módulo correspondiente.
3. Ejecutar el proceso de punta a punta, respetando permisos.
4. Registrar evidencia o comentario.
5. Consultar dashboard, reporte o audit trail para validar resultado.

**Entregable del estudiante:** captura o descripción del resultado, estado final, evidencia creada y lección aprendida.

**Criterio de aprobación:** proceso completo sin violar permisos, sin usar tenant incorrecto y con trazabilidad visible.

### 11.3 Ejercicio práctico: Ver indicador

**Caso:** ejecutar `Ver indicador` en un tenant de entrenamiento llamado `Academy Demo Tenant`.

**Tareas dentro de Compliance 360:**

1. Iniciar sesión con usuario de entrenamiento del rol `Customer User`.
2. Navegar al módulo correspondiente.
3. Ejecutar el proceso de punta a punta, respetando permisos.
4. Registrar evidencia o comentario.
5. Consultar dashboard, reporte o audit trail para validar resultado.

**Entregable del estudiante:** captura o descripción del resultado, estado final, evidencia creada y lección aprendida.

**Criterio de aprobación:** proceso completo sin violar permisos, sin usar tenant incorrecto y con trazabilidad visible.

### 11.4 Ejercicio práctico: Validar CAPA

**Caso:** ejecutar `Validar CAPA` en un tenant de entrenamiento llamado `Academy Demo Tenant`.

**Tareas dentro de Compliance 360:**

1. Iniciar sesión con usuario de entrenamiento del rol `Customer User`.
2. Navegar al módulo correspondiente.
3. Ejecutar el proceso de punta a punta, respetando permisos.
4. Registrar evidencia o comentario.
5. Consultar dashboard, reporte o audit trail para validar resultado.

**Entregable del estudiante:** captura o descripción del resultado, estado final, evidencia creada y lección aprendida.

**Criterio de aprobación:** proceso completo sin violar permisos, sin usar tenant incorrecto y con trazabilidad visible.

### 11.5 Ejercicio práctico: Solicitar aclaración

**Caso:** ejecutar `Solicitar aclaración` en un tenant de entrenamiento llamado `Academy Demo Tenant`.

**Tareas dentro de Compliance 360:**

1. Iniciar sesión con usuario de entrenamiento del rol `Customer User`.
2. Navegar al módulo correspondiente.
3. Ejecutar el proceso de punta a punta, respetando permisos.
4. Registrar evidencia o comentario.
5. Consultar dashboard, reporte o audit trail para validar resultado.

**Entregable del estudiante:** captura o descripción del resultado, estado final, evidencia creada y lección aprendida.

**Criterio de aprobación:** proceso completo sin violar permisos, sin usar tenant incorrecto y con trazabilidad visible.

### 11.6 Ejercicio práctico: Revisar dashboard

**Caso:** ejecutar `Revisar dashboard` en un tenant de entrenamiento llamado `Academy Demo Tenant`.

**Tareas dentro de Compliance 360:**

1. Iniciar sesión con usuario de entrenamiento del rol `Customer User`.
2. Navegar al módulo correspondiente.
3. Ejecutar el proceso de punta a punta, respetando permisos.
4. Registrar evidencia o comentario.
5. Consultar dashboard, reporte o audit trail para validar resultado.

**Entregable del estudiante:** captura o descripción del resultado, estado final, evidencia creada y lección aprendida.

**Criterio de aprobación:** proceso completo sin violar permisos, sin usar tenant incorrecto y con trazabilidad visible.

---

# CAPÍTULO 12 - Certificación

## Criterios de aprobación

- Evaluación teórica mínima: 80%.
- Evaluación práctica mínima: 85%.
- No cometer violaciones críticas: operar tenant equivocado, usar permisos no asignados, aprobar sin evidencia o prometer funcionalidades no implementadas.

## Puntaje mínimo

| Componente | Peso | Mínimo |
| --- | --- | --- |
| Evaluación teórica | 50% | 80% |
| Evaluación práctica | 35% | 85% |
| Checklist operativo | 10% | Completo |
| Conducta de seguridad y trazabilidad | 5% | Sin faltas críticas |

## Competencias adquiridas

- Comprensión del rol `Customer User`.
- Uso de módulos asignados.
- Respeto de RBAC, MFA, multitenancy y Audit Trail.
- Ejecución de procesos reales.
- Identificación de errores comunes y escalamiento.

## Nivel alcanzado

`Compliance 360 Certified User`.

## Matriz final de permisos

| Módulo | Acceso | Permiso/Claim orientativo | Alcance |
| --- | --- | --- | --- |
| Tenant Management | No | No asignado | No debe acceder salvo aprobación formal |
| Identity | No | No asignado | No debe acceder salvo aprobación formal |
| RBAC | No | No asignado | No debe acceder salvo aprobación formal |
| MFA | No | No asignado | No debe acceder salvo aprobación formal |
| Audit Trail | Sí | AUDIT.READ limitado | Operar/consultar según alcance del rol |
| Storage | No | No asignado | No debe acceder salvo aprobación formal |
| Notifications | No | No asignado | No debe acceder salvo aprobación formal |
| Document Management | Sí | DOCUMENT.READ | Operar/consultar según alcance del rol |
| Workflow Engine | No | No asignado | No debe acceder salvo aprobación formal |
| Technical Sheets | No | No asignado | No debe acceder salvo aprobación formal |
| Supplier Management | No | No asignado | No debe acceder salvo aprobación formal |
| Audit Management | No | No asignado | No debe acceder salvo aprobación formal |
| CAPA Management | No | No asignado | No debe acceder salvo aprobación formal |
| Risk Management | No | No asignado | No debe acceder salvo aprobación formal |
| Quality Indicators | No | No asignado | No debe acceder salvo aprobación formal |
| Reporting Engine | Sí | CUSTOMER.PORTAL | Operar/consultar según alcance del rol |
| Dashboard | Sí | CUSTOMER.PORTAL | Operar/consultar según alcance del rol |
| Template Builder | No | No asignado | No debe acceder salvo aprobación formal |
| Regulatory Management | No | No asignado | No debe acceder salvo aprobación formal |
| Training Management | No | No asignado | No debe acceder salvo aprobación formal |
| Supplier Portal | No | No asignado | No debe acceder salvo aprobación formal |
| Customer Portal | Sí | CUSTOMER.PORTAL | Operar/consultar según alcance del rol |
| Observability | No | No asignado | No debe acceder salvo aprobación formal |
| CI/CD | No | No asignado | No debe acceder salvo aprobación formal |
| Security Hardening | No | No asignado | No debe acceder salvo aprobación formal |
