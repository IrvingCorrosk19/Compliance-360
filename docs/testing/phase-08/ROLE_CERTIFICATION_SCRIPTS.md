# Fase 8 — Certificación por rol (sesiones aisladas)
## Certification v2.0 · DISEÑO · sin ejecución

**Regla:** un rol = logout completo = evidencia separada en `evidence/role-<name>/`.  
**No ejecutar** estos scripts hasta Entry Gate firmado.


## Script R1 — Tenant Administrator
1. Login  
2. TAC: verificar/crear usuarios RA roles  
3. `#/regulatory` todas las vistas  
4. Bootstrap  
5. Producto + dossier + accept criticals + submit  
6. Approve (permitido)  
7. Import stage XLSX (permitido)  
8. Licencia + renew  
9. Logout  

## Script R2 — Regulatory Administrator
Igual R1 operación RA; confirmar Configure/import/approve.

## Script R3 — Regulatory Specialist
1. Login limpio  
2. Crear producto/dossier; transitions; submit OK  
3. Approve → **403**  
4. Import → **403**  
5. License create → **403**; license list puede READ  
6. Logout  

## Script R4 — Regulatory Reviewer
1. Ver portafolio/pipeline  
2. Approve dossier preparado → OK  
3. Create product → **403**  
4. Logout  

## Script R5 — Regulatory Viewer
1. Todas las vistas lectura  
2. Cualquier write UI falla / API 403  
3. Logout  

## Script R6 — Quality Manager
1. Approve permitido  
2. Create product no  
3. Logout  

## Script R7 — Viewer (global)
1. Menú reducido; regulatory si tiene RA read  
2. Sin writes  

## Script R8 — Document Controller / CAPA / Risk / etc.
1. Confirmar `#/regulatory` **no** disponible (sin grants)  
2. APIs regulatory → 403  

## Script R9 — Platform Administrator
1. SuperAdmin; no contaminar datos RA business sin cambiar tenant  
2. Crear/inspect tenants only  

## Evidencia mínima por script
- Screenshot menú  
- Network 200/403 sample  
- caseNumber / jobId generados  
