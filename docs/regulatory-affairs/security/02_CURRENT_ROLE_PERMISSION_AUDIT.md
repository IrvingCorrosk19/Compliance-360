# 02 — Current Role & Permission Audit
## Compliance 360 · Regulatory Affairs · AS-BUILT

**Fecha:** 2026-07-14  
**Fuente:** `RoleCatalog.cs`, `PermissionCatalog` / `RbacCatalog.cs`, `PermissionPolicies.cs`, `FoundationEndpoints.MapRegulatoryAffairs`, `RegulatoryAffairsService`, `RegistrationDossier`.  
**Estado certificación funcional por roles:** **PAUSADA** hasta veredicto SoD.

---

## 1. Permisos REGULATORY.* actuales (17)

| Código | Significado catalogado |
|--------|------------------------|
| REGULATORY.PRODUCT.READ / .MANAGE | Productos DM |
| REGULATORY.DOSSIER.READ / .CREATE / .UPDATE | Expediente |
| REGULATORY.DOSSIER.SUBMIT | Someter ante autoridad |
| REGULATORY.DOSSIER.APPROVE | **“Approve dossiers and activate CT/RS”** = decisión externa + alta registro |
| REGULATORY.REQUIREMENT.MANAGE | Checklist (aceptar/rechazar/waive en un solo permiso) |
| REGULATORY.OBSERVATION.MANAGE | Crear/responder/cerrar observaciones |
| REGULATORY.REGISTRATION.READ / .MANAGE | CT/RS |
| REGULATORY.MANUFACTURER_DOCUMENT.READ / .MANAGE | Fabricantes/certs |
| REGULATORY.LICENSE.READ / .MANAGE | Licencias OP |
| REGULATORY.REPORT.READ | Dashboard |
| REGULATORY.CONFIGURE | Bootstrap, packs, import |

**Problema estructural:** un único `DOSSIER.APPROVE` mezcla “activar CT/RS de autoridad” con el concepto inglés *approve*, y **no existe** permiso de **aprobación interna para sometimiento**.

---

## 2. Policies amigables (fuga de SoD)

| Policy | Accepts (AddAny) | Problema |
|--------|------------------|----------|
| `Regulatory.Submit` | SUBMIT **∨ APPROVE** | Reviewer puede llamar `/submit` sin SUBMIT |
| `Regulatory.Approve` | APPROVE **∨ REGISTRATION.MANAGE** | Aceptable para CT/RS |
| `Regulatory.Manage` | CREATE/UPDATE/OBS/REQ/MFR/LICENSE/CONFIGURE (sin REGISTRATION.MANAGE) | Incluye `POST /transition` a **cualquier** estado permitido, incl. `Approved`/`Rejected` **sin** crear CT/RS |
| `Regulatory.Read` | reads + manages (read elevation) | OK lectura |

---

## 3. Roles as-built con grants RA

| Rol sistema | Grants RA relevantes |
|-------------|----------------------|
| **Tenant Administrator** | **Los 17** (incluye SUBMIT+APPROVE+CONFIGURE) — contradice “no opera datos de negocio por defecto” |
| **Regulatory Administrator** | Los 17 |
| **Regulatory Specialist** | Product*, Dossier C/U/**SUBMIT**, REQ, OBS, Reg READ, Mfr*, Lic READ |
| **Regulatory Reviewer** | Dossier READ+**APPROVE**, Reg R+M — **sin** preparar |
| **Regulatory Viewer** / Viewer | Solo READ |
| **Quality Manager** | Dossier READ+APPROVE, Reg R+M, Report |

Roles **inexistentes** hoy: Regulatory Manager, Approver (interno), Submitter dedicado, Observation Specialist, Sales Viewer, Document Contributor.

---

## 4. Semántica real de Approve vs Submit

| Acción | Endpoint | Efecto real |
|--------|----------|-------------|
| Submit | `POST .../submit` | `ReadyForSubmission`→`Submitted`; **no** crea CT/RS |
| Open observation | `POST .../observations` | Registra feedback autoridad → `Observed` |
| **Approve** | `POST .../approve` | Exige UAR; →`Approved`; **crea `SanitaryRegistration` activa**; →`Closed`; producto comercializable |

**Conclusión:** el sistema **sí** separa sometimiento de decisión de autoridad en endpoints, pero el estado `Approved` **solo** modela la decisión externa. **No** hay estado/permission de autorización interna previa al sometimiento.

---

## 5. Controles SoD en backend

| Control | ¿Existe? |
|---------|----------|
| PreventSelfReview | **No** |
| PreventSelfApproval | **No** |
| Comparación CreatedByUserId vs actor | **No** en RA service |
| Gate Ready→ApprovedForSubmission | **No** |
| Restricción de `transition` a estados terminales de autoridad | **No** (solo grafo AllowedTransitions) |

Ocultar botón en UI ≠ seguridad. Enforcement real = claim + recurso + estado + política SoD.
