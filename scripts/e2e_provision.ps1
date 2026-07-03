# Compliance 360 - E2E test data provisioning
# Creates the "Alimentos Premium Panama S.A." tenant + one user per tenant role,
# then clears force-password-change and disables MFA so each role can log in.
$ErrorActionPreference = "Stop"

$BaseUrl   = "http://localhost:5272"
$PlatformTenant = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626"
$PlatformEmail  = "admin@compliance360.local"
$PlatformPass   = "OwnerStart!2026"
$PgBin     = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
$env:PGPASSWORD = "Panama2020$"
$Pg = @("-h","localhost","-U","postgres","-d","compliance360","-t","-A","-F",",")

function Login($tenantId, $email, $pass) {
  $body = @{ tenantId = $tenantId; email = $email; password = $pass } | ConvertTo-Json
  $r = Invoke-RestMethod -Uri "$BaseUrl/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json"
  return $r
}

function Sql($query) {
  $tmp = [System.IO.Path]::GetTempFileName() + ".sql"
  Set-Content -Path $tmp -Value $query -Encoding UTF8
  try { return (& $PgBin @Pg -f $tmp) 2>&1 }
  finally { Remove-Item $tmp -ErrorAction SilentlyContinue }
}

function SqlScalar($query) {
  $out = Sql $query
  if ($null -eq $out) { return "" }
  return ("" + ($out | Select-Object -First 1)).Trim()
}

Write-Host "== Login Platform Administrator =="
$plat = Login $PlatformTenant $PlatformEmail $PlatformPass
$platToken = $plat.accessToken
if (-not $platToken) { $platToken = $plat.token }
if (-not $platToken) { throw "No platform token. Response: $($plat | ConvertTo-Json -Depth 5)" }
$platHeaders = @{ Authorization = "Bearer $platToken" }
Write-Host "   token acquired."

# --- Check existing tenant by slug ---
$slug = "alimentos-premium-pa"
$existingId = SqlScalar "select ""Id"" from compliance360.tenants where ""Slug"" = '$slug' limit 1;"

if ($existingId) {
  Write-Host "== Tenant already exists: $existingId =="
  $tenantId = $existingId
} else {
  Write-Host "== Creating tenant Alimentos Premium Panama S.A. =="
  $tenantBody = @{
    Name = "Alimentos Premium Panama S.A."
    Slug = $slug
    LegalName = "Alimentos Premium Panama, S.A."
    CommercialName = "Alimentos Premium"
    TaxIdentifier = "RUC-155999999-2-2026"
    CountryCode = "PA"
    Currency = "USD"
    AdminEmail = "tenantadmin@alimentos-premium.test"
    AdminFullName = "Tenant Administrator - Alimentos Premium"
    AdminPassword = "Premium!2026"
  } | ConvertTo-Json
  $created = Invoke-RestMethod -Uri "$BaseUrl/api/v1/tenants/" -Method Post -Body $tenantBody -ContentType "application/json" -Headers $platHeaders
  $tenantId = $created.id
  if (-not $tenantId) { $tenantId = $created.tenantId }
  if (-not $tenantId) { $tenantId = SqlScalar "select ""Id"" from compliance360.tenants where ""Slug"" = '$slug' limit 1;" }
  Write-Host "   tenant created: $tenantId"
}

# --- Ensure tenant active, MFA disabled, force-password cleared ---
Write-Host "== Relaxing security for testing (tenant $tenantId) =="
Sql "update compliance360.tenants set ""Status"" = 'Active' where ""Id"" = '$tenantId';" | Out-Null
Sql "update compliance360.tenant_settings set ""RequireMfa"" = false where ""TenantId"" = '$tenantId';" | Out-Null
Sql "update compliance360.users set ""ForcePasswordChangeRequired"" = false where ""TenantId"" = '$tenantId';" | Out-Null

# --- Login tenant admin ---
Write-Host "== Login Tenant Administrator =="
$ten = Login $tenantId "tenantadmin@alimentos-premium.test" "Premium!2026"
$tenToken = $ten.accessToken; if (-not $tenToken) { $tenToken = $ten.token }
if (-not $tenToken) { throw "No tenant admin token. Response: $($ten | ConvertTo-Json -Depth 5)" }
$tenHeaders = @{ Authorization = "Bearer $tenToken" }
Write-Host "   token acquired."

