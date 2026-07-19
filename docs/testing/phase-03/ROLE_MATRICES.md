# Phase 03 — Role Matrices
## v2.0 DISEÑO · Fuente: `RoleCatalog.cs` · sin ejecución

Roles inventados no existen. “Sales” no es rol — es campo `SalesMarketingInput`.

---

## 1. Roles en alcance de certificación RA

### 1.1 Regulatory Administrator

| Dimensión | Contenido |
|-----------|-----------|
| Permisos | Configure, Product*, Dossier*, Submit, Approve, Obs, Registration*, Mfr*, License*, Report, AuditRead |
| Pantallas | Todas las 10 vistas RA + import/config |
| Positivos | Bootstrap, import, CRUD, approve, renew |
| Negativos | N/A elevación; no cross-tenant |
| Script | `phase-08` RA-ADMIN |

### 1.2 Regulatory Specialist

| Dimensión | Contenido |
|-----------|-----------|
| Permisos | Product manage, dossier create/update/submit, obs, mfr manage, license **read**, report read — **sin Approve, Configure** |
| Pantallas | Operación día a día; import/config ocultos o 403 |
| Positivos | Crear producto/dossier, checklist, submit, obs |
| Negativos | Approve=403, import=403 |
| Script | RA-SPEC |

### 1.3 Regulatory Reviewer

| Dimensión | Contenido |
|-----------|-----------|
| Permisos | Product **read**, dossier **read**, **Approve**, registration manage, report |
| Pantallas | Pipeline/dossiers/registrations (approve); no alta producto |
| Positivos | Approve CT en UnderAuthorityReview |
| Negativos | POST products=403, import=403, create dossier=403 |
| Script | RA-REV |

### 1.4 Regulatory Viewer

| Dimensión | Contenido |
|-----------|-----------|
| Read-only | products/dossiers/regs/mfr/license/report |
| Negativos | Cualquier POST/PUT write = 403 |
| Script | RA-VIEW |

### 1.5 Tenant Administrator

| Dimensión | Contenido |
|-----------|-----------|
| RA | Mismos grants RA que Admin regulatorio + TAC users/roles |
| Extra | Sembrar usuarios certificación |
| Script | TAC-ADMIN |

### 1.6 Quality Manager

| Dimensión | Contenido |
|-----------|-----------|
| RA | Dossier read, Approve, registration manage, report |
| Parecido Reviewer | Certificar camino approve; deny product create |
| Script | QM-RA (parcial) |

### 1.7 Viewer (tenant genérico)

Read RA como Regulatory Viewer; script negativo write.

### 1.8 Roles sin RA (Document Controller, CAPA Manager, …)

TC negativo: sin acceso efectivo a datos RA (403 o UI sin módulo).

### 1.9 Platform roles

No operan tenant RA business data; TC: login platform ≠ ver expedientes tenant sin soporte.

---

## 2. Matriz permiso × rol (RA codes)

| Permiso | RA Adm | Spec | Rev | RA View | TAC Adm | QM |
|---------|--------|------|-----|---------|---------|-----|
| PRODUCT.READ | ✓ | ✓ | ✓ | ✓ | ✓ | —* |
| PRODUCT.MANAGE | ✓ | ✓ | ✗ | ✗ | ✓ | ✗ |
| DOSSIER.READ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| DOSSIER.CREATE/UPDATE | ✓ | ✓ | ✗ | ✗ | ✓ | ✗ |
| DOSSIER.SUBMIT | ✓ | ✓ | ✗ | ✗ | ✓ | ✗ |
| DOSSIER.APPROVE | ✓ | ✗ | ✓ | ✗ | ✓ | ✓ |
| OBSERVATION.MANAGE | ✓ | ✓ | ✗ | ✗ | ✓ | ✗ |
| CONFIGURE (import) | ✓ | ✗ | ✗ | ✗ | ✓ | ✗ |
| LICENSE.MANAGE | ✓ | ✗† | ✗ | ✗ | ✓ | ✗ |
| REPORT.READ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

\*QM: sin ProductRead en catálogo base — validar comportamiento real en Fase 10.  
†Spec tiene LICENSE.READ only.

---

## 3. Regresión obligatoria post–BUG_003

`Regulatory.Manage` **no** incluye `REGISTRATION.MANAGE`.  
Reviewer con Registration.Manage **no** debe satisfacer create product.
