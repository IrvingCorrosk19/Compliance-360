# 05 — Test Environment
## v2.0 DISEÑO — calificación de lab

## 1. Ambiente objetivo (Lab Certification)

| Parámetro | Valor |
|-----------|--------|
| App URL | `http://localhost:5272` |
| Health | `GET /health/live` → 200 |
| Stack | .NET 9 · PostgreSQL `compliance360` |
| UI | SPA `wwwroot` + `regulatory-affairs.js` |
| Contrato Excel | raíz repo `REGUTRACK 02JUN26 MG.xlsx` |
| Launch | `dotnet run` project `Compliance360.Web` profile `http` |

## 2. Connection (lab only)

Definida en máquina de certificación vía env:

`ConnectionStrings__Compliance360=Host=localhost;Port=5432;Database=compliance360;Username=…;Password=…`

**Prohibido** versionar secretos reales en git.

## 3. Qualificación de ambiente (antes de Fase 10)

| Check | Criterio | Evidencia |
|-------|----------|-----------|
| Q1 | Build Release/Debug sin error | log `dotnet build` |
| Q2 | Migraciones RA al día | bootstrap console Schema OK |
| Q3 | Health LIVE 200 | request log |
| Q4 | Unit/domain Regulatory filter verde | `dotnet test` |
| Q5 | Excel presente + tamaño/fecha registrados | `06` |
| Q6 | Seed users RA (`07`) creables | TAC API |
| Q7 | Bootstrap RA → pack 22 defs | API |

## 4. Estabilidad

- Rate limit Development: elevado para baterías (no desactivar auth).  
- Ventana de ejecución dedicada: no deploy paralelo salvo hotfix P0.  
- Backup DB recomendado antes de commit masivo Excel.

## 5. Ambientes posteriores

| Env | Uso |
|-----|-----|
| Staging | Repetición Entry/Exit parcial tras lab GO staging |
| Production | Solo marco `12` — no certificar producción en lab |

## 6. Herramientas de ejecución (post-Gate)

| Herramienta | Uso |
|-------------|-----|
| Navegador real Chromium/Edge | Manual UI |
| Playwright | Solo Fase 9 |
| Scripts API (PowerShell/`HttpClient`) | Soporte evidenciado; no sustituyen UI P0 |
| Editor / Diff Excel | Comparación columnas vs matriz |
