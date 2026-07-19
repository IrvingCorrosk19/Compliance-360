#!/usr/bin/env python3
"""Expand thin role manuals with detailed test case steps."""
from pathlib import Path

OUT = Path(__file__).resolve().parents[1] / "docs" / "manual-functional-testing"

def tc(id_, name, obj, priority, tipo, pre, data, route, menu, screen, steps, expected, audit, evidence):
    steps_txt = "\n".join(f"{i+1}. {s}" for i, s in enumerate(steps))
    return f"""
### {id_} — {name}

| Campo | Valor |
|-------|-------|
| **Objetivo** | {obj} |
| **Prioridad** | {priority} |
| **Tipo** | {tipo} |
| **Precondiciones** | {pre} |
| **Datos** | {data} |
| **Ruta inicial** | `{route}` |
| **Menú** | {menu} |
| **Pantalla** | {screen} |

**Pasos:**

{steps_txt}

**Resultado final esperado:** {expected}

**Auditoría esperada:** {audit}

**Evidencia:** `{evidence}`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---
"""

def footer(role, prefix, crit):
    return f"""
## 6. Validaciones negativas

Ver casos marcados **Negativo** y **Permisos** en sección 5.

## 7. Validación visual

Loaders, toasts, labels exactos del Action Center, listados refrescados post-creación.

## 8. Permisos esperados

Ver `RoleCatalog.cs` para permisos oficiales del rol {role}.

## 9. Auditoría

Cada mutación exitosa genera evento en `#/audit-trail` con actor, tenantId, correlationId.

## 10. Criterio de aprobación del rol

{crit} PASS obligatorio. Mínimo 10/12 casos PASS.

## 11. Qué aprendí

Este rol es pieza del programa MFT; ejecute en orden del roadmap `00`.

## 12. Referencias y extensiones API

- URL: `http://localhost:5272`
- Journey: `scripts/customer_journey.ps1`
- Credenciales: `e2e/testdata.json`
"""

# Expand 09 Supplier Manager with full steps
write_09 = f"""# Manual funcional — Supplier Manager

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Supplier Manager

## 1. Propósito del rol

Gestiona **proveedores**, evaluaciones, homologación y documentación asociada.

## 2. Precondiciones

Storage configurado (manual 04). Usuario `supplier@alimentos-premium.test`.

## 3. Credenciales

| Email | `supplier@alimentos-premium.test` | Contraseña | `e2e/testdata.json` |

## 4. Datos de prueba

| Campo UI | Valor |
|----------|-------|
| Razon social | Distribuidora Central MFT S.A. |
| Identificacion fiscal | RUC-SUP-MFT-001 |
| Pais | PA |

## 5. Casos de prueba

### Login v2: supplier@ → **Siguiente** → **Continuar** → contraseña → **Iniciar sesion**

{tc("TC-SM-001","Login Supplier Manager","Acceso al módulo proveedores","Crítica","Positivo","e2e_provision.ps1","supplier@alimentos-premium.test","http://localhost:5272/","Login","Enterprise Login",["Abra `http://localhost:5272/`","Ingrese supplier@alimentos-premium.test → **Siguiente**","Seleccione Alimentos Premium → **Continuar**","Contraseña desde testdata → **Iniciar sesion**","Verifique toast éxito","Confirme sidebar **Operations** con **Suppliers**"],"Sesión activa","AuthenticationSuccess","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-001/")}

{tc("TC-SM-002","Navegar Suppliers Module","Abrir Action Center proveedores","Alta","Positivo","TC-SM-001 PASS","—","#/suppliers","Operations → Suppliers","Suppliers Module",["Clic **Suppliers** o navegue `#/suppliers`","Espere loader","Localice `#module-action-form`","Verifique labels **Razon social**, **Identificacion fiscal**, **Pais**","Verifique botón **Crear registro real**"],"Formulario proveedores visible","N/A","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-002/")}

{tc("TC-SM-003","Crear proveedor","Alta proveedor crítica","Crítica","Positivo","TC-SM-002 PASS","Sección 4","#/suppliers","Suppliers","Action Center",["**Razon social:** Distribuidora Central MFT S.A.","**Identificacion fiscal:** RUC-SUP-MFT-001","**Pais:** PA","Presione **Crear registro real**","Espere toast éxito","Verifique fila en listado"],"Proveedor creado","SupplierCreated","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-003/")}

{tc("TC-SM-004","Segundo proveedor","Segundo registro","Media","Positivo","TC-SM-003 PASS","RUC-SUP-MFT-002","#/suppliers","Suppliers","Action Center",["Razon social: Lacteos del Istmo MFT","Identificacion fiscal: RUC-SUP-MFT-002","Pais: PA","**Crear registro real**"],"Segundo proveedor listado","SupplierCreated","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-004/")}

{tc("TC-SM-005","RUC duplicado","Validación unicidad","Alta","Negativo","TC-SM-003 PASS","RUC-SUP-MFT-001","#/suppliers","Suppliers","Action Center",["Reingrese RUC-SUP-MFT-001","**Crear registro real**","Observe error UI","Network ≠ 500"],"Rechazo validación","SupplierCreateFailed","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-005/")}

{tc("TC-SM-006","País inválido","maxlength ISO2","Media","Negativo","TC-SM-002 PASS","PAN","#/suppliers","Suppliers","Action Center",["Pais: PAN (3 chars)","**Crear registro real**"],"Validación maxlength=2","N/A","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-006/")}

{tc("TC-SM-007","Documents lectura","Consulta documentos","Media","Positivo","TC-SM-001 PASS","—","#/documents","Operations → Documents","Documents",["Navegue `#/documents`","Observe listado","Confirme sin **Crear registro real** o Modo solo lectura"],"Solo lectura o sin CREATE","N/A","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-007/")}

{tc("TC-SM-008","Sin tenant admin","SoD administración","Alta","Permisos","TC-SM-001 PASS","—","#/tenant-administration","Enterprise","TAC",["Navegue `#/tenant-administration`","Verifique sin tab **Usuarios** editable"],"Sin TENANT.USERS","AuthorizationDenied","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-008/")}

{tc("TC-SM-009","Report Center lectura","Reportes","Media","Positivo","TC-SM-001 PASS","—","#/reports","Command Center → Reports","Report Center",["Navegue `#/reports`","Verifique **Report Center** visible"],"Lectura reportes","N/A","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-009/")}

{tc("TC-SM-010","Audit Trail","Trazabilidad","Alta","Positivo","TC-SM-003 PASS","—","#/audit-trail","Command Center","Audit Trail",["Navegue `#/audit-trail`","Busque SupplierCreated","Presione **Exportar**"],"Evento auditado","AuditExported","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-010/")}

{tc("TC-SM-011","Handoff CAPA","Preparar NC","Baja","Positivo","TC-SM-003 PASS","Proveedor ID","#/suppliers","Suppliers","Listado",["Anote proveedor para manual 11","Documente en notas E2E"],"Handoff documentado","N/A","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-011/")}

{tc("TC-SM-012","Logout","Cierre sesión","Baja","Positivo","Sesión activa","—","cualquiera","Topbar","Salir",["Presione **Salir**","Confirme login"],"Sesión cerrada","Logout","artifacts/manual-functional-testing/Supplier-Manager/TC-SM-012/")}

{footer("Supplier Manager","TC-SM","TC-SM-003")}
"""

