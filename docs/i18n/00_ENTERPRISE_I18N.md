# Enterprise i18n — deliverables

## Architecture

| Piece | Path |
|-------|------|
| Translation service | `wwwroot/i18n.js` |
| Spanish catalog | `wwwroot/locales/es.json` |
| English catalog | `wwwroot/locales/en.json` |
| SPA bootstrap | `wwwroot/app.js` (`t()`, selector, reverse-map) |
| RA module | `wwwroot/regulatory-affairs.js` |
| User preference API | `GET/PUT /api/v1/tenants/{id}/users/me/preferences` |
| DB column | `users.PreferredLanguage` |

## Behavior

- Languages: `es` (default) · `en`
- Detect: localStorage → sessionStorage → cookie `c360.language` → browser
- Persist guest: localStorage + cookie
- Persist authenticated: same + `PreferredLanguage` in DB
- Selector: 🇪🇸 Español / 🇺🇸 English (sidebar + login)
- Instant switch without restart
- Load only active locale JSON (+ peer for reverse-map); in-memory cache
- Dates via `I18n.formatDate` (`es-PA` / `en-US`)

## Reports

1. `docs/i18n/01_HARDCODED_STRINGS_SCAN.md` — initial inventory
2. `docs/i18n/02_LOCALE_KEY_CATALOG.md` — namespace counts
3. `docs/i18n/03_KEY_MAPPING.json` — key ↔ text mapping
4. `docs/i18n/05_TRANSLATED_COVERAGE.md` — coverage after migration
5. `docs/i18n/06_PENDING_STRINGS.md` — remaining candidates (dead QMS modules / roadmap screens)

## Adding a language tomorrow

1. Copy `locales/es.json` → `locales/pt.json` (or `fr`, `de`, …)
2. Translate values
3. Add code to `SUPPORTED_LANGUAGES` in `i18n.js` and one `<option>` in `languageSelectorHtml`
4. No business logic changes
