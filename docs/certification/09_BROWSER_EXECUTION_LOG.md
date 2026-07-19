# 09 — Browser Execution Log (Playwright)

| Campo | Valor |
|---|---|
| Fecha | 2026-07-17T19:16:05.953Z |
| Runner | Playwright Test (Chromium, 1 worker, serial) |
| Spec | `e2e/tests/role-e2e-certification.spec.ts` |
| Resultado | **6 passed** (RA Specialist, Reviewer, Approver, Submitter, Manager, Viewer) |
| Regresión asociada | `manual-roles-browser.spec.ts` + `manual-workflow-cert.spec.ts` → **2 passed** |

## Chronology por rol (pasos reales)

### RA Specialist (ra.spec@cert.local)

```
[+  1037 ms] PASS  LOGIN                         shell visible
[+  1051 ms] PASS  SHELL-SIDEBAR                 [dashboard, regulatory]
[+  1100 ms] PASS  SHELL-NAVBAR                  chip="RA Specialist Regulatory Specialist" tenant="Tenant: 82af3877..."
[+  2652 ms] PASS  DASHBOARD-KPI                 bloques=8
[+  3188 ms] PASS  RA-TABS                       [dashboard, portfolio, pipeline, dossiers, registrations, manufacturers]
[+  4785 ms] PASS  CREATE-PRODUCT-DOSSIER        riesgo=[A,B,C] país=PA producto=OK expediente=Planning
[+  7918 ms] PASS  UPLOAD-REQUIREMENTS           requisitos Received=3
[+  7977 ms] PASS  EDIT-DATES                    http=200
[+ 12726 ms] PASS  SEND-TO-REVIEW                status=ReadyForSubmission
[+ 13031 ms] PASS  SOD-SELF-REVIEW               http=400
[+ 13402 ms] PASS  NO-APPROVE-BTN                botones=0
[+ 13412 ms] PASS  NO-SUBMIT-BTN                 botones=0
[+ 13428 ms] PASS  NEG-API-APPROVE-INT           http=403
[+ 13443 ms] PASS  NEG-API-SUBMIT                http=403
[+ 13459 ms] PASS  NEG-API-APPROVE-EXT           http=403
[+ 13476 ms] PASS  NEG-API-BOOTSTRAP             http=403
[+ 13496 ms] PASS  NEG-API-LICENSE               http=403
[+ 14757 ms] PASS  NEG-URL-tenant-administration  Acceso denegado renderizado
[+ 16032 ms] PASS  NEG-URL-security              Acceso denegado renderizado
[+ 17317 ms] PASS  NEG-URL-audit-trail           Acceso denegado renderizado
[+ 18381 ms] PASS  HISTORY                       eventos=6
[+ 18515 ms] PASS  LOGOUT                        login visible
TOTAL RA-SPEC: 18515 ms — PASS
```

### RA Reviewer (ra.rev@cert.local)

```
[+   698 ms] PASS  LOGIN                         shell visible
[+   720 ms] PASS  SHELL-SIDEBAR                 [dashboard, regulatory]
[+   763 ms] PASS  SHELL-NAVBAR                  chip="RA Reviewer Regulatory Reviewer" tenant="Tenant: 82af3877..."
[+  2326 ms] PASS  DASHBOARD-KPI                 bloques=8
[+  2989 ms] PASS  RA-TABS                       [dashboard, pipeline, dossiers, registrations]
[+  3960 ms] PASS  OPEN-DOSSIER                  aceptar=22 rechazar=22
[+  5161 ms] PASS  RETURN-REQ                    status=Rejected
[+ 14991 ms] PASS  REVIEW-ACCEPT-ALL             críticos pendientes=0
[+ 15373 ms] PASS  NO-APPROVE-BTN                botones=0
[+ 15385 ms] PASS  NO-SUBMIT-BTN                 botones=0
[+ 15404 ms] PASS  NEG-API-APPROVE-INT           http=403
[+ 15420 ms] PASS  NEG-API-SUBMIT                http=403
[+ 15436 ms] PASS  NEG-API-CREATE-PRODUCT        http=403
[+ 15453 ms] PASS  NEG-API-CREATE-DOSSIER        http=403
[+ 15472 ms] PASS  NEG-API-TRANSITION            http=403
[+ 15487 ms] PASS  NEG-API-OBSERVE               http=403
[+ 16754 ms] PASS  NEG-URL-tenant-administration  Acceso denegado renderizado
[+ 16766 ms] PASS  NEG-JS-ra-new-product         botón ausente del DOM
[+ 16931 ms] PASS  LOGOUT                        login visible
TOTAL RA-REV: 16931 ms — PASS
```

