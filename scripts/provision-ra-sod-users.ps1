$ErrorActionPreference = "Stop"
$base = "http://localhost:5272/api/v1"
$tenantId = "82af3877-2786-4d39-bce8-c981101c771d"
$adminEmail = "irvingcorrosk19@gmail.com"
$adminPassword = "OwnerStart!2026"
$pass = "OwnerStart!2026"

function Invoke-Json($Method, $Url, $Body, $Token) {
  $headers = @{ "Content-Type" = "application/json" }
  if ($Token) { $headers["Authorization"] = "Bearer $Token" }
  $json = if ($null -ne $Body) { $Body | ConvertTo-Json -Depth 8 -Compress } else { $null }
  return Invoke-RestMethod -Method $Method -Uri $Url -Headers $headers -Body $json
}

Write-Host "Identifying tenant admin..."
$identify = Invoke-Json POST "$base/auth/identify" @{ email = $adminEmail }
$login = Invoke-Json POST "$base/auth/login" @{
  email = $adminEmail
  password = $adminPassword
  resolverToken = $identify.resolverToken
  organizationId = $identify.preselectedOrganizationId
}
$token = $login.accessToken
if (-not $token) { throw "Login failed for tenant admin" }
Write-Host "Logged in."

$usersAdmin = Invoke-Json GET "$base/tenants/$tenantId/users" $null $token
$roles = @($usersAdmin.roles)
if (-not $roles -or $roles.Count -eq 0) { $roles = @($usersAdmin.Roles) }
Write-Host ("Roles loaded: " + (($roles | ForEach-Object { $_.name }) -join ", "))

function Find-RoleId([string]$name) {
  $r = $roles | Where-Object { $_.name -eq $name } | Select-Object -First 1
  if (-not $r) { throw "Role not found: $name" }
  return $r.id
}

$desired = @(
  @{ email = "ra.spec@cert.local";  name = "RA Specialist";  role = "Regulatory Specialist" },
  @{ email = "ra.rev@cert.local";   name = "RA Reviewer";    role = "Regulatory Reviewer" },
  @{ email = "ra.appr@cert.local";  name = "RA Approver";    role = "Regulatory Approver" },
  @{ email = "ra.sub@cert.local";   name = "RA Submitter";   role = "Regulatory Submitter" },
  @{ email = "ra.view@cert.local";  name = "RA Viewer";      role = "Regulatory Viewer" }
)

$existing = @($usersAdmin.users)
if (-not $existing) { $existing = @($usersAdmin.Users) }

foreach ($u in $desired) {
  $found = $existing | Where-Object { $_.email -eq $u.email } | Select-Object -First 1
  $roleId = Find-RoleId $u.role
  if ($found) {
    Write-Host "Exists: $($u.email) - resetting password"
    try {
      Invoke-Json POST "$base/tenants/$tenantId/users/$($found.id)/reset-password" @{
        newPassword = $pass
        forcePasswordChange = $false
        changeReason = "Production SoD lab reset"
      } $token | Out-Null
    } catch {
      Write-Host "Reset skipped for $($u.email): $($_.Exception.Message)"
    }
  } else {
    Write-Host "Creating: $($u.email) as $($u.role)"
    Invoke-Json POST "$base/tenants/$tenantId/users" @{
      email = $u.email
      fullName = $u.name
      initialPassword = $pass
      forcePasswordChange = $false
      roleId = $roleId
      changeReason = "Production SoD certification users"
    } $token | Out-Null
  }
}

Write-Host "SoD users ready."
