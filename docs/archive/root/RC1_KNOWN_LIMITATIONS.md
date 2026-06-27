# RC-1 Known Limitations

Fecha: 2026-06-25

## Veredicto Honesto

RC-1 esta listo para pruebas funcionales, no para produccion.

## Limitaciones Conocidas

### KL-001 - QA visual completo pendiente

No se ejecuto una revision manual exhaustiva en navegador de cada pantalla, breakpoint, dark mode y flujo visual. La SPA compila y los endpoints principales responden, pero la validacion visual completa corresponde a la fase funcional del Product Owner.

### KL-002 - Performance con dataset real pendiente

Build, tests y smoke local pasaron. No se ejecuto load test ni benchmark con dataset enterprise representativo.

### KL-003 - Revision DBA profunda pendiente

No se midieron locks, bloat, vacuum/autovacuum, restore points ni planes reales de PostgreSQL con credenciales DBA.

### KL-004 - Providers externos reales

Storage local fue configurado y probado. No se envio email real ni se validaron proveedores externos SMTP/cloud/payment/AI.

### KL-005 - Technical Sheets y Suppliers no tuvieron smoke live RC1

Estos modulos estan cubiertos por pruebas automatizadas existentes, pero en RC-1 no se ejecuto smoke live especifico para creacion de ficha tecnica ni supplier.

### KL-006 - Document upload binario

El smoke valido creacion de documento y storage provider. No se valido carga binaria real de archivo durante este RC-1.

### KL-007 - Artefactos Excel sin trackear

Persisten sin trackear:

- `Formato_Carpetas.xls`
- `Formato_Carpetas (1).xls`

No se eliminaron porque pueden ser documentos reales del usuario/proyecto.

### KL-008 - CI/CD secrets deben configurarse

Se removieron secretos hardcodeados. Para que CI/CD ejecute correctamente, se deben configurar:

- `CI_POSTGRES_PASSWORD`
- `CI_JWT_SIGNING_KEY`

## Defecto Critico Detectado y Corregido

`Reporting Engine` fallaba en smoke live al ejecutar reportes por un problema de tracking EF. Fue corregido y validado nuevamente con build, tests y smoke completo.
