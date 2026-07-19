# 13 — Pruebas negativas de seguridad

## Ejecutadas con evidencia

- Viewer: metadata, corrección, evidencia, reopen, override y archive → HTTP 403.
- TAC y RA-ADM: review, approve y override → HTTP 403.
- Revisión obsoleta → HTTP 409 con detalle `Expected 1, current 2`.
- Evidencia fuera del scope de corrección → rechazada sin persistencia.
- Upload unit tests: extensión ejecutable/SVG/HTML, MIME spoofing, firma PDF falsa, path traversal y tamaño >25 MB → rechazados.
- Perfil REGUTRACK: solo contenedor `.xlsx` válido.

## Cubiertas por pruebas focalizadas

Las pruebas de `FileUploadSecurityTests.cs`, identidad y workflow forman parte de la ejecución focalizada .NET **62/62 PASS** informada.

## Cierre productivo

- Rate limiter validado detrás del proxy: 0 falsos positivos 429 durante 72 escenarios; protección permanece activa por endpoint/IP o sesión.
- Negativos RBAC/SoD y aislamiento multi-tenant repetidos con asignaciones productivas.
- Límites y firma de uploads cubiertos por .NET y carga documental E2E; 610 archivos disponibles persistidos.
- HTTPS/HSTS/CSP/X-Frame-Options/X-Content-Type-Options verificados; puerto 8085 no expuesto públicamente.
- Conflictos duplicados devuelven 409, denegaciones 403 y cancelaciones del cliente no contaminan métricas 5xx.

## Veredicto

**PASS — cobertura negativa productiva completada.**
