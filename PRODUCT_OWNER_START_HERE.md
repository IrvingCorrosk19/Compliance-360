# Compliance 360 - Product Owner Start Here

Esta guía es tu punto de partida para aprender Compliance 360 como lo hará un cliente real.

No empieces creando datos al azar. Recorre el sistema en este orden para entender qué configura cada cosa y por qué existe.

## Antes De Empezar

La aplicación quedó limpia. Solo existe el acceso inicial de SuperAdmin.

Debes esperar ver una plataforma sin tenants de cliente, sin empresas, sin usuarios operativos, sin documentos, sin proveedores, sin auditorías, sin riesgos, sin indicadores y sin reportes creados.

Eso es correcto.

## Paso 1 - Iniciar Sesión

Qué hacer:

1. Abre `http://localhost:5272`.
2. Entra con el usuario inicial `admin@compliance360.local`.
3. Usa la contraseña temporal entregada para esta preparación.
4. Si el sistema solicita cambiar contraseña, cámbiala antes de continuar.

Por qué hacerlo:

El SuperAdmin es el operador inicial de la plataforma. No representa a una empresa cliente; representa al dueño SaaS de Compliance 360.

Qué ocurre internamente:

El sistema valida el usuario, carga el tenant mínimo de plataforma, asigna el rol `SuperAdmin` y emite una sesión autenticada con permisos globales.

Qué debes esperar ver:

- Inicio de sesión exitoso.
- Acceso a administración global.
- No debes ver datos reales de clientes.

Errores normales:

- Si escribes mal la contraseña, verás credenciales inválidas.
- Si la sesión expira, vuelve a iniciar sesión.

Errores que no deberían aparecer:

- Error 500.
- Pantalla en blanco.
- Mensaje de base de datos no disponible.
- MFA obligatorio en el primer acceso.

## Paso 2 - Revisar Primero El Estado Del Sistema

Qué hacer:

Entra al centro de administración disponible para SuperAdmin y revisa el dashboard inicial.

Por qué hacerlo:

Antes de crear clientes, necesitas confirmar que estás en una instalación limpia.

Qué ocurre internamente:

La aplicación consulta conteos globales: tenants, usuarios, configuración, salud y actividad.

Qué debes esperar ver:

- Sin clientes configurados por ti.
- Sin documentos.
- Sin auditorías.
- Sin CAPA.
- Sin riesgos.
- Sin indicadores.
- Sin notificaciones enviadas.

Si ves datos de prueba, no continúes: el entorno no está limpio.

## Paso 3 - Configurar Primero Lo Básico

Qué hacer:

Antes de crear operación real, define el orden de implementación:

1. Tenant.
2. Empresa.
3. Branding.
4. Storage.
5. Notificaciones.
6. Usuarios.
7. Roles.
8. Módulos funcionales.

Por qué hacerlo:

Compliance 360 es multitenant. Primero debes crear el contenedor del cliente; después configuras cómo se ve, dónde guarda archivos, cómo envía correos y quién lo usa.

Qué debes esperar ver:

Cada configuración debe quedar asociada al tenant correcto.

## Paso 4 - Qué Nunca Debes Modificar

No modifiques manualmente:

- Migraciones.
- Tablas.
- Relaciones.
- Índices.
- Permisos base del SuperAdmin.
- Hashes de contraseña.
- IDs internos en la base.
- Configuración técnica que no entiendas.

Por qué:

Esos elementos son la base de integridad del producto. Como Product Owner debes aprender el flujo funcional desde la interfaz, no corrigiendo datos por SQL.

## Paso 5 - Crear Tu Primer Tenant

Qué hacer:

Desde administración de tenants, crea el primer tenant como si fuera tu primer cliente real.

Datos recomendados para aprender:

- Nombre legal: empresa real o ficticia clara.
- Nombre comercial: cómo se verá en la plataforma.
- País, moneda y zona horaria.
- Contacto principal.
- Plan o modalidad inicial.

Por qué hacerlo:

El tenant es el espacio aislado del cliente. Todo lo demás vive dentro de ese tenant.

Qué ocurre internamente:

El sistema crea la identidad SaaS del cliente y prepara su configuración base.

Qué debes esperar ver:

Un nuevo tenant listado y accesible para administración.

## Paso 6 - Configurar La Primera Empresa

Qué hacer:

Dentro del tenant, crea la primera empresa operativa.

Por qué hacerlo:

