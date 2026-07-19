# 91 — Final Role Functional Certification

**Date:** 2026-07-15  
**SoD baseline:** GO  
**Functional replacement:** pending (see 95)

| Role | Cases designed (suite estimate) | Executed in this cert | PASS | FAIL | BLOCKED | Critical open | High open | Certified |
|------|----------------------------------|-----------------------|------|------|---------|---------------|-----------|-----------|
| Regulatory Administrator | subset AUTH + bootstrap + import | Yes (API) | Yes | 0 | — | — | — | **PARTIAL** |
| Regulatory Manager | obs + external approve + dash | Yes | Yes | 0 | — | — | — | **PARTIAL** |
| Regulatory Specialist | product/dossier/pack path | Yes | Yes | 0 | — | — | — | **PARTIAL** |
| Regulatory Reviewer | reject/accept criticals | Yes | Yes | 0 | — | — | — | **PARTIAL** |
| Regulatory Approver | internal approve; no submit | Yes | Yes | 0 | — | — | — | **PARTIAL** |
| Regulatory Submitter | submit after internal gate | Yes | Yes | 0 | — | — | — | **PARTIAL** |
| Regulatory Viewer | read; create deny | Yes | Yes | 0 | — | — | — | **PARTIAL** |
| Quality Manager | login + limited RA | Login only | — | — | Incomplete UI catalog | — | — | **NO** |
| Tenant Administrator | deny operational approve | Yes (TAC-NO-APPR) | Yes | 0 | — | — | — | **PARTIAL** |

**Note:** Role SoD certification remains **GO**. This table is **functional replacement** certification — requires full suite execution per role (DEF-0004). No role is marked Certified=YES for REGUTRACK replacement.
