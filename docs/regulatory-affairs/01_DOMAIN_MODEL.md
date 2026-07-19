# 01 — Domain Model (Regulatory Affairs MVP)

Ver `src/Compliance360.Domain/RegulatoryAffairs/`.

## Aggregates

- `MedicalDeviceProduct` — producto médico (no TechnicalSheet nutricional)
- `RegistrationDossier` — expediente / caso (aggregate raíz operativo)
- `SanitaryRegistration` — CT/RS
- `RegulatoryAuthority` — MINSA/CSS extensible
- `RegulatoryRequirementPack` + `RequirementDefinition` — definición versionable
- `ManufacturerProfile` + `ManufacturerCertificate` — vault fabricante (`SupplierId` opcional)
- `OperatingLicense` + `LicenseRenewalCase` — licencias corporativas
- `RegutrackImportJob` + `RegutrackImportRow` — staging de importación

## Invariantes clave

- Submit bloquea si críticos incompletos
- Active registration requiere número
- Expiration < issued inválido
- Pack congelado en dossier al aplicar
- `DeviceRiskClass` ≠ RiskManagement