El tenant representa al cliente SaaS; la empresa representa la entidad operativa que usará calidad, auditoría, documentos, proveedores y demás módulos.

Qué debes esperar ver:

La empresa debe quedar asociada al tenant correcto.

Error que no debería aparecer:

No debería crearse una empresa fuera del tenant seleccionado.

## Paso 7 - Configurar Branding

Qué hacer:

Configura nombre visible, colores, correo corporativo, tema y, si aplica, logo.

Por qué hacerlo:

El branding hace que el cliente sienta Compliance 360 como su propia plataforma.

Qué ocurre internamente:

El sistema guarda preferencias visuales del tenant y las usa para presentar la experiencia de usuario.

Qué debes esperar ver:

Los cambios visuales deben reflejarse sin afectar otros tenants.

## Paso 8 - Configurar Storage

Qué hacer:

Configura dónde se guardarán los archivos del tenant.

Para aprendizaje local puedes empezar con storage local. Para una implementación real, valida antes el proveedor productivo que usarás.

Por qué hacerlo:

Documentos, evidencias, adjuntos, auditorías y archivos regulatorios necesitan almacenamiento confiable.

Qué debes esperar ver:

Una configuración activa o lista para probar.

Errores normales:

- Credenciales incompletas.
- Ruta local no disponible.
- Proveedor externo rechazando conexión.

Errores que no deberían aparecer:

- Error 500 sin explicación.
- Archivos guardados en el tenant incorrecto.

## Paso 9 - Configurar Notificaciones

Qué hacer:

Configura un proveedor SMTP. Para Gmail SMTP normalmente necesitas:

- Host: `smtp.gmail.com`.
- Puerto: `587`.
- SSL/TLS activado.
- Usuario Gmail.
- App Password, no la contraseña normal de Gmail.
- From address.
- From name.

Por qué hacerlo:

Compliance 360 necesita enviar avisos de tareas, aprobaciones, vencimientos, auditorías y alertas.

Qué ocurre internamente:

El sistema guarda la configuración del proveedor y la usa para preparar/envíar mensajes del tenant.

Errores normales:

- Gmail bloquea acceso si no usas App Password.
- Credenciales inválidas.
- From address no coincide.

Errores que no deberían aparecer:

- Mensajes enviados desde otro tenant.
- Secretos visibles en pantalla.

## Paso 10 - Crear Tu Primer Usuario

Qué hacer:

Crea un usuario funcional dentro del tenant.

Empieza con un usuario administrativo del cliente, no con muchos usuarios a la vez.

Por qué hacerlo:

Necesitas validar cómo una persona real entra, ve permisos y ejecuta tareas.

Qué ocurre internamente:

El sistema crea identidad, contraseña inicial, relación con tenant y estado de acceso.

Qué debes esperar ver:

El usuario aparece dentro del tenant y puede recibir rol.

## Paso 11 - Asignar Roles

Qué hacer:

Asigna al primer usuario un rol adecuado.

No asignes SuperAdmin a usuarios de cliente salvo que estés probando administración global de plataforma.

Por qué hacerlo:

Los roles controlan qué módulos puede ver y qué acciones puede ejecutar cada usuario.

Qué debes esperar ver:

El usuario solo debe ver las opciones correspondientes a sus permisos.

Error que no debería aparecer:

Un usuario de cliente no debería administrar otro tenant.

## Paso 12 - Comenzar A Usar Compliance 360

Qué hacer:

Después de tenant, empresa, branding, storage, notificaciones, usuario y rol, empieza con un flujo funcional pequeño:

1. Crear un documento.
2. Revisar aprobación.
3. Crear un proveedor.
4. Registrar un riesgo.
5. Crear un indicador.
6. Preparar una auditoría.

Por qué hacerlo:

Así aprendes Compliance 360 implementándolo, no leyendo teoría.

Qué debes observar:

- Qué datos pide cada módulo.
- Qué permisos se necesitan.
- Qué eventos quedan auditados.
- Qué notificaciones se generan.
- Qué configuraciones faltan para operar como cliente real.

## Regla De Oro

Si algo no entiendes, no lo configures todavía como definitivo.

Primero pregúntate:

- Qué problema del cliente resuelve.
- En qué tenant estoy.
- Qué usuario lo ejecuta.
- Qué evidencia deja.
- Qué pasará si lo cambio después.

Compliance 360 se aprende mejor recorriendo un caso real de implementación, paso por paso.