# --- Get tenant role ids ---
$rolesRaw = Sql "select ""Name"", ""Id"" from compliance360.roles where ""TenantId"" = '$tenantId';"
$roleMap = @{}
foreach ($line in $rolesRaw) {
  $t = "$line".Trim(); if (-not $t) { continue }
  $parts = $t.Split(","); if ($parts.Count -ge 2) { $roleMap[$parts[0]] = $parts[1] }
}
Write-Host "   roles found: $($roleMap.Keys.Count)"

# --- Users to create: role -> email ---
$users = [ordered]@{
  "Tenant Security Administrator" = "security@alimentos-premium.test"
  "Document Controller"           = "doccontrol@alimentos-premium.test"
  "Quality Manager"               = "quality@alimentos-premium.test"
  "Auditor"                       = "auditor@alimentos-premium.test"
  "Supplier Manager"              = "supplier@alimentos-premium.test"
  "CAPA Manager"                  = "capa@alimentos-premium.test"
  "Risk Manager"                  = "risk@alimentos-premium.test"
  "Indicators Manager"            = "indicators@alimentos-premium.test"
  "Reporting Manager"             = "reporting@alimentos-premium.test"
  "Storage Administrator"         = "storage@alimentos-premium.test"
  "Notification Administrator"    = "notifications@alimentos-premium.test"
  "Viewer"                        = "viewer@alimentos-premium.test"
}

$result = [ordered]@{
  baseUrl = $BaseUrl
  tenantId = $tenantId
  platform = @{ tenantId = $PlatformTenant; email = $PlatformEmail; password = $PlatformPass; role = "Platform Administrator" }
  users = @()
}
$result.users += [ordered]@{ role = "Tenant Administrator"; email = "tenantadmin@alimentos-premium.test"; password = "Premium!2026" }

foreach ($role in $users.Keys) {
  $email = $users[$role]
  $roleId = $roleMap[$role]
  if (-not $roleId) { Write-Host "   !! role not found: $role"; continue }
  $exists = SqlScalar "select ""Id"" from compliance360.users where ""TenantId"" = '$tenantId' and lower(""Email"") = lower('$email') limit 1;"
  if ($exists) {
    Write-Host "   user exists: $email"
  } else {
    $userBody = @{
      Email = $email
      FullName = "$role - Alimentos Premium"
      InitialPassword = "Premium!2026"
      ForcePasswordChange = $false
      RoleId = $roleId
      ChangeReason = "E2E provisioning"
    } | ConvertTo-Json
    try {
      Invoke-RestMethod -Uri "$BaseUrl/api/v1/tenants/$tenantId/users" -Method Post -Body $userBody -ContentType "application/json" -Headers $tenHeaders | Out-Null
      Write-Host "   created: $email ($role)"
    } catch {
      Write-Host "   !! failed $email : $($_.Exception.Message)"
    }
  }
  $result.users += [ordered]@{ role = $role; email = $email; password = "Premium!2026" }
}

# --- Support Operator (platform break-glass role) ---
Write-Host "== Provisioning Support Operator on platform tenant =="
$supportEmail = "support@compliance360.local"
& $PgBin @Pg -f (Join-Path $PSScriptRoot "e2e_provision_support.sql") | Out-Null
$result.support = [ordered]@{ role = "Support Operator"; email = $supportEmail; password = $PlatformPass; tenantId = $PlatformTenant }
Write-Host "   support operator ready: $supportEmail"

# --- Final relax pass for all created users ---
Sql "update compliance360.users set ""ForcePasswordChangeRequired"" = false, ""Status"" = 'Active' where ""TenantId"" = '$tenantId';" | Out-Null
Sql "update compliance360.tenant_settings set ""RequireMfa"" = false where ""TenantId"" = '$tenantId';" | Out-Null

$json = $result | ConvertTo-Json -Depth 6
[System.IO.File]::WriteAllText((Join-Path (Get-Location) "e2e/testdata.json"), $json, (New-Object System.Text.UTF8Encoding $false))
Write-Host "== Wrote e2e/testdata.json with $($result.users.Count) users =="
