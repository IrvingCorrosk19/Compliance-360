# Manual Interactivo por Rol — Compliance 360

## Cómo abrir

1. Abra `docs/user-manual/index.html` con doble clic (protocolo `file://`).
2. Elija tema Claro / Oscuro / Sistema.
3. Seleccione su rol y siga los capítulos + tutoriales.
4. Practique en el **Simulador guiado** (no requiere API).

## App real (laboratorio)

- URL: `http://localhost:5272`
- Login: `#/login`
- Consola RA: `#/regulatory`
- Alert Center: `#/alert-center` (campana del encabezado)

## Contenido

- 10 roles: TAC + Notification Administrator + 8 RA/QM
- Pantallas, campos y botones alineados a `regulatory-affairs.js`, `alert-center.js` y `RoleCatalog`
- Flujo certificado Prep → … → CT/RS → Renovación
- Alert Center: Inbox (todos con `NOTIFICATION.READ`) · configuración (Notification Administrator)
- Buscador, glosario, errores SoD/401, progreso en `localStorage`

## Roles clave de Alert Center

| Rol | Qué hace en Alert Center |
|-----|--------------------------|
| **Notification Administrator** | Configura plantillas, reglas, destinatarios, scheduler, providers y operaciones |
| Roles RA / QM / Viewer | Usan el **Inbox** (leer, marcar, archivar, favoritos) |
| Tenant Administrator | Asigna el rol Notification Administrator (SoD vs Storage Admin) |
| Storage Administrator | **No** administra notificaciones/SMTP |

## No inventa

No documenta pantallas, permisos ni flujos fuera del modelo certificado actual.
