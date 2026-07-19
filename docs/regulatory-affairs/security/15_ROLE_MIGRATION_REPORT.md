# 15 — Role Migration Report (lab aplicado)

| Antes (DB pre-restart) | Después (DB post-EnsureTenantRoles) |
|------------------------|--------------------------------------|
| 4 roles RA | 7 roles RA |
| TAC con APPROVE/SUBMIT | TAC solo CONFIGURE/SOD.MANAGE/READ |
| Specialist con SUBMIT | Specialist sin SUBMIT |
| Reviewer con APPROVE | Reviewer con REVIEW |
| Permisos sin APPROVE_FOR_SUBMISSION | Permisos atómicos v1 presentes |

Usuarios lab creados/verificados vía TAC API. Tokens regenerados por login.
