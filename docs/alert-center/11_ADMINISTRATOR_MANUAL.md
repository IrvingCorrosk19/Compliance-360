# 11 — Manual funcional del administrador

**Estado:** manual de operación objetivo; se actualizará con capturas tras implementación.

## 1. Perfiles administrativos

- Functional Rule Designer.
- Template Designer.
- Compliance Reviewer/Approver.
- Alert Operator.
- Provider Administrator.
- Secret Administrator.
- Tenant Alert Administrator.
- Auditor.

No se recomienda asignar todas las capacidades a una sola persona.

## 2. Crear un evento

1. Abra **Alert Center → Diseño → Eventos**.
2. Seleccione **Crear evento**.
3. Defina código estable, nombre, módulo, owner y descripción.
4. Agregue campos al schema con tipo, required y sensibilidad.
5. Defina variables expuestas.
6. Cargue un ejemplo válido y uno inválido.
7. Ejecute **Validar**.
8. Guarde Draft.
9. Envíe a revisión y publique la versión aprobada.

Eventos puramente de UI no hacen que el backend los emita. Un evento nuevo debe tener una fuente declarativa soportada: schedule, date-offset, webhook/API o productor integrado.

## 3. Crear una regla

1. Abra **Reglas → Nueva regla**.
2. Complete identidad/owner/criticidad.
3. Seleccione evento o schedule.
4. Construya condiciones con el editor visual.
5. Defina dedupe/throttling.
6. Seleccione audiencia/fallbacks.
7. Configure canales/templates/locales.
8. Agregue SLA/escalaciones.
9. Configure timezone/calendario/quiet hours.
10. Ejecute simulación positiva, negativa y de volumen.
11. Revise el resumen en lenguaje natural.
12. Guarde Draft y envíe a revisión.
13. El aprobador revisa diff/evidencia.
14. El publisher publica.
15. El activator programa canary y monitorea.

## 4. Crear audiencia

1. Abra **Destinatarios → Nueva audiencia**.
2. Elija estática o dinámica.
3. Seleccione users, roles, groups, departments, owner, manager u otra fuente.
4. Configure condiciones/exclusiones/fallbacks.
5. Ejecute preview.
6. Verifique usuarios sin endpoint/canal.
7. Guarde, revise y publique.

Para campañas críticas, congele snapshot.

## 5. Crear template

1. Abra **Plantillas → Nueva plantilla**.
2. Defina código, categoría, owner, clasificación y canales.
3. Elija locale base.
4. Use editor visual/HTML seguro.
5. Inserte variables desde catálogo.
6. Configure text alternative, branding y dark mode.
7. Añada traducciones.
8. Ejecute lint y preview.
9. Envíe prueba a allowlist.
10. Envíe a aprobación.
11. Aprobador firma/publica según policy.

Nunca incluya contraseñas, tokens, secretos o PII innecesaria.

## 6. Configurar provider

Provider Administrator:

1. Abra **Canales y Proveedores → Proveedores → Nuevo**.
2. Elija canal/tipo/ambiente/residencia.
3. Complete endpoint/sender/rate limit.
4. Guarde metadata.

Secret Administrator:

5. Abra **Configurar secreto**.
6. Introduzca credencial write-only.
7. Configure expiración/rotación.

Provider Administrator:

8. Ejecute health y test a allowlist.
9. Configure routing/failover.
10. Envíe a aprobación.
11. Active en canary.

La UI nunca vuelve a mostrar el secreto.

## 7. Configurar schedule

1. Abra **Programaciones → Nueva**.
2. Elija frecuencia mediante builder.
3. Seleccione timezone IANA.
4. Agregue business calendar/holidays.
5. Defina quiet windows y misfire policy.
6. Revise las próximas ejecuciones, incluido DST.
7. Vincule regla/digest/SLA.
8. Simule y publique.

## 8. Operar cola

1. Abra **Operaciones → Cola**.
2. Filtre por estado, edad, prioridad, canal, provider o tenant scope.
3. Abra mensaje y revise timeline.
4. Para retry/cancel/release lease:
   - verifique estado;
   - revise impacto;
   - indique motivo;
   - confirme.
