/**
 * Clean polluted locale keys (CSS classes, meta tags, technical noise)
 * and fix mojibake / missing accents in Spanish catalog.
 */
import fs from "fs";
import path from "path";

const root = process.cwd();
const esPath = path.join(root, "src/Compliance360.Web/wwwroot/locales/es.json");
const enPath = path.join(root, "src/Compliance360.Web/wwwroot/locales/en.json");
const es = JSON.parse(fs.readFileSync(esPath, "utf8"));
const en = JSON.parse(fs.readFileSync(enPath, "utf8"));

const dropKey = (key, value) => {
  if (!value || typeof value !== "string") return true;
  if (/^(c360\.|REGULATORY\.|TENANT\.|PLATFORM\.|IX_|PK_|FK_)/i.test(value)) return true;
  if (/[{};]|^\.|width\s*=|device-width|initial-scale|viewport/i.test(value)) return true;
  if (/^(btn|card|grid|form-stack|status-pill|hero|nav-|ra-|cs-|progress|loading|skeleton|compact|elevated|modern)/i.test(value)) return true;
  if (/class=|data-|aria-|autocomplete|inputmode/i.test(value)) return true;
  if (value.length <= 2) return true;
  // keys that look like CSS class dumps
  if (/ProgressBar|FormStackCard|HeroCard|LoadingLogo|Skeleton|StatusPillWarn|TenantAlertOk|ModuleCard|ReadinessCard|ErrorStateRich|BreadcrumbsCompact|ButtonRowSection|TenantHero|PlatformShell|AdminPasswordDialogFormStack/i.test(key)
      && !/[áéíóúñÁÉÍÓÚÑ ]/.test(value) && value.split(" ").length <= 4 && /^[A-Za-z0-9 _-]+$/.test(value)) {
    // drop if English-looking CSS-ish short token without spaces of UI meaning
    if (!/\b(Save|Cancel|Delete|Search|Login|Password|Email|Settings|Required|Error|Success|Welcome|Sign)\b/i.test(value)) {
      return /Card|Pill|Hero|Stack|Compact|Dynamic|Warn|Ok|Modern|Rich|Bar|Logo|Shell|Grid|Form/i.test(key);
    }
  }
  return false;
};

const accentFix = {
  "Contrase?a": "Contraseña",
  "contrase?a": "contraseña",
  "electr?nico": "electrónico",
  "Electr?nico": "Electrónico",
  "sesi?n": "sesión",
  "Sesi?n": "Sesión",
  "configuraci?n": "configuración",
  "Configuraci?n": "Configuración",
  "administraci?n": "administración",
  "Administraci?n": "Administración",
  "autenticaci?n": "autenticación",
  "verificaci?n": "verificación",
  "descripci?n": "descripción",
  "opci?n": "opción",
  "informaci?n": "información",
  "organizaci?n": "organización",
  "Organizaci?n": "Organización",
  "permiso?": "permisos",
};

function fixAccents(s) {
  let out = s;
  for (const [bad, good] of Object.entries(accentFix)) out = out.split(bad).join(good);
  // common missing accents in Spanish UI
  out = out
    .replace(/\bContrasena\b/g, "Contraseña")
    .replace(/\bcontrasena\b/g, "contraseña")
    .replace(/\bSesion\b/g, "Sesión")
    .replace(/\bsesion\b/g, "sesión")
    .replace(/\bCodigo\b/g, "Código")
    .replace(/\bcodigo\b/g, "código")
    .replace(/\bPagina\b/g, "Página")
    .replace(/\bTamano\b/g, "Tamaño")
    .replace(/\belectronico\b/g, "electrónico")
    .replace(/\bElectronico\b/g, "Electrónico")
    .replace(/\borganizacion\b/g, "organización")
    .replace(/\bOrganizacion\b/g, "Organización")
    .replace(/\bconfiguracion\b/g, "configuración")
    .replace(/\bConfiguracion\b/g, "Configuración")
    .replace(/\bAdministracion\b/g, "Administración")
    .replace(/\badministracion\b/g, "administración")
    .replace(/\bInformacion\b/g, "Información")
    .replace(/\binformacion\b/g, "información")
    .replace(/\bDescripcion\b/g, "Descripción")
    .replace(/\bVerificacion\b/g, "Verificación")
    .replace(/\bAutenticacion\b/g, "Autenticación")
    .replace(/\bopcion\b/g, "opción")
    .replace(/\bOpcion\b/g, "Opción")
    .replace(/\bAnadir\b/g, "Añadir")
    .replace(/\banadir\b/g, "añadir");
  return out;
}

