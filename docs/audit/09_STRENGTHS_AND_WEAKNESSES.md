# Fortalezas, debilidades, riesgos y oportunidades

## Fortalezas

- Dominio Regulatory Affairs profundo: no se limita a CRUD.
- Arquitectura .NET por capas con DDD y DI.
- 282/282 pruebas .NET pasan.
- RBAC granular y SoD cubierto por políticas y gates.
- Auditoría e historial con protección append-only en PostgreSQL.
- Versionado documental, hashes y ownership de archivos.
- Responsive/i18n supera prueba en nueve roles.
- Health checks separados y logging estructurado.
- 163/163 tablas tienen PK; 0 índices inválidos.
- Dependencias sin vulnerabilidades reportadas en el corte.

## Debilidades

- Frontend Vanilla JS monolítico y con textos hardcoded residuales.
- V1 y V2 regulatorios coexisten.
- Cobertura global observada 6.5%; infraestructura ~1.67%.
- Gate de cobertura de ramas no alcanza 90%.
- Formato no limpio en cuatro archivos.
- Platform Administrator productivo sin rol.
- Contenedores root, sin read-only ni resource limits.
- Sin RLS y sin backup scheduler demostrado.
- Flakiness TAC en sincronización UI/E2E.
- Documentación abundante pero fragmentada y con históricos competitivos.

## Riesgos

| Riesgo | Impacto | Probabilidad | Mitigación | Prioridad |
|---|---|---|---|---|
| Fuga cross-tenant por filtro omitido | Crítico | Media | RLS/guard central + IDOR tests | P0 |
| Credenciales sobre HTTP `:8085` | Crítico | Alta si se usa | Deshabilitar/redirigir HTTPS | P0 |
| Recuperación no demostrada | Crítico | Media | PITR + restore drills | P0 |
| Bypass por rutas V1 | Alto | Media | Retiro de mutaciones V1 | P0 |
| Escape de contenedor root | Alto | Baja/Media | Non-root/cap drop/read-only | P0 |
| Cambio regulatorio no detectado | Alto | Media | Cobertura Infrastructure/E2E y traceability | P0 |
| Drift permiso-menu-policy-migration | Alto | Media | Catálogo único y tests executable | P1 |
| Flaky E2E oculta regresión | Medio | Media | Esperas por response/state y quarantine con SLA | P1 |

## Oportunidades

- Posicionarlo como plataforma regional de Regulatory Operations para empresas medianas.
- Convertir workflow y matriz de autoridad en configuración versionada por jurisdicción.
- Ofrecer paquetes validados por industria con evidencia IQ/OQ/PQ.
- Añadir analytics de ciclo, cuellos de botella y riesgo de vencimiento.
- Construir integraciones con Veeva/SAP/ServiceNow en lugar de competir frontalmente al inicio.
- Modularizar frontend y publicar design system accesible.

## Qué transmite

Transmite un producto comercial serio construido con conocimiento funcional. Su mayor distancia frente a clase mundial no está en cantidad de pantallas, sino en confiabilidad operacional, validación independiente, experiencia simplificada y controles de plataforma.
