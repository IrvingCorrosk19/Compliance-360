# Compliance 360 — Manual 08 Technical Sheets functional harness
$ErrorActionPreference = "Stop"
$base = "http://localhost:5272"
$tenant = "ddcaf211-afe0-44a0-9c90-4fbda8fc4871"
$pwd = "Premium!2026"
$code = "TS-MFT-" + (Get-Date -Format "HHmmss")
$sku = $code
$steps = New-Object System.Collections.Generic.List[object]

function Add-Step($id, $name, $ok, $detail) {
    $steps.Add([ordered]@{ id = $id; name = $name; status = $(if ($ok) { "PASS" } else { "FAIL" }); detail = "$detail" })
    $mark = if ($ok) { "OK  " } else { "FAIL" }
    Write-Host ("[$mark] $id - $name :: $detail")
}

function Login($tenantId, $email, $password) {
    $body = @{ tenantId = $tenantId; email = $email; password = $password } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$base/api/v1/auth/login" -Method Post -Body $body -ContentType "application/json"
    $token = $r.accessToken; if (-not $token) { $token = $r.token }
    return @{ headers = @{ Authorization = "Bearer $token"; "Content-Type" = "application/json" }; userId = $r.userId }
}

function Call($headers, $method, $path, $body) {
    $uri = "$base$path"
    try {
        if ($body) { return Invoke-WebRequest -Uri $uri -Method $method -Headers $headers -Body ($body | ConvertTo-Json -Depth 8) -ContentType "application/json" -UseBasicParsing }
        return Invoke-WebRequest -Uri $uri -Method $method -Headers $headers -UseBasicParsing
    } catch {
        $resp = $_.Exception.Response
        $status = if ($resp) { [int]$resp.StatusCode } else { 0 }
        $text = ""
        if ($resp -and $resp.GetResponseStream()) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $text = $reader.ReadToEnd()
        }
        return [pscustomobject]@{ StatusCode = $status; Content = $text; Failed = $true }
    }
}

function Get-RoleId($headers, $tenantId, $roleName) {
    $center = Invoke-RestMethod -Uri "$base/api/v1/tenants/$tenantId/administration-center" -Headers $headers
    $roles = $center.users.roles
    $role = $roles | Where-Object { $_.name -eq $roleName } | Select-Object -First 1
    if (-not $role) { throw "Role not found: $roleName" }
    return $role.id
}

function Ensure-Permission($headers, $tenantId, $roleId, $module, $action, $description) {
    $created = Call $headers "POST" "/api/v1/tenants/$tenantId/rbac/permissions" @{ module = $module; action = $action; description = $description }
    if ($created.Failed) { throw "Create permission failed: $($created.StatusCode) $($created.Content)" }
    $perm = $created.Content | ConvertFrom-Json
    $grant = Call $headers "POST" "/api/v1/tenants/$tenantId/rbac/permissions/grant" @{ roleId = $roleId; permissionId = $perm.id }
    if ($grant.Failed) { throw "Grant permission failed: $($grant.StatusCode) $($grant.Content)" }
    return $perm
}

New-Item -ItemType Directory -Force -Path "artifacts/manual-functional-testing" | Out-Null
Write-Host "===== MANUAL 08 - TECHNICAL SHEETS MFT ====="

# TC-TS-001 — Document Controller cannot create
$dc = Login $tenant "doccontrol@alimentos-premium.test" $pwd
$dcCreate = Call $dc.headers "POST" "/api/v1/tenants/$tenant/technical-sheets/products" @{ name = "Blocked"; sku = "BLK-$code"; description = "x" }
Add-Step "TC-TS-001" "Document Controller sin CREATE" ($dcCreate.StatusCode -eq 403) "HTTP $($dcCreate.StatusCode)"

# TC-TS-002 — Grant TECHNICALSHEET.CREATE (+ UPDATE para submit)
$ta = Login $tenant "tenantadmin@alimentos-premium.test" $pwd
$taRole = Get-RoleId $ta.headers $tenant "Tenant Administrator"
try {
    Ensure-Permission $ta.headers $tenant $taRole "TECHNICALSHEET" 1 "MFT Technical sheet create" | Out-Null
    Ensure-Permission $ta.headers $tenant $taRole "TECHNICALSHEET" 2 "MFT Technical sheet update" | Out-Null
    $grantOk = $true
} catch { $grantOk = $false; $grantErr = $_.Exception.Message }
Add-Step "TC-TS-002" "Otorgar TECHNICALSHEET.CREATE/UPDATE" $grantOk "$grantErr"

# Re-login tenant admin so JWT picks up new permissions
$ta = Login $tenant "tenantadmin@alimentos-premium.test" $pwd

# TC-TS-003 / TC-TS-004 — Create product + sheet
$productResp = Call $ta.headers "POST" "/api/v1/tenants/$tenant/technical-sheets/products" @{
    name = "Ficha Tecnica Yogurt Natural MFT"; sku = $sku; description = "Producto lacteo pasteurizado - prueba MFT"
}
$productOk = -not $productResp.Failed -and $productResp.StatusCode -ge 200 -and $productResp.StatusCode -lt 300
$product = if ($productOk) { $productResp.Content | ConvertFrom-Json } else { $null }
Add-Step "TC-TS-003" "Crear producto técnico" $productOk $(if ($productOk) { "productId=$($product.id)" } else { "HTTP $($productResp.StatusCode)" })