const cleanedEs = {};
const cleanedEn = {};
let dropped = 0;
for (const key of Object.keys(es)) {
  const esVal = fixAccents(String(es[key]));
  const enVal = fixAccents(String(en[key] ?? esVal));
  if (dropKey(key, esVal) || dropKey(key, enVal)) {
    dropped++;
    continue;
  }
  cleanedEs[key] = esVal;
  cleanedEn[key] = enVal;
}

// Ensure critical enterprise keys exist
const essential = {
  "Common.Save": ["Guardar", "Save"],
  "Common.Cancel": ["Cancelar", "Cancel"],
  "Common.Edit": ["Editar", "Edit"],
  "Common.Delete": ["Eliminar", "Delete"],
  "Common.Search": ["Buscar", "Search"],
  "Common.SignOut": ["Cerrar sesión", "Sign out"],
  "Common.Back": ["Atrás", "Back"],
  "Common.Next": ["Siguiente", "Next"],
  "Common.Continue": ["Continuar", "Continue"],
  "Common.Loading": ["Cargando…", "Loading…"],
  "Common.Retry": ["Reintentar", "Retry"],
  "Common.Close": ["Cerrar", "Close"],
  "Common.Confirm": ["Confirmar", "Confirm"],
  "Common.Yes": ["Sí", "Yes"],
  "Common.No": ["No", "No"],
  "Common.Actions": ["Acciones", "Actions"],
  "Common.Status": ["Estado", "Status"],
  "Common.Name": ["Nombre", "Name"],
  "Common.Email": ["Correo electrónico", "Email"],
  "Common.Password": ["Contraseña", "Password"],
  "Common.Language": ["Idioma", "Language"],
  "Common.Theme": ["Tema", "Theme"],
  "Common.Light": ["Claro", "Light"],
  "Common.Dark": ["Oscuro", "Dark"],
  "Common.Live": ["En vivo", "Live"],
  "Common.Reload": ["Recargar", "Reload"],
  "Common.Export": ["Exportar", "Export"],
  "Common.Create": ["Crear", "Create"],
  "Common.Update": ["Actualizar", "Update"],
  "Common.Required": ["Obligatorio", "Required"],
  "Common.Optional": ["Opcional", "Optional"],
  "Common.LanguageUpdated": ["Idioma actualizado.", "Language updated."],
  "Login.SignIn": ["Iniciar sesión", "Sign in"],
  "Login.Title": ["Compliance 360 Enterprise", "Compliance 360 Enterprise"],
  "Login.Email": ["Correo electrónico", "Email"],
  "Login.Password": ["Contraseña", "Password"],
  "Login.RememberMe": ["Recordarme en este dispositivo", "Remember me on this device"],
  "Login.ForgotPassword": ["Olvidé mi contraseña", "Forgot password"],
  "Login.Next": ["Siguiente", "Next"],
  "Login.Continue": ["Continuar", "Continue"],
  "Login.Back": ["Atrás", "Back"],
  "Login.ChangeOrganization": ["Cambiar organización", "Change organization"],
  "Login.ChangeEmail": ["Cambiar correo", "Change email"],
  "Login.EmailStepHint": ["Ingresa tu correo corporativo para identificar tus organizaciones.", "Enter your corporate email to identify your organizations."],
  "Login.OrganizationStepHint": ["Selecciona la organización para continuar.", "Select the organization to continue."],
  "Login.PasswordStepHint": ["Ingresa tu contraseña para continuar.", "Enter your password to continue."],
  "Login.SelectOrganization": ["Selecciona tu organización", "Select your organization"],
  "Login.SignedIn": ["Sesión iniciada correctamente.", "Signed in successfully."],
  "Login.SignedOut": ["Sesión cerrada.", "Signed out."],
  "Login.SessionExpired": ["Sesión expirada. Inicia sesión nuevamente.", "Session expired. Sign in again."],
  "Login.ForgotPasswordHint": ["Si olvidó su contraseña, pida a un administrador del tenant que la restablezca en Administración del Tenant → Usuarios.", "If you forgot your password, ask a tenant administrator to reset it under Tenant Administration → Users."],
  "Login.NoOrganization": ["No encontramos una organización para este correo. Verifica que el administrador te haya creado en el tenant.", "We could not find an organization for this email. Verify that an administrator has created your account in the tenant."],
  "MFA.Title": ["Verificación de segundo factor", "Second-factor verification"],
  "MFA.Hint": ["El tenant o el usuario requiere MFA. Ingresa el código TOTP para emitir el token final de sesión.", "The tenant or user requires MFA. Enter the TOTP code to issue the final session token."],
  "MFA.Code": ["Código de 6 dígitos", "6-digit code"],
  "MFA.Submit": ["Completar inicio de sesión seguro", "Complete secure sign-in"],
  "MFA.Required": ["MFA requerido. Completa el segundo factor.", "MFA required. Complete the second factor."],
  "MFA.Success": ["MFA validado. Sesión segura iniciada.", "MFA validated. Secure session started."],
  "Nav.CommandCenter": ["Centro de comando", "Command center"],
  "Nav.Regulatory": ["Asuntos Regulatorios", "Regulatory Affairs"],
  "Nav.Administration": ["Administración", "Administration"],
  "Nav.Platform": ["Gobierno global", "Global governance"],
  "Brand.Name": ["Compliance 360", "Compliance 360"],
  "Brand.Edition": ["Enterprise Edition", "Enterprise Edition"],
  "Settings.Language": ["Idioma", "Language"],
  "Validation.Required": ["Campo obligatorio", "Required field"],
  "Validation.InvalidEmail": ["Correo inválido", "Invalid email"],
  "Validation.MinPassword": ["La contraseña debe tener mínimo 8 caracteres", "Password must contain at least 8 characters"],
  "Validation.PasswordMismatch": ["Las contraseñas no coinciden", "Passwords do not match"],
  "Errors.Generic": ["No se pudo completar la operación.", "The operation could not be completed."],
  "Errors.Forbidden": ["No autorizado", "Forbidden"],
  "Errors.NotFound": ["No encontrado", "Not found"],
  "Tac.Title": ["Administración de Tenant", "Tenant Administration"],
  "Tac.Users": ["Usuarios", "Users"],
  "Tac.Roles": ["Roles y permisos", "Roles & permissions"],
  "Tac.General": ["Información general", "General information"],
  "Tac.Branding": ["Branding", "Branding"],
  "Tac.Licensing": ["Licenciamiento", "Licensing"],
  "Tac.Health": ["Health y backups", "Health & backups"],
  "Tac.Audit": ["Auditoría", "Audit"],
  "Tac.ChangeReason": ["Motivo del cambio", "Change reason"],
  "Tac.UserAdmin": ["Administración de usuarios", "User administration"],
  "Tac.UserAdminHint": ["Crear, bloquear, desbloquear, restablecer contraseña, reset MFA, roles y sesiones por tenant", "Create, block, unblock, reset password, reset MFA, roles and sessions per tenant"],
  "Regulatory.Title": ["Asuntos Regulatorios", "Regulatory Affairs"],
  "Regulatory.NoPermission": ["Sin permiso", "No permission"],
  "Dashboard.Title": ["Centro de comando", "Command center"],
  "Dashboard.Welcome": ["Bienvenido", "Welcome"]
};

for (const [key, [esVal, enVal]] of Object.entries(essential)) {
  cleanedEs[key] = esVal;
  cleanedEn[key] = enVal;
}

fs.writeFileSync(esPath, JSON.stringify(cleanedEs, null, 2) + "\n", "utf8");
fs.writeFileSync(enPath, JSON.stringify(cleanedEn, null, 2) + "\n", "utf8");
console.log(`Kept ${Object.keys(cleanedEs).length} keys, dropped ${dropped}`);
