# 07 — Test Users & Roles
## v2.0 DISEÑO — anclado a `RoleCatalog.cs`

**Fuente única de verdad de roles:** `src/Compliance360.Domain/Identity/RoleCatalog.cs`  
**Prohibido** inventar roles “Sales” / “Document Contributor” si no existen en catálogo.  
Si el negocio requiere “Sales”, se mapea a permisos reales (`SalesMarketingInput` es campo, no rol).

---

## 1. Roles Platform (fuera de operación RA)

| Rol | ¿Certifica RA negocio? |
|-----|------------------------|
| Platform Administrator | No (salvo soporte TAC) |
| Platform Operations | No |
| Platform Security | No |
| Support Operator | No (break-glass) |

---

## 2. Roles Tenant — relevancia Regulatory Affairs

| Rol | Permisos RA clave | Certificación RA |
|-----|-------------------|------------------|
| **Tenant Administrator** | Configure + Product/Dossier full + Approve + Import | **Completa** (dueño TAC+RA) |
| **Quality Manager** | Dossier Read/Approve + Registration Manage + Report | **Parcial RA** (approve path) |
| **Viewer** (tenant genérico) | Read products/dossiers/regs/mfr/license/report | **Read-only** |
| **Regulatory Administrator** | Configure + full RA write + Approve + Import | **Completa RA** |
| **Regulatory Specialist** | Product/Dossier/Submit/Obs/Mfr — **NO Approve, NO Configure/Import** | **Completa SoD** |
| **Regulatory Reviewer** | Read + **Approve** + Registration Manage — **NO ProductManage** | **Completa SoD** |
| **Regulatory Viewer** | Read-only RA | **Read-only** |
| Document Controller / CAPA / Risk / etc. | Sin grants RA (salvo Auditor read modules) | **Negativo:** no ver consola RA o sin data |

---

## 3. Usuarios de lab (a crear en Fase 10 / Entry E5)

Tenant lab: Irving Corro S.A. (`82af3877-2786-4d39-bce8-c981101c771d`) — solo lab.

| User ID plan | Email plan | Rol |
|--------------|------------|-----|
| U-TAC-01 | (owner lab existente) | Tenant Administrator |
| U-RA-ADM | `ra.admin@cert.local` | Regulatory Administrator |
| U-RA-SPEC | `ra.spec@cert.local` | Regulatory Specialist |
| U-RA-REV | `ra.rev@cert.local` | Regulatory Reviewer |
| U-RA-VIEW | `ra.view@cert.local` | Regulatory Viewer |
| U-QM | opcional | Quality Manager |
| U-VIEW | opcional | Viewer |

Password: política app (≥12, upper/lower/digit/symbol). Rotar tras certificación. **No** documentar secretos en git en claro en reportes públicos.

---

## 4. Matriz SoD mínima (debe tener TC)

| Acción | Admin | Specialist | Reviewer | Viewer |
|--------|-------|------------|----------|--------|
| Bootstrap / Import | ✓ | ✗ | ✗ | ✗ |
| Create product | ✓ | ✓ | ✗ | ✗ |
| Create/transition dossier | ✓ | ✓ | ✗* | ✗ |
| Submit | ✓ | ✓ | ✗ | ✗ |
| Approve CT/RS | ✓ | ✗ | ✓ | ✗ |
| Open observation | ✓ | ✓ | ✗ | ✗ |
| Dashboard read | ✓ | ✓ | ✓ | ✓ |

\*Reviewer sin DossierUpdate/Create — solo Read+Approve.

---

## 5. Notas de autenticación UI

Login enterprise: email → (org radios si multi) → password.  
Casos Auth deben cubrir single-org y multi-org.  
Testdata inventado (`alimentos-premium`) **no** es válido salvo reprovisión.