### RA Approver (ra.appr@cert.local)

```
[+  1343 ms] PASS  LOGIN                         shell visible
[+  1357 ms] PASS  SHELL-SIDEBAR                 [dashboard, regulatory]
[+  1387 ms] PASS  SHELL-NAVBAR                  chip="RA Approver Regulatory Approver" tenant="Tenant: 82af3877..."
[+  2962 ms] PASS  DASHBOARD-KPI                 bloques=8
[+  3518 ms] PASS  RA-TABS                       [dashboard, pipeline, dossiers, registrations]
[+  4567 ms] PASS  VALIDATE-STATE                status=ReadyForSubmission indicadores=35
[+  4687 ms] PASS  NO-EDIT-RESTRICTED            http=[403,403,403]
[+  5167 ms] PASS  APPROVE-INTERNAL              status=ApprovedForSubmission
[+  5557 ms] PASS  NO-SUBMIT-BTN                 botones=0
[+  5583 ms] PASS  NEG-API-SUBMIT                http=403
[+  5604 ms] PASS  NEG-API-APPROVE-EXT           http=403
[+  5642 ms] PASS  NEG-API-CREATE-PRODUCT        http=403
[+  5668 ms] PASS  NEG-API-OBSERVE               http=403
[+  6956 ms] PASS  NEG-URL-tenant-administration  Acceso denegado renderizado
[+  6966 ms] PASS  NEG-JS-ra-submit              botón ausente del DOM
[+  7115 ms] PASS  LOGOUT                        login visible
TOTAL RA-APPR: 7115 ms — PASS
```

### RA Submitter (ra.sub@cert.local)

```
[+   986 ms] PASS  LOGIN                         shell visible
[+   998 ms] PASS  SHELL-SIDEBAR                 [dashboard, regulatory]
[+  1026 ms] PASS  SHELL-NAVBAR                  chip="RA Submitter Regulatory Submitter" tenant="Tenant: 82af3877..."
[+  2589 ms] PASS  DASHBOARD-KPI                 bloques=8
[+  3233 ms] PASS  RA-TABS                       [dashboard, pipeline, dossiers, registrations]
[+  4056 ms] PASS  FIND-APPROVED                 status=ApprovedForSubmission
[+  5372 ms] PASS  SUBMIT                        status=Submitted submittedOn=2026-07-17
[+  5524 ms] PASS  NO-EDIT-LOCKED                http=[403,403,403]
[+  5542 ms] PASS  NEG-API-APPROVE-INT           http=403
[+  5572 ms] PASS  NEG-API-APPROVE-EXT           http=403
[+  5621 ms] PASS  NEG-API-OBSERVE               http=403
[+  5665 ms] PASS  NEG-API-CREATE-DOSSIER        http=403
[+  7030 ms] PASS  NEG-URL-tenant-administration  Acceso denegado renderizado
[+  7050 ms] PASS  NEG-JS-ra-approve-internal    botón ausente del DOM
[+  7231 ms] PASS  LOGOUT                        login visible
TOTAL RA-SUB: 7231 ms — PASS
```

### RA Manager (decisión externa / CT-RS) (ra.mgr@cert.local)

```
[+  1009 ms] PASS  LOGIN                         shell visible
[+  3955 ms] PASS  EXTERNAL-DECISION             status=Closed ctrs=MQ-E2E-61753
[+  4083 ms] PASS  LOGOUT                        login visible
TOTAL RA-MGR: 4083 ms — PASS
```

### RA Viewer (ra.view@cert.local)

