# 04 — Template Center Enterprise

## 1. Alcance

Centro no-code, multicanal, multidioma y regulado para:

- Email HTML y texto.
- In-app.
- SMS.
- WhatsApp.
- Push.
- Teams y Slack.
- Webhook y API payloads.

No se declara un canal soportado hasta existir adapter, provider, callbacks cuando apliquen y pruebas reales.

## 2. Módulos

1. Biblioteca de templates.
2. Editor visual.
3. Editor de HTML seguro para usuarios autorizados.
4. Editor de texto y payload estructurado.
5. Catálogo de variables.
6. Preview por locale/canal/dispositivo.
7. Test send a allowlist.
8. Versiones, diff y aprobaciones.
9. Traducciones y cobertura.
10. Branding, temas, layouts y bloques.
11. Assets y attachments.
12. Uso e impacto por reglas/campañas.

## 3. Wizard

1. Metadata: código, categoría, owner, clasificación y canales.
2. Locale base y estrategia de fallback.
3. Contenido por canal.
4. Variables y datos de prueba.
5. Branding/layout.
6. Texto alternativo, accesibilidad y dark mode.
7. Attachments permitidos.
8. Preview.
9. Lint de seguridad/calidad.
10. Enviar a revisión.

## 4. Estados

`Draft → InReview → Approved → Published → Superseded → Retired`.

- Published es inmutable.
- Los cambios generan versión nueva.
- Reglas apuntan a versión exacta o a alias aprobado con política explícita.
- Rollback crea un deployment a una versión previa; no muta historia.
- Publicación futura usa vigencia `EffectiveFromUtc`.

## 5. Variables

Sintaxis canónica:

```text
{{ dossier.code }}
{{ recipient.displayName }}
{{ formatDate(dossier.expirationDate, recipient.locale) }}
```

Capacidades:

- tipado;
- required/default;
- formato por canal;
- locale/timezone;
- classification PII;
- masking;
- preview data;
- deprecation y replacement;
- lineage al schema del evento.

No permitido:

- acceso a propiedades arbitrarias;
- reflection;
- ejecución de código;
- requests de red;
- secretos;
- funciones no registradas;
- HTML raw por defecto.

## 6. Render seguro

- Escaping contextual HTML/attribute/URL/JSON.
- Sanitización allowlist.
- URLs con esquemas permitidos.
- CSP compatible y sin scripts.
- Subject/header protegido contra CRLF.
- JSON generado mediante serializer, no concatenación.
- HTML y text alternative obligatorios para email regulado.
- Attachments por referencia segura; no paths.
- Tracking links y pixels desactivables por tenant/jurisdicción.
- Tokens desconocidos provocan validación fallida; no se envía contenido incompleto.

## 7. Editor visual

Bloques:

- Header, logo, heading, paragraph.
- Button/link.
- Table/key-value.
- Alert banner.
- Timeline.
- Signature/footer.
- Regulatory disclaimer.
- Attachment list.
- Conditional block con expresiones permitidas.
- Repeating block con límites.

El drag-and-drop tiene alternativas de teclado: añadir, subir, bajar, duplicar y eliminar.

## 8. Branding y temas

Configurables por tenant:

- logos light/dark;
- paleta semántica;
- tipografía allowlisted;
- spacing/radius;
- header/footer;
- dirección/contacto;
- firmas;
- disclaimers por país/autoridad;
- dominio/base URL;
- temas por marca/producto si la política lo permite.

Se valida contraste WCAG 2.2 AA y compatibilidad de clientes de email.

## 9. Idiomas

- Locale base por template.
- Fallback explícito por canal.
- Cobertura visible.
- Workflow independiente o conjunto para traducciones.
- Variables conservan significado/tipo.
- Preview RTL.
- Formatos ICU para pluralización cuando corresponda.
- No se traduce contenido legal automáticamente sin revisión humana.

## 10. Preview y pruebas

Preview:

- fixtures guardados;
- entidad real con masking y permiso;
- viewport desktop/móvil;
- light/dark;
- HTML/text;
- locale/timezone;
- clientes de email soportados mediante snapshots.

Test send:

- solo allowlist tenant;
- banner `[TEST]`;
- no modifica métricas productivas;
- conserva audit trail;
- provider sandbox o explícitamente habilitado.

Lint:

- variables desconocidas/faltantes;
- links inválidos;
- contraste;
- alt text;
- tamaño;
- HTML prohibido;
- JSON schema;
- locale faltante;
- contenido sensible en canal no permitido;
- URLs no HTTPS salvo sandbox.

## 11. Aprobación y firma

Contenido regulado exige:

- autor distinto de aprobador;
- motivo/caso de uso;
- diff semántico;
- preview fixtures obligatorios;
- test evidence;
- firma electrónica si la política tenant lo exige;
- hash SHA-256 de contenido y metadata;
- vigencia.

## 12. APIs

- `GET/POST /api/v2/.../templates`
- `GET/PATCH /templates/{id}`
- `POST /templates/{id}/versions`
- `POST /template-versions/{id}/validate`
- `POST /template-versions/{id}/preview`
- `POST /template-versions/{id}/test-send`
- `POST /template-versions/{id}/submit`
- `POST /template-versions/{id}/approve`
- `POST /template-versions/{id}/reject`
- `POST /template-versions/{id}/publish`
- CRUD `/variables`, `/layouts`, `/blocks`, `/themes`, `/assets`
- `GET /templates/{id}/usage`

Preview/test tienen rate limit y audit.

## 13. Permisos

- `ALERT.TEMPLATE.READ`
- `ALERT.TEMPLATE.READ_SENSITIVE`
- `ALERT.TEMPLATE.CREATE`
- `ALERT.TEMPLATE.UPDATE`
- `ALERT.TEMPLATE.CLONE`
- `ALERT.TEMPLATE.SUBMIT`
- `ALERT.TEMPLATE.REVIEW`
- `ALERT.TEMPLATE.APPROVE`
- `ALERT.TEMPLATE.PUBLISH`
- `ALERT.TEMPLATE.RETIRE`
- `ALERT.TEMPLATE.TEST`
- `ALERT.VARIABLE.MANAGE`
- `ALERT.BRANDING.MANAGE`

## 14. Compatibilidad

`NotificationTemplate` se mantiene como cabecera. Las versiones legacy se migran a `notification_template_versions`:

- versión 1;
- locale existente;
- status `LegacyDraft` si no hay aprobación demostrable;
- content hash;
- sin inventar actor/aprobación.

El `NotificationTemplateEngine` pasa a adapter legacy. El nuevo renderer sirve solo versiones validadas.

## 15. Pruebas obligatorias

- escaping y XSS;
- CRLF/header injection;
- JSON correctness;
- variables required/default/null;
- locales, RTL, timezone y DST;
- version immutability y SoD;
- preview/test allowlist;
- attachments y límites;
- WCAG del HTML;
- snapshots por canal;
- compatibilidad legacy;
- load/concurrency de render;
- no exposición de PII/secrets en logs.

