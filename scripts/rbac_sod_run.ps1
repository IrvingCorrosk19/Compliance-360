param([string]$tid, [string]$adminEmail, [string]$adminPass)
$ErrorActionPreference = "Stop"
$base = "http://localhost:5272/api/v1"

function Get-Token($tenantId, $email, $password) {
  $b = @{ tenantId = $tenantId; email = $email; password = $password } | ConvertTo-Json
  ($((Invoke-WebRequest -Uri "$base/auth/login" -Method Post -ContentType "application/json" -Body $b -UseBasicParsing).Content) | ConvertFrom-Json).accessToken
}
function Get-Perms($token) {
  $p = $token.Split('.')[1].Replace('-', '+').Replace('_', '/'); $p = $p.PadRight($p.Length + (4 - $p.Length % 4) % 4, '=')
  @(([System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($p)) | ConvertFrom-Json).permission)
}
function Has($perms, $code) { if ($perms -contains $code) { "YES" } else { "no" } }

$tadminToken = Get-Token $tid $adminEmail $adminPass
$tperms = Get-Perms $tadminToken
"TENANT ADMIN perms: $($tperms.Count) | TENANT.USERS=$(Has $tperms 'TENANT.USERS') | DOCUMENT.APPROVE(must be no)=$(Has $tperms 'DOCUMENT.APPROVE')"
$th = @{ Authorization = "Bearer $tadminToken" }
$roles = (Invoke-WebRequest -Uri "$base/tenants/$tid/roles" -Method Get -Headers $th -UseBasicParsing).Content | ConvertFrom-Json
function RoleId($name) { ($roles | Where-Object { $_.name -eq $name }).id }
function NewUser($email, $name, $roleName) {
  $b = @{ email = $email; fullName = $name; initialPassword = "User!2026Aa"; forcePasswordChange = $false; roleId = (RoleId $roleName); changeReason = "SoD demo" } | ConvertTo-Json
  try { Invoke-WebRequest -Uri "$base/tenants/$tid/users" -Method Post -Headers $th -ContentType "application/json" -Body $b -UseBasicParsing | Out-Null; "created $roleName" } catch { "FAILED $roleName $($_.Exception.Response.StatusCode.value__)" }
}
NewUser "dc@$tid.test" "Doc Controller" "Document Controller" | Out-Null
NewUser "qm@$tid.test" "Quality Mgr" "Quality Manager" | Out-Null
NewUser "aud@$tid.test" "Auditor" "Auditor" | Out-Null
NewUser "st@$tid.test" "Storage Admin" "Storage Administrator" | Out-Null
NewUser "nt@$tid.test" "Notif Admin" "Notification Administrator" | Out-Null

$dc = Get-Perms (Get-Token $tid "dc@$tid.test" "User!2026Aa")
$qm = Get-Perms (Get-Token $tid "qm@$tid.test" "User!2026Aa")
$aud = Get-Perms (Get-Token $tid "aud@$tid.test" "User!2026Aa")
$st = Get-Perms (Get-Token $tid "st@$tid.test" "User!2026Aa")
$nt = Get-Perms (Get-Token $tid "nt@$tid.test" "User!2026Aa")
""
"=== SoD MATRIX (effective JWT permissions) ==="
"Document Controller: CREATE=$(Has $dc 'DOCUMENT.CREATE') UPDATE=$(Has $dc 'DOCUMENT.UPDATE') APPROVE(no)=$(Has $dc 'DOCUMENT.APPROVE')"
"Quality Manager:     APPROVE=$(Has $qm 'DOCUMENT.APPROVE') CREATE(no)=$(Has $qm 'DOCUMENT.CREATE') CAPA.APPROVE=$(Has $qm 'CAPA.APPROVE')"
"Auditor:             AUDITMGMT.MANAGE=$(Has $aud 'AUDITMANAGEMENT.MANAGE') CAPA.MANAGE(no)=$(Has $aud 'CAPA.MANAGE') CAPA.CLOSE(no)=$(Has $aud 'CAPA.CLOSE')"
"Storage Admin:       STORAGE.CREATE=$(Has $st 'STORAGE.CREATE') NOTIFICATION.ADMIN(no)=$(Has $st 'NOTIFICATION.ADMIN')"
"Notification Admin:  NOTIFICATION.ADMIN=$(Has $nt 'NOTIFICATION.ADMIN') STORAGE.CREATE(no)=$(Has $nt 'STORAGE.CREATE')"