5. Verifique el resultado por correlation ID.

No edite estados directamente ni use SQL.

## 9. Remediar DLQ

1. Abra **Dead Letters**.
2. Agrupe por causa.
3. Asigne owner.
4. Abra el caso.
5. Revise intentos, provider y error redacted.
6. Corrija solo campos reparables o la dependencia.
7. Ejecute test/simulación.
8. Seleccione Requeue o Discard.
9. Registre motivo.
10. Confirme timeline.

Requeue no borra el registro original.

## 10. Gestionar Inbox

Usuario:

- leer/no leer;
- favorite/pin;
- archive;
- snooze;
- acknowledge;
- ejecutar acción/resolver.

Supervisor:

- ver scope de equipo;
- asignar/reasignar;
- recordar/escalar;
- revisar vencidas.

Archive/DeleteFromInbox no elimina evidencia.

## 11. Monitorear Command Center

Revise:

- queue age/depth;
- outbox/scheduler/projection lag;
- throughput;
- delivery/failure/retry/DLQ;
- provider health/callback lag;
- SLA;
- tendencias por regla/template/canal.

Drill-down siempre conserva filtros. Si una métrica no reconcilia, abra incidente; no cambie configuraciones para ocultarla.

## 12. Aprobar y publicar

Aprobador:

1. Abra **Mis tareas**.
2. Verifique versión, diff, owner y simulaciones.
3. Revise impacto/dependencies.
4. Apruebe, rechace o solicite cambios con comentario.

Publisher:

5. Verifique approvals/hash.
6. Publique y defina vigencia.

Activator:

7. Defina ring/canary.
8. Active.
9. Monitoree.
10. Complete rollout o rollback.

## 13. Auditoría y evidence pack

1. Abra **Auditoría**.
2. Filtre actor/recurso/acción/fecha/outcome/correlation.
3. Abra diff.
4. Verifique integridad.
5. Seleccione **Crear expediente**.
6. Defina alcance/formato/motivo.
7. Espere job.
8. Descargue antes de expiración.

Exportar contenido sensible requiere permiso reforzado y queda auditado.

## 14. Retención y legal hold

1. Cree policy en Draft.
2. Simule filas/bytes afectados.
3. Revise mínimos regulatorios.
4. Aplique legal hold si corresponde.
5. Obtenga aprobación.
6. Ejecute job controlado.
7. Verifique manifest y resultados.

Purge es irreversible; rollback se basa en backup conforme a política, no en botón.

## 15. Promover entre ambientes

1. Seleccione artefactos.
2. Calcule dependencies.
3. Valide target/drift.
4. Revise diff.
5. Excluya secrets.
6. Envíe a aprobación.
7. Programe promoción.
8. Verifique deployment.
9. Ejecute smoke.
10. Complete o rollback.

## 16. Respuesta a incidentes

### Backlog

- verificar worker/DB/provider;
- pausar productores no críticos si es necesario;
- aumentar workers dentro de límites;
- priorizar críticos;
- monitorear queue age;
- reconciliar tras recuperación.

### Provider caído

- confirmar health/error class;
- abrir circuit;
- activar failover permitido;
- comunicar impacto;
- verificar duplicates/unknown outcomes;
- recuperar gradualmente.

### DLQ spike

- agrupar causa;
- detener regla/provider si reduce daño;
- corregir dependencia;
- test;
- requeue por lotes;
- reconciliar.

### Sospecha de fuga

- deshabilitar credencial;
- rotar;
- preservar evidencia;
- activar incident response;
- revisar audit/access;
- no copiar secretos a tickets.

## 17. Operaciones prohibidas

- Cambiar estados con SQL.
- Compartir secretos.
- Reutilizar cuentas.
- Aprobar trabajo propio.
- Enviar test a destinatarios productivos no autorizados.
- Deshabilitar auditoría/RLS.
- Eliminar evidencia.
- Reprocesar masivamente sin preview.
- Usar GET para generar alertas.
- Declarar delivery exitoso sin evidencia provider/callback.