```
[+   959 ms] PASS  LOGIN                         shell visible
[+   970 ms] PASS  SHELL-SIDEBAR                 [dashboard, regulatory]
[+   998 ms] PASS  SHELL-NAVBAR                  chip="RA Viewer Regulatory Viewer" tenant="Tenant: 82af3877..."
[+  2552 ms] PASS  DASHBOARD-KPI                 bloques=8
[+  3095 ms] PASS  RA-TABS                       [dashboard, portfolio, pipeline, dossiers, registrations, alerts]
[+  8734 ms] PASS  BROWSE-ALL                    dashboard=OK portfolio=OK pipeline=OK dossiers=OK registrations=OK alerts=OK
[+ 10164 ms] PASS  OPEN-RECORD                   botones mutación=0 aviso sin acciones=true
[+ 10337 ms] PASS  SEARCH-FILTER                 http=[200,200,200]
[+ 10367 ms] PASS  NO-EXPORT                     botones export=0
[+ 10394 ms] PASS  NEG-API-CREATE                http=403
[+ 10433 ms] PASS  NEG-API-CREATE-DOSSIER        http=403
[+ 10461 ms] PASS  NEG-API-EDIT                  http=403
[+ 10497 ms] PASS  NEG-API-REVIEW                http=403
[+ 10523 ms] PASS  NEG-API-APPROVE-INT           http=403
[+ 10542 ms] PASS  NEG-API-SUBMIT                http=403
[+ 10562 ms] PASS  NEG-API-APPROVE-EXT           http=403
[+ 10580 ms] PASS  NEG-API-STATE                 http=403
[+ 10596 ms] PASS  NEG-API-IMPORT                http=403
[+ 10610 ms] PASS  NEG-API-OBSERVE               http=403
[+ 10626 ms] PASS  NEG-API-SOD                   http=403
[+ 10641 ms] PASS  NEG-API-MFR                   http=403
[+ 10654 ms] PASS  NEG-API-LICENSE               http=403
[+ 10679 ms] PASS  NEG-API-TAMPER-ID             http=403
[+ 11976 ms] PASS  NEG-URL-tenant-administration  Acceso denegado renderizado
[+ 13234 ms] PASS  NEG-URL-security              Acceso denegado renderizado
[+ 14490 ms] PASS  NEG-URL-audit-trail           Acceso denegado renderizado
[+ 15780 ms] PASS  NEG-URL-superadmin-platform   Acceso denegado renderizado
[+ 17067 ms] PASS  NEG-URL-configuration         Acceso denegado renderizado
[+ 17075 ms] PASS  NEG-JS-ra-new-product         botón ausente del DOM
[+ 17082 ms] PASS  NEG-JS-ra-add-mfr             botón ausente del DOM
[+ 17090 ms] PASS  NEG-JS-ra-add-lic             botón ausente del DOM
[+ 17103 ms] PASS  NEG-JS-FETCH                  http=403
[+ 17226 ms] PASS  LOGOUT                        login visible
TOTAL RA-VIEW: 17226 ms — PASS
```

## Capturas generadas

Total: **38** capturas en `docs/certification/evidence/role-e2e/`

| Archivo |
|---|
| `ra-appr-approve-internal.png` |
| `ra-appr-dashboard-kpi.png` |
| `ra-appr-login.png` |
| `ra-appr-neg-url-tenant-administration.png` |
| `ra-appr-ra-tabs.png` |
| `ra-appr-validate-state.png` |
| `ra-mgr-external-decision.png` |
| `ra-mgr-login.png` |
| `ra-rev-dashboard-kpi.png` |
| `ra-rev-login.png` |
| `ra-rev-neg-url-tenant-administration.png` |
| `ra-rev-open-dossier.png` |
| `ra-rev-ra-tabs.png` |
| `ra-rev-review-accept-all.png` |
| `ra-spec-create-product-dossier.png` |
| `ra-spec-dashboard-kpi.png` |
| `ra-spec-login.png` |
| `ra-spec-neg-url-audit-trail.png` |
| `ra-spec-neg-url-security.png` |
| `ra-spec-neg-url-tenant-administration.png` |
| `ra-spec-ra-tabs.png` |
| `ra-spec-send-to-review.png` |
| `ra-spec-upload-requirements.png` |
| `ra-sub-dashboard-kpi.png` |
| `ra-sub-login.png` |
| `ra-sub-neg-url-tenant-administration.png` |
| `ra-sub-ra-tabs.png` |
| `ra-sub-submit.png` |
| `ra-view-browse-all.png` |
| `ra-view-dashboard-kpi.png` |
| `ra-view-login.png` |
| `ra-view-neg-url-audit-trail.png` |
| `ra-view-neg-url-configuration.png` |
| `ra-view-neg-url-security.png` |
| `ra-view-neg-url-superadmin-platform.png` |
| `ra-view-neg-url-tenant-administration.png` |
| `ra-view-open-record.png` |
| `ra-view-ra-tabs.png` |

