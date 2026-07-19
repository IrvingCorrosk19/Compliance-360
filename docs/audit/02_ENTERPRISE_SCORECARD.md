# Enterprise scorecard

## Resultado

**Total ponderado: 64.5/100.** El total pondera más seguridad, operación, regulación y preparación productiva que apariencia o volumen documental.

| Dimensión | Score | Evidencia principal |
|---|---:|---|
| Arquitectura | 78 | Capas Domain/Application/Infrastructure/Web y dependencias dirigidas |
| Código | 73 | Build limpio y 282 pruebas; archivos JS y servicios de gran tamaño |
| Clean Code | 68 | Nombres claros; `dotnet format --verify-no-changes` falla |
| SOLID | 76 | Interfaces y DI; algunos servicios/endpoints concentran responsabilidades |
| DDD | 82 | Agregados, métodos de dominio, estados y eventos regulatorios |
| Separación de responsabilidades | 80 | Proyectos separados; SPA monolítica en `app.js` |
| Escalabilidad | 62 | Stateless web y PostgreSQL; sin prueba de carga ni workers dedicados |
| Mantenibilidad | 67 | Tipado backend y tests; frontend Vanilla JS voluminoso |
| Performance | 68 | Índices y consultas async; sin SLO ni benchmark representativo |
| Seguridad | 62 | JWT ligado a sesión y uploads validados; HTTP público, root containers, sin RLS |
| UX | 67 | Flujos completos; alta densidad y curva de aprendizaje |
| UI | 70 | Visual SaaS coherente; inconsistencias y textos hardcoded |
| Responsive | 84 | 9/9 roles superan prueba responsive |
| Accesibilidad | 61 | `focus-visible` y reduced motion parciales; sin certificación WCAG |
| Consistencia visual | 72 | Tokens/patrones compartidos; múltiples estilos y paneles especializados |
| Experiencia de usuario | 64 | Feedback y modales; navegación compleja y terminología regulatoria |
| Flujos | 70 | Flujos reales ejercitados; coexistencia V1/V2 aumenta riesgo |
| RBAC | 79 | Políticas granulares; Platform Admin productivo sin rol |
| Segregación de funciones | 82 | Pruebas negativas y gates; dependencia fuerte de catálogo/migración |
| Auditoría | 84 | Eventos y triggers append-only |
| Logging | 78 | Serilog, correlation y middleware; retención/alertamiento no evidenciados |
| Trazabilidad | 85 | Timeline, historial, hashes y eventos |
| Workflow | 76 | V2 controlado; complejidad y compatibilidad V1 |
| Regulatory Affairs | 73 | Cobertura funcional amplia; sin validación formal 21 CFR Part 11/GxP |
| Calidad del código | 71 | Build/tests sólidos; deuda de formato y cobertura |
| Cobertura | 48 | Reporte global 6.5%; gate de ramas 89.81% no alcanzado |
| Testing | 78 | xUnit + Playwright con trazas; flakiness TAC |
| Observabilidad | 72 | live/ready/notifications; faltan SLO, métricas y alertas demostradas |
| CI/CD | 61 | Workflow y gates; gate actual no pasa y despliegue conserva pasos manuales |
| Infraestructura | 44 | Root, sin resource limits/read-only, backup scheduler ausente |
| Documentación | 83 | Volumen y trazabilidad altos; duplicación/históricos reducen señal |
| Facilidad de uso | 62 | Usuarios expertos pueden operar; onboarding nuevo es exigente |
| Preparación para producción | 52 | VPS responde; identidad privilegiada, backup e infraestructura bloquean |
| Nivel Enterprise | 66 | Buen dominio funcional, controles operativos incompletos |
| Calidad general | 65 | Profesional avanzado, no clase mundial |

## Lectura del score

- **90–100:** clase mundial verificable.
- **80–89:** Enterprise maduro con riesgos acotados.
- **70–79:** profesional avanzado.
- **60–69:** producto comercial viable con brechas Enterprise.
- **<60:** no apto para contexto regulado crítico.

La media simple de dimensiones no se usa como total porque permitiría que UI/documentación compensaran controles críticos de seguridad y continuidad.