$sheetId = $null
if ($productOk) {
    $sheetResp = Call $ta.headers "POST" "/api/v1/tenants/$tenant/technical-sheets" @{
        productId = $product.id; title = "Ficha Tecnica Yogurt Natural MFT"
    }
    $sheetOk = -not $sheetResp.Failed -and $sheetResp.StatusCode -ge 200 -and $sheetResp.StatusCode -lt 300
    if ($sheetOk) { $sheetId = ($sheetResp.Content | ConvertFrom-Json).id }
    Add-Step "TC-TS-004" "Crear ficha técnica API" $sheetOk $(if ($sheetOk) { "sheetId=$sheetId code=$sku" } else { "HTTP $($sheetResp.StatusCode)" })
} else {
    Add-Step "TC-TS-004" "Crear ficha técnica API" $false "Skipped (product failed)"
}

# TC-TS-005 — Quality Manager read, no create
$qm = Login $tenant "quality@alimentos-premium.test" $pwd
$qmList = Call $qm.headers "GET" "/api/v1/tenants/$tenant/technical-sheets?searchText=&page=1&pageSize=10" $null
$qmCreate = Call $qm.headers "POST" "/api/v1/tenants/$tenant/technical-sheets/products" @{ name = "QM Block"; sku = "QM-$code"; description = "x" }
$listOk = -not $qmList.Failed -and $qmList.StatusCode -eq 200
Add-Step "TC-TS-005" "Quality Manager lectura sin create" ($listOk -and $qmCreate.StatusCode -eq 403) "list=$($qmList.StatusCode) create=$($qmCreate.StatusCode)"

# TC-TS-006 — Version + submit + approve
if ($sheetId) {
    $ver = Call $ta.headers "POST" "/api/v1/tenants/$tenant/technical-sheets/$sheetId/versions" @{ changeSummary = "Initial version MFT" }
    $verOk = -not $ver.Failed -and $ver.StatusCode -ge 200 -and $ver.StatusCode -lt 300
    $submit = Call $ta.headers "POST" "/api/v1/tenants/$tenant/technical-sheets/$sheetId/submit" $null
    $submitOk = -not $submit.Failed -and $submit.StatusCode -ge 200 -and $submit.StatusCode -lt 300
    $decide = Call $qm.headers "POST" "/api/v1/tenants/$tenant/technical-sheets/$sheetId/decision" @{ decision = 0; comments = "Aprobado MFT" }
    $decideOk = -not $decide.Failed -and $decide.StatusCode -ge 200 -and $decide.StatusCode -lt 300
    Add-Step "TC-TS-006" "Aprobar ficha tecnica (QM)" ($verOk -and $submitOk -and $decideOk) "version=$($ver.StatusCode) submit=$($submit.StatusCode) decide=$($decide.StatusCode)"
} else {
    Add-Step "TC-TS-006" "Aprobar ficha técnica (QM)" $false "No sheetId"
}

# TC-TS-007 — Viewer read-only
$vw = Login $tenant "viewer@alimentos-premium.test" $pwd
$vwList = Call $vw.headers "GET" "/api/v1/tenants/$tenant/technical-sheets?searchText=&page=1&pageSize=10" $null
$vwCreate = Call $vw.headers "POST" "/api/v1/tenants/$tenant/technical-sheets/products" @{ name = "VW Block"; sku = "VW-$code"; description = "x" }
Add-Step "TC-TS-007" "Viewer solo lectura" ((-not $vwList.Failed) -and $vwCreate.StatusCode -eq 403) "list=$($vwList.StatusCode) create=$($vwCreate.StatusCode)"

# TC-TS-008 — Duplicate SKU
$dup = Call $ta.headers "POST" "/api/v1/tenants/$tenant/technical-sheets/products" @{
    name = "Duplicado"; sku = $sku; description = "dup"
}
Add-Step "TC-TS-008" "SKU duplicado rechazado" ($dup.StatusCode -eq 400 -or $dup.StatusCode -eq 409) "HTTP $($dup.StatusCode)"

$pass = @($steps | Where-Object { $_.status -eq "PASS" }).Count
$fail = @($steps | Where-Object { $_.status -eq "FAIL" }).Count
$summary = @{
    generatedAt = (Get-Date).ToString("o")
    manual = "08_TECHNICAL_SHEETS_FUNCTIONAL_TESTS"
    tenantId = $tenant
    productSku = $sku
    technicalSheetId = $sheetId
    pass = $pass
    fail = $fail
    steps = $steps
}
$out = "artifacts/manual-functional-testing/technical_sheets_mft_result.json"
$summary | ConvertTo-Json -Depth 6 | Out-File -FilePath $out -Encoding utf8
Write-Host ""
Write-Host "===== TECHNICAL SHEETS MFT: $pass PASS / $fail FAIL ====="
Write-Host "Wrote $out"
if ($fail -gt 0) { exit 1 }