## Log crudo de Playwright

```
�� 
 R u n n i n g   6   t e s t s   u s i n g   1   w o r k e r  
  
  
  
 n o d e . e x e   :   ( n o d e : 5 7 4 8 8 )   W a r n i n g :   T h e   ' N O _ C O L O R '   e n v   i s   i g n o r e d   d u e   t o   t h e   ' F O R C E _ C O L O R '   e n v   b e i n g   s e t .  
 A t   l i n e : 1   c h a r : 1  
 +   &   " C : \ P r o g r a m   F i l e s \ n o d e j s / n o d e . e x e "   " C : \ P r o g r a m   F i l e s \ n o d e j s / n o d e _ m o   . . .  
 +   ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~  
         +   C a t e g o r y I n f o                     :   N o t S p e c i f i e d :   ( ( n o d e : 5 7 4 8 8 )   W a . . .   e n v   b e i n g   s e t . : S t r i n g )   [ ] ,   R e m o t e E x c e p t i o n  
         +   F u l l y Q u a l i f i e d E r r o r I d   :   N a t i v e C o m m a n d E r r o r  
    
 ( U s e   ` n o d e   - - t r a c e - w a r n i n g s   . . . `   t o   s h o w   w h e r e   t h e   w a r n i n g   w a s   c r e a t e d )  
 [ 1 / 6 ]   [ c h r o m i u m ]   �� Q%  t e s t s \ r o l e - e 2 e - c e r t i f i c a t i o n . s p e c . t s : 2 0 6 : 7   �� Q%  R o l e   E 2 E   c e r t i f i c a t i o n   �� Q%  R A   S p e c i a l i s t   �� �   r e c o r r i d o   E 2 E   c o m p l e t o  
 [ 2 / 6 ]   [ c h r o m i u m ]   �� Q%  t e s t s \ r o l e - e 2 e - c e r t i f i c a t i o n . s p e c . t s : 3 2 7 : 7   �� Q%  R o l e   E 2 E   c e r t i f i c a t i o n   �� Q%  R A   R e v i e w e r   �� �   r e c o r r i d o   E 2 E   c o m p l e t o  
 [ 3 / 6 ]   [ c h r o m i u m ]   �� Q%  t e s t s \ r o l e - e 2 e - c e r t i f i c a t i o n . s p e c . t s : 4 1 5 : 7   �� Q%  R o l e   E 2 E   c e r t i f i c a t i o n   �� Q%  R A   A p p r o v e r   �� �   r e c o r r i d o   E 2 E   c o m p l e t o  
 [ 4 / 6 ]   [ c h r o m i u m ]   �� Q%  t e s t s \ r o l e - e 2 e - c e r t i f i c a t i o n . s p e c . t s : 4 7 8 : 7   �� Q%  R o l e   E 2 E   c e r t i f i c a t i o n   �� Q%  R A   S u b m i t t e r   �� �   r e c o r r i d o   E 2 E   c o m p l e t o  
 [ 5 / 6 ]   [ c h r o m i u m ]   �� Q%  t e s t s \ r o l e - e 2 e - c e r t i f i c a t i o n . s p e c . t s : 5 4 0 : 7   �� Q%  R o l e   E 2 E   c e r t i f i c a t i o n   �� Q%  R A   M a n a g e r   �� �   d e c i s i %%n   e x t e r n a   ( r e g i s t r o   d e   t r %� m i t e / r e s o l u c i %%n / f e c h a s )  
 [ 6 / 6 ]   [ c h r o m i u m ]   �� Q%  t e s t s \ r o l e - e 2 e - c e r t i f i c a t i o n . s p e c . t s : 5 7 7 : 7   �� Q%  R o l e   E 2 E   c e r t i f i c a t i o n   �� Q%  R A   V i e w e r   �� �   r e c o r r i d o   E 2 E   c o m p l e t o   ( s o l o   l e c t u r a )  
     6   p a s s e d   ( 1 . 9 m )  
 
```

**VEREDICTO DEL DOCUMENTO 09: PASS**
