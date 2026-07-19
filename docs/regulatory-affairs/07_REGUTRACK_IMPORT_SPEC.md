# 07 — REGUTRACK Import Spec

`POST /imports/stage` con `rowsJson` (array tipado).  
Validación → Simulated/Validated → `POST /imports/{id}/commit`.  
Staging tables: `regutrack_import_jobs`, `regutrack_import_rows`.  
Parser .xlsx nativo: pendiente (Medium).
