# Plantilla — Reporte de defecto

**Programa:** Compliance 360 Manual Functional Testing  
**Versión plantilla:** 1.0

---

## Identificación

| Campo | Valor |
|-------|-------|
| **ID Defecto** | DEF-MFT-____ |
| **Caso relacionado** | TC-___-___ / E2E-___ |
| **Manual** | 01–17 (número de archivo) |
| **Rol bajo prueba** | |
| **Reportado por** | |
| **Fecha detección** | |
| **Entorno** | Local `http://localhost:5272` |
| **Tenant ID** | |
| **Severidad** | [ ] Crítica  [ ] Alta  [ ] Media  [ ] Baja |
| **Prioridad fix** | [ ] P0  [ ] P1  [ ] P2  [ ] P3 |
| **Estado** | [ ] Abierto  [ ] En progreso  [ ] Resuelto  [ ] Cerrado  [ ] Won't fix |
| **Tipo** | [ ] Funcional  [ ] UI/UX  [ ] Permisos/RBAC  [ ] API  [ ] Auditoría  [ ] Rendimiento  [ ] Seguridad |

---

## Título (una línea)

> Ejemplo: "Document Controller puede ver botón Aprobar — violación SoD"

---

## Descripción

(Qué falló, en qué contexto, frecuencia de reproducción: siempre / intermitente / una vez)

---

## Pasos para reproducir

(Copiar numerados del manual funcional; incluir nombres exactos de botones)

1.
2.
3.

---

## Resultado esperado

(Según manual — incluir ID caso)

---

## Resultado actual

(Comportamiento observado, mensajes de error, códigos HTTP)

---

## Evidencia

| Tipo | Ruta / descripción |
|------|-------------------|
| Screenshot inicio | `artifacts/manual-functional-testing/...` |
| Screenshot error | |
| Network response | HTTP ___ — body: |
| Audit log excerpt | |
| Console errors | |
| Request/Response JSON | |

---

## Impacto en negocio

- [ ] Bloquea go-live
- [ ] Violación SoD / compliance
- [ ] Pérdida de datos
- [ ] Workaround disponible (describir)
- [ ] Cosmético

**Descripción impacto:**

---

## Workaround temporal

(Si aplica — ej. "usar Swagger POST /decision en lugar de UI")

---

## Información técnica adicional

| Campo | Valor |
|-------|-------|
| Navegador / versión | |
| Ruta hash | `#/...` |
| Endpoint API | |
| CorrelationId (audit) | |
| Usuario email | |
| Commit / build | |

---

## Historial

| Fecha | Acción | Responsable | Notas |
|-------|--------|-------------|-------|
| | Creado | | |
| | Asignado | | |
| | Fix verificado | | |
| | Cerrado | | |

---

## Verificación de fix

| Campo | Valor |
|-------|-------|
| **Caso re-ejecutado** | TC-___-___ |
| **Resultado post-fix** | PASS / FAIL |
| **Regresión en otros casos** | Sí / No — cuáles |
| **Verificado por** | |
| **Fecha verificación** | |

---

## Enlaces

- Resultados: `18_FUNCTIONAL_TEST_RESULTS_TEMPLATE.md`
- Roadmap: `00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md`
- Issue tracker / PR: (URL)