(OUT / "09_SUPPLIER_MANAGER_FUNCTIONAL_TESTS.md").write_text(write_09.strip() + "\n", encoding="utf-8")
print("Expanded 09")

# Similar pattern for 10 Auditor - key cases with full steps
for fname, role, email, route, mod, fields, prefix, cases in [
("10_AUDITOR_FUNCTIONAL_TESTS.md", "Auditor", "auditor@alimentos-premium.test", "#/audits", "Audits",
 "Nombre/Codigo/Alcance", "TC-AU", [
("001","Login Auditor","Crítica",["Login v2 auditor@","Verifique **Audits** en sidebar"]),
("002","Navegar Audits","Alta",["`#/audits`","Formulario `#module-action-form`","Campos Nombre, Codigo, Alcance"]),
("003","Crear auditoría","Crítica",["Nombre: Auditoria Interna ISO 22000 MFT","Codigo: AUD-MFT-001","Alcance: Sistema de gestion de calidad","**Crear registro real**","Anote Audit ID"]),
("004","Segunda auditoría","Media",["AUD-MFT-002","Alcance: Proveedores criticos","**Crear registro real**"]),
("005","Código duplicado","Negativo",["Repetir AUD-MFT-001","Error validación"]),
("006","Suppliers lectura","Positivo",["`#/suppliers`","Listado sin crear"]),
("007","CAPA sin cierre","Permisos",["`#/capa`","Sin aprobar/cerrar"]),
("008","Documents lectura","Positivo",["`#/documents`","Evidencia auditoría"]),
("009","Report Center","Positivo",["`#/reports`","Lectura"]),
("010","Audit Trail","Alta",["AuditCreated","**Exportar**"]),
("011","Extensión API program/close","Positivo",["Referencia customer_journey.ps1","Documente UI parcial"]),
("012","Logout","Baja",["**Salir**"]),
])]:
    blocks = ""
    for c in cases:
        cid, name, pri, steps = c
        tipo = "Negativo" if "duplicado" in name.lower() or "sin" in name.lower() else "Positivo"
        if "Permisos" in name or pri == "Permisos": tipo = "Permisos"
        blocks += tc(f"{prefix}-{cid}", name, name, pri, tipo, f"{prefix}-001 PASS", "Ver sección 4", route, f"Operations → {mod}", mod,
            steps, "Según objetivo del caso", f"{mod}Created" if "003" in cid else "N/A",
            f"artifacts/manual-functional-testing/{role.replace(' ','-')}/{prefix}-{cid}/")
    content = f"""# Manual funcional — {role}

**Versión:** 1.0 | **URL:** `http://localhost:5272`

## 1. Propósito del rol

Planifica y ejecuta **auditorías** y hallazgos. No cierra CAPAs (SoD).

## 2. Precondiciones

Usuario `{email}` provisionado.

## 3. Credenciales

Email `{email}` | Contraseña `e2e/testdata.json`

## 4. Datos de prueba

| Nombre / titulo | Auditoria Interna ISO 22000 MFT |
| Codigo | AUD-MFT-001 |
| Alcance | Sistema de gestion de calidad e inocuidad alimentaria |

## 5. Casos de prueba

Login v2: {email} → **Siguiente** → **Continuar** → **Iniciar sesion**

{blocks}
{footer(role, prefix, f"{prefix}-003")}
"""
    if fname.startswith("10"):
        (OUT / fname).write_text(content.strip() + "\n", encoding="utf-8")
        print(f"Expanded {fname}")

print("Done")
