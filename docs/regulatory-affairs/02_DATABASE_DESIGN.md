# 02 — Database Design

Migración: `20260714025033_AddRegulatoryAffairs`

Schema `compliance360`, tablas snake_case listadas en baseline §7 y DbContext `ConfigureRegulatoryAffairs`.

Índices únicos: `(TenantId, CatalogCode)`, `(TenantId, CaseNumber)`, `(TenantId, Authority.Code)`, `(TenantId, Pack.Code)`.
