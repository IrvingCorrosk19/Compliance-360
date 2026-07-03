$ErrorActionPreference = "Stop"
$base = "http://localhost:5272/api/v1"
$platformTenant = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626"

function Get-Token($tenantId, $email, $password) {
  $b = @{ tenantId = $tenantId; email = $email; password = $password } | ConvertTo-Json
  $r = Invoke-WebRequest -Uri "$base/auth/login" -Method Post -ContentType "application/json" -Body $b -UseBasicParsing
  return ($r.Content | ConvertFrom-Json).accessToken
}

function Get-Perms($token) {
  $p = $token.Split('.')[1].Replace('-', '+').Replace('_', '/')
  $p = $p.PadRight($p.Length + (4 - $p.Length % 4) % 4, '=')
  $json = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($p))
  return @(($json | ConvertFrom-Json).permission)
}

function Has($perms, $code) { if ($perms -contains $code) { "YES" } else { "no" } }

# 1) Platform admin creates a tenant WITH an initial Tenant Administrator.
$padmin = Get-Token $platformTenant "admin@compliance360.local" "OwnerStart!2026"
$h = @{ Authorization = "Bearer $padmin" }
$slug = "sod-demo-" + (Get-Random -Maximum 99999)
$adminEmail = "tenantadmin@$slug.test"
$t = @{
  name = "SoD Demo Tenant"; slug = $slug; legalName = "SoD Demo S.A."; commercialName = "SoD Demo";
  taxIdentifier = ("RUC-" + (Get-Random -Maximum 999999)); countryCode = "PA"; currency = "USD";
  adminEmail = $adminEmail; adminFullName = "SoD Tenant Admin"; adminPassword = "Admin!2026Aa"
} | ConvertTo-Json
$tenant = (Invoke-WebRequest -Uri "$base/tenants/" -Method Post -Headers $h -ContentType "application/json" -Body $t -UseBasicParsing).Content | ConvertFrom-Json
$tid = $tenant.id
"TENANT CREATED: $tid ($slug)"

# 2) Login as the auto-provisioned Tenant Administrator.
$tadminToken = Get-Token $tid $adminEmail "Admin!2026Aa"
$tperms = Get-Perms $tadminToken
"TENANT ADMIN perms: $($tperms.Count) | TENANT.USERS=$(Has $tperms 'TENANT.USERS') | DOCUMENT.APPROVE=$(Has $tperms 'DOCUMENT.APPROVE')"
$th = @{ Authorization = "Bearer $tadminToken" }

# 3) Tenant admin lists roles and creates role-specific users.
$roles = (Invoke-WebRequest -Uri "$base/tenants/$tid/roles" -Method Get -Headers $th -UseBasicParsing).Content | ConvertFrom-Json
function RoleId($name) { ($roles | Where-Object { $_.name -eq $name }).id }

function NewUser($email, $name, $roleName) {
  $b = @{ email = $email; fullName = $name; initialPassword = "User!2026Aa"; forcePasswordChange = $false; roleId = (RoleId $roleName); changeReason = "SoD demo" } | ConvertTo-Json
  Invoke-WebRequest -Uri "$base/tenants/$tid/users" -Method Post -Headers $th -ContentType "application/json" -Body $b -UseBasicParsing | Out-Null
}

NewUser "dc@$slug.test" "Doc Controller" "Document Controller"
NewUser "qm@$slug.test" "Quality Mgr" "Quality Manager"
NewUser "aud@$slug.test" "Auditor" "Auditor"
NewUser "st@$slug.test" "Storage Admin" "Storage Administrator"
NewUser "nt@$slug.test" "Notif Admin" "Notification Administrator"

# 4) Verify SoD via each role's effective JWT permissions.
$dc = Get-Perms (Get-Token $tid "dc@$slug.test" "User!2026Aa")
$qm = Get-Perms (Get-Token $tid "qm@$slug.test" "User!2026Aa")
$aud = Get-Perms (Get-Token $tid "aud@$slug.test" "User!2026Aa")
$st = Get-Perms (Get-Token $tid "st@$slug.test" "User!2026Aa")
$nt = Get-Perms (Get-Token $tid "nt@$slug.test" "User!2026Aa")

""
"=== SoD MATRIX (effective JWT permissions) ==="
"Document Controller: CREATE=$(Has $dc 'DOCUMENT.CREATE') UPDATE=$(Has $dc 'DOCUMENT.UPDATE') APPROVE(must be no)=$(Has $dc 'DOCUMENT.APPROVE')"
"Quality Manager:     APPROVE=$(Has $qm 'DOCUMENT.APPROVE') CREATE(must be no)=$(Has $qm 'DOCUMENT.CREATE') CAPA.APPROVE=$(Has $qm 'CAPA.APPROVE')"
"Auditor:             AUDITMGMT.MANAGE=$(Has $aud 'AUDITMANAGEMENT.MANAGE') CAPA.MANAGE(must be no)=$(Has $aud 'CAPA.MANAGE') CAPA.CLOSE(must be no)=$(Has $aud 'CAPA.CLOSE')"
"Storage Admin:       STORAGE.CREATE=$(Has $st 'STORAGE.CREATE') NOTIFICATION.ADMIN(must be no)=$(Has $st 'NOTIFICATION.ADMIN')"
"Notification Admin:  NOTIFICATION.ADMIN=$(Has $nt 'NOTIFICATION.ADMIN') STORAGE.CREATE(must be no)=$(Has $nt 'STORAGE.CREATE')"
