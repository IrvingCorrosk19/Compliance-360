# Compliance 360 Technical Debt Report

## Hallazgos Corregidos

### TD-001 - Secretos locales versionados

- Archivo: `src/Compliance360.Web/appsettings.Development.json`
- Riesgo: exposicion de password local y signing key.
- Accion: valores vaciados; ahora deben inyectarse por variables de entorno.
- Estado: corregido.

### TD-002 - Secretos CI hardcodeados

- Archivos: `.github/workflows/ci-cd-enterprise.yml`, `azure-pipelines.yml`
- Riesgo: credenciales y signing keys visibles en pipeline.
- Accion: reemplazado por `CI_POSTGRES_PASSWORD` y `CI_JWT_SIGNING_KEY`.
- Estado: corregido.

### TD-003 - Inconsistencia de idioma en SuperAdmin Platform Center

- Archivo: `src/Compliance360.Web/wwwroot/app.js`
- Riesgo: percepcion de producto no uniforme.
- Accion: normalizacion a espanol y helper central `displayLabel`.
- Estado: corregido parcialmente para la superficie estabilizada.

### TD-004 - Archivo temporal Office

- Archivo: `docs/training/word/~$_PLATFORM_OVERVIEW_CERTIFICATION.docx`
- Riesgo: ruido en repo y artefacto temporal no productivo.
- Accion: eliminado.
- Estado: corregido.

## Hallazgos Pendientes

### TD-005 - Artefactos Excel sin trackear

- Archivos: `Formato_Carpetas.xls`, `Formato_Carpetas (1).xls`
- Riesgo: posible ruido de repo.
- Decision: no eliminar sin confirmacion porque pueden contener informacion real.

### TD-006 - Documentacion historica con veredictos previos

- Archivos: reportes historicos Tenant/Admin/Security.
- Riesgo: lectura confusa si se toma un reporte antiguo como estado actual.
- Recomendacion: mantener como historico, pero publicar un indice de documentacion vigente.

### TD-007 - UI extensa en un solo `app.js`

- Archivo: `src/Compliance360.Web/wwwroot/app.js`
- Riesgo: mantenibilidad decreciente.
- Recomendacion: refactor tecnico futuro a componentes/modulos JS sin cambiar UX.

## Paquetes

`dotnet list Compliance360.sln package --vulnerable` no reporto paquetes vulnerables.
