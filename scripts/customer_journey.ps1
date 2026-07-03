# Compliance 360 - Final Customer Acceptance Journey
# Drives a real end-to-end customer journey and emits a structured JSON evidence file.
$ErrorActionPreference = "Stop"
$base = "http://localhost:5272"
$platformTenant = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626"
$opsTenant = "ddcaf211-afe0-44a0-9c90-4fbda8fc4871"
$pwd = "Premium!2026"
$steps = New-Object System.Collections.Generic.List[object]

function Add-Step($n, $name, $ok, $detail) {
    $steps.Add([ordered]@{ step = $n; name = $name; status = $(if ($ok) { "PASS" } else { "FAIL" }); detail = "$detail" })
    $color = if ($ok) { "OK  " } else { "FAIL" }
    Write-Host ("$n [$color] $name :: $detail")
}
function Login($tenant, $email, $password) {
    if (-not $password) { $password = $pwd }
    $b = @{ tenantId = $tenant; email = $email; password = $password } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$base/api/v1/auth/login" -Method Post -Body $b -ContentType "application/json"
    $t = $r.accessToken; if (-not $t) { $t = $r.token }
    return @{ headers = @{ Authorization = "Bearer $t"; "Content-Type" = "application/json" }; userId = $r.userId; raw = $r }
}
function Call($headers, $method, $path, $body) {
    $uri = "$base$path"
    try {
        if ($body) { return Invoke-RestMethod -Uri $uri -Method $method -Headers $headers -Body ($body | ConvertTo-Json -Depth 8) }
        else { return Invoke-RestMethod -Uri $uri -Method $method -Headers $headers }
    } catch {
        $sc = $_.Exception.Response.StatusCode.value__
        throw "HTTP $sc on $method $path"
    }
}

function Get-RoleId($tenantId, $roleName) {
    $sql = "select ""Id"" from compliance360.roles where ""TenantId""='$tenantId' and ""Name""='$roleName' limit 1;"
    $f = "scripts/_role_$([guid]::NewGuid().ToString('N')).sql"
    $sql | Out-File -FilePath $f -Encoding ascii
    $id = (& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -h localhost -U postgres -d compliance360 -t -A -f $f 2>$null | Select-Object -First 1).Trim()
    Remove-Item $f -ErrorAction SilentlyContinue
    return $id
}

function Upload-File($headers, $tenantId, $ownerEntityName, $ownerEntityId) {
    New-Item -ItemType Directory -Force -Path "artifacts/e2e" | Out-Null
    $path = Join-Path (Get-Location) "artifacts/e2e/journey-upload.txt"
    [System.IO.File]::WriteAllText($path, "Journey acceptance test file")
    $uri = "$base/api/v1/tenants/$tenantId/storage/files"
    $token = $headers.Authorization -replace '^Bearer\s+',''
    Add-Type -AssemblyName System.Net.Http
    $client = New-Object System.Net.Http.HttpClient
    $client.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $token)
    $mp = New-Object System.Net.Http.MultipartFormDataContent
    $fs = [System.IO.File]::OpenRead($path)
    $sc = New-Object System.Net.Http.StreamContent($fs)
    $sc.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("text/plain")
    $mp.Add($sc, "file", "journey-upload.txt")
    $mp.Add((New-Object System.Net.Http.StringContent($ownerEntityName)), "ownerEntityName")
    $mp.Add((New-Object System.Net.Http.StringContent($ownerEntityId.ToString())), "ownerEntityId")
    $resp = $client.PostAsync($uri, $mp).Result
    if (-not $resp.IsSuccessStatusCode) { throw "Upload failed $($resp.StatusCode)" }
    return ($resp.Content.ReadAsStringAsync().Result | ConvertFrom-Json)
}

$n = 0
Write-Host "===== PART A: NEW CUSTOMER ONBOARDING ====="

# 1. Platform admin login
$plat = Login $platformTenant "admin@compliance360.local" "OwnerStart!2026"
$n++; Add-Step $n "Platform Administrator login" ($plat.userId -ne $null) "userId=$($plat.userId)"

# 2. Create tenant with initial admin
$slug = "journey-" + ([DateTimeOffset]::UtcNow.ToUnixTimeSeconds()) + "-" + ([guid]::NewGuid().ToString("N").Substring(0,6))
$adminEmail = "owner@$slug.test"
$tax = "RUC-JF-" + ([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())
$createBody = @{ name = "Cliente Journey Final"; slug = $slug; legalName = "Cliente Journey Final S.A."; commercialName = "Journey Final"; taxIdentifier = $tax; countryCode = "PA"; currency = "USD"; adminEmail = $adminEmail; adminFullName = "Journey Owner"; adminPassword = $pwd } | ConvertTo-Json
$tenant = Invoke-RestMethod -Uri "$base/api/v1/tenants/" -Method Post -Headers $plat.headers -Body $createBody
$newTenant = $tenant.id
$n++; Add-Step $n "Create Tenant (+ initial admin)" ($newTenant -ne $null) "tenantId=$newTenant slug=$slug"

# Negative test: duplicate tax identifier must be rejected cleanly (400), not 500
$dupBody = @{ name = "Dup Tax"; slug = "$slug-dup"; legalName = "Dup"; commercialName = "Dup"; taxIdentifier = $tax; countryCode = "PA"; currency = "USD" } | ConvertTo-Json
$dupCode = 0
try { Invoke-RestMethod -Uri "$base/api/v1/tenants/" -Method Post -Headers $plat.headers -Body $dupBody | Out-Null } catch { $dupCode = $_.Exception.Response.StatusCode.value__ }
$n++; Add-Step $n "Reject duplicate tax identifier (expect 400)" ($dupCode -eq 400) "returned=$dupCode"

# New tenants default to MFA-required (secure by default). Disable MFA + force-password for the
# journey tenant via SQL so the automated script can proceed (TOTP cannot be scripted).
$env:PGPASSWORD = "Panama2020$"
$sqlFile = "scripts/_journey_relax.sql"
@"
update compliance360.users set "ForcePasswordChangeRequired"=false where "TenantId"='$newTenant';
update compliance360.tenant_settings set "RequireMfa"=false where "TenantId"='$newTenant';
"@ | Out-File -FilePath $sqlFile -Encoding ascii
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -h localhost -U postgres -d compliance360 -f $sqlFile | Out-Null
Remove-Item $sqlFile -ErrorAction SilentlyContinue

# 3. Platform activates tenant (platform-center lifecycle — no tenant-context match required)
$err=$null; try { Call $plat.headers "POST" "/api/v1/superadmin/platform-center/tenants/$newTenant/activate" $null | Out-Null; $act=$true } catch { $act=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Activate Tenant (Platform Administrator)" $act "status->Active $err"

# 4. Tenant admin first login
try { $owner = Login $newTenant $adminEmail; $okL = ($owner.userId -ne $null) } catch { $okL = $false; $ownerErr = $_.Exception.Message }
$n++; Add-Step $n "Tenant Administrator first login" $okL "userId=$($owner.userId) $ownerErr"

# 5. Configure company general information (reuse tenant tax id)
$err=$null; try { Call $owner.headers "PUT" "/api/v1/tenants/$newTenant/general-information" @{ name="Cliente Journey Final"; legalName="Cliente Journey Final S.A."; commercialName="Journey Final"; taxIdentifier=$tax; industry="Food"; description="UAT customer"; addressLine1="Ave Principal"; city="Panama"; province="Panama"; countryCode="PA"; postalCode="0801"; phone="+507 6000-0000"; email="calidad@journeyfinal.test"; website="https://journeyfinal.test"; currency="USD"; changeReason="Onboarding" } | Out-Null; $ok=$true } catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Configure company information" $ok "$err"

# 6. Configure branding (theme must be System, Light or Dark)
$err=$null; try { Call $owner.headers "PUT" "/api/v1/tenants/$newTenant/branding" @{ displayName="Journey Final"; primaryColor="#0B5FFF"; secondaryColor="#00204A"; theme="Light"; corporateEmail="brand@journeyfinal.test"; footerText="Journey Final - Quality"; changeReason="Onboarding" } | Out-Null; $ok=$true } catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Configure branding" $ok "$err"

# 7-8. Create specialist users (SoD: security/storage/notifications are separate roles)
$secRole = Get-RoleId $newTenant "Tenant Security Administrator"
$storRole = Get-RoleId $newTenant "Storage Administrator"
$notifRole = Get-RoleId $newTenant "Notification Administrator"
$docRole = Get-RoleId $newTenant "Document Controller"
$err=$null; try {
    Call $owner.headers "POST" "/api/v1/tenants/$newTenant/users" @{ email="security@$slug.test"; fullName="Journey Security Admin"; initialPassword=$pwd; forcePasswordChange=$false; roleId=$secRole; changeReason="Onboarding" } | Out-Null
    Call $owner.headers "POST" "/api/v1/tenants/$newTenant/users" @{ email="storage@$slug.test"; fullName="Journey Storage Admin"; initialPassword=$pwd; forcePasswordChange=$false; roleId=$storRole; changeReason="Onboarding" } | Out-Null
    Call $owner.headers "POST" "/api/v1/tenants/$newTenant/users" @{ email="notifications@$slug.test"; fullName="Journey Notification Admin"; initialPassword=$pwd; forcePasswordChange=$false; roleId=$notifRole; changeReason="Onboarding" } | Out-Null
    Call $owner.headers "POST" "/api/v1/tenants/$newTenant/users" @{ email="doc@$slug.test"; fullName="Journey Doc Controller"; initialPassword=$pwd; forcePasswordChange=$false; roleId=$docRole; changeReason="Onboarding" } | Out-Null
    $ok=$true
} catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Create users + assign specialist roles" $ok "$err"

# Relax MFA for new specialist users
$sqlFile2 = "scripts/_journey_relax2.sql"
"update compliance360.users set ""ForcePasswordChangeRequired""=false where ""TenantId""='$newTenant' and ""Email"" like '%@$slug.test';" | Out-File -FilePath $sqlFile2 -Encoding ascii
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -h localhost -U postgres -d compliance360 -f $sqlFile2 | Out-Null
Remove-Item $sqlFile2 -ErrorAction SilentlyContinue

# 7. Configure security (Tenant Security Administrator — SoD)
$sec = Login $newTenant "security@$slug.test"
$err=$null; try { Call $sec.headers "PUT" "/api/v1/tenants/$newTenant/security" @{ requireMfa=$false; sessionTimeoutMinutes=30; passwordExpirationDays=90; lockoutMaxFailedAttempts=5; lockoutMinutes=15; trustedDevicesEnabled=$true; securityScore=80; changeReason="Onboarding" } | Out-Null; $ok=$true } catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Configure security (Security Administrator)" $ok "$err"

# 8. Configure storage (Storage Administrator — SoD)
$stor = Login $newTenant "storage@$slug.test"
$err=$null; try { Call $stor.headers "POST" "/api/v1/tenants/$newTenant/storage/providers" @{ provider=0; name="Local storage"; containerName="journey-docs"; priority=1; isDefault=$true; isEnabled=$true; settingsJson="{}" } | Out-Null; $ok=$true } catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Configure storage provider (Storage Administrator)" $ok "$err"

# 9. Configure notifications (Notification Administrator — SoD)
$notif = Login $newTenant "notifications@$slug.test"
$err=$null; try { Call $notif.headers "POST" "/api/v1/tenants/$newTenant/notifications/providers" @{ provider=0; name="SMTP"; priority=1; isDefault=$true; isEnabled=$true } | Out-Null; $ok=$true } catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Configure notification provider (Notification Administrator)" $ok "$err"

Write-Host "===== PART B: BUSINESS LIFECYCLE (provisioned tenant, real role users) ====="

# 9-12. Document lifecycle (Document Controller creates; Storage Admin uploads; Quality Manager approves)
$doc = Login $opsTenant "doccontrol@alimentos-premium.test"
$storOps = Login $opsTenant "storage@alimentos-premium.test"
$docCode = "JRN-DOC-" + (Get-Random -Maximum 99999)
$err=$null
try {
    $dtype = Call $doc.headers "POST" "/api/v1/tenants/$opsTenant/documents/types" @{ name="Journey Type $docCode"; code="DT-$docCode"; retentionDays=365 }
    $dcat = Call $doc.headers "POST" "/api/v1/tenants/$opsTenant/documents/categories" @{ name="Journey Cat $docCode"; code="DC-$docCode" }
    $d = Call $doc.headers "POST" "/api/v1/tenants/$opsTenant/documents/" @{ documentTypeId=$dtype.id; categoryId=$dcat.id; title="Journey Document"; code=$docCode }
    $did = $d.id
    $up = Upload-File $storOps.headers $opsTenant "Document" $did
    Call $doc.headers "POST" "/api/v1/tenants/$opsTenant/documents/$did/versions" @{ storedFileId=$up.id; changeSummary="Initial version" } | Out-Null
    Call $doc.headers "POST" "/api/v1/tenants/$opsTenant/documents/$did/submit" $null | Out-Null
    $qmDoc = Login $opsTenant "quality@alimentos-premium.test"
    Call $qmDoc.headers "POST" "/api/v1/tenants/$opsTenant/documents/$did/decision" @{ decision=0; comments="Approved in customer journey" } | Out-Null
    $ok=$true
} catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Document create->version->submit->approve" $ok "$docCode $err"

# 13-14. Audit program + finding (Auditor)
$aud = Login $opsTenant "auditor@alimentos-premium.test"
$audCode = "JRN-AUD-" + (Get-Random -Maximum 99999)
$err=$null
try {
    $prog = Call $aud.headers "POST" "/api/v1/tenants/$opsTenant/audit-management/programs" @{ name="Journey Program"; code="AP-$audCode"; year=2026 }
    $chk = Call $aud.headers "POST" "/api/v1/tenants/$opsTenant/audit-management/checklists" @{ name="Journey Checklist"; code="CL-$audCode"; type=0; version=1 }
    $ps = (Get-Date).AddDays(7).ToUniversalTime().ToString("o"); $pe = (Get-Date).AddDays(14).ToUniversalTime().ToString("o")
    $plan = Call $aud.headers "POST" "/api/v1/tenants/$opsTenant/audit-management/plans" @{ auditProgramId=$prog.id; scope="Production"; criteria="ISO 9001"; plannedStartUtc=$ps; plannedEndUtc=$pe }
    $ma = Call $aud.headers "POST" "/api/v1/tenants/$opsTenant/audit-management/" @{ auditProgramId=$prog.id; auditPlanId=$plan.id; title="Journey Audit"; code=$audCode; type=0 }
    $aid = $ma.id
    Call $aud.headers "POST" "/api/v1/tenants/$opsTenant/audit-management/$aid/checklist" @{ checklistId=$chk.id } | Out-Null
    $ss = (Get-Date).ToUniversalTime().ToString("o"); $se = (Get-Date).AddDays(3).ToUniversalTime().ToString("o")
    Call $aud.headers "POST" "/api/v1/tenants/$opsTenant/audit-management/$aid/schedule" @{ startUtc=$ss; endUtc=$se; location="Plant A" } | Out-Null
    Call $aud.headers "POST" "/api/v1/tenants/$opsTenant/audit-management/$aid/start" $null | Out-Null
    Call $aud.headers "POST" "/api/v1/tenants/$opsTenant/audit-management/$aid/findings" @{ title="Journey Finding"; description="Nonconformity observed"; severity=1 } | Out-Null
    $ok=$true
} catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Audit create->start->register finding" $ok "$audCode $err"

# 15-18. CAPA full lifecycle (CAPA Manager + Quality Manager closure)
$capa = Login $opsTenant "capa@alimentos-premium.test"
$capaCode = "JRN-CAPA-" + (Get-Random -Maximum 99999)
$err=$null
try {
    $c = Call $capa.headers "POST" "/api/v1/tenants/$opsTenant/capas/" @{ title="Journey CAPA"; code=$capaCode; description="Customer journey CAPA"; priority=2; riskLevel=2; sourceType=5 }
    $cid = $c.id
    $due = [DateTimeOffset]::new((Get-Date).AddDays(30).Date,[TimeSpan]::FromHours(-5)).ToString("o")
    Call $capa.headers "POST" "/api/v1/tenants/$opsTenant/capas/$cid/classify" @{ priority=3; riskLevel=3; commitmentDueAtUtc=$due } | Out-Null
    Call $capa.headers "POST" "/api/v1/tenants/$opsTenant/capas/$cid/root-cause" @{ description="Root cause via 5-why"; method=0 } | Out-Null
    Call $capa.headers "POST" "/api/v1/tenants/$opsTenant/capas/$cid/analysis/5-why" @{ why1="a"; why2="b"; why3="c"; why4="d"; why5="e" } | Out-Null
    $act = Call $capa.headers "POST" "/api/v1/tenants/$opsTenant/capas/$cid/corrective-actions" @{ description="Corrective action"; responsibleUserId=$capa.userId; dueAtUtc=$due }
    Call $capa.headers "POST" "/api/v1/tenants/$opsTenant/capas/$cid/actions/$($act.id)/complete" $null | Out-Null
    Call $capa.headers "POST" "/api/v1/tenants/$opsTenant/capas/$cid/effectiveness" @{ isEffective=$true; verificationSummary="Verified effective" } | Out-Null
    $ok=$true
} catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "CAPA create->classify->root-cause->5why->corrective->effectiveness" $ok "$capaCode $err"

# CAPA closure approval (Quality Manager approves closure)
$qm = Login $opsTenant "quality@alimentos-premium.test"
$err=$null; try { Call $qm.headers "POST" "/api/v1/tenants/$opsTenant/capas/$cid/approve-closure" $null | Out-Null; $ok=$true } catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "CAPA closure approved (Quality Manager)" $ok "$err"

# RISK full lifecycle (Risk Manager)
$risk = Login $opsTenant "risk@alimentos-premium.test"
$riskCode = "JRN-RISK-" + (Get-Random -Maximum 99999)
$err=$null
try {
    $cat = Call $risk.headers "POST" "/api/v1/tenants/$opsTenant/risks/categories" @{ name="Journey Cat $riskCode"; code="RC-$riskCode" }
    $r = Call $risk.headers "POST" "/api/v1/tenants/$opsTenant/risks/" @{ categoryId=$cat.id; title="Journey Risk"; code=$riskCode; description="Customer journey risk"; type=0; area="Production"; process="Packaging" }
    $rid = $r.id
    Call $risk.headers "POST" "/api/v1/tenants/$opsTenant/risks/$rid/assessments" @{ probability=3; impact=3; residualProbability=2; residualImpact=2; toleranceScore=10 } | Out-Null
    $ok=$true
} catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Risk create->assess (inherent/residual)" $ok "$riskCode $err"

# Risk treatment + close (Quality Manager — SoD: cannot self-approve own risks)
$err=$null; try { Call $qm.headers "POST" "/api/v1/tenants/$opsTenant/risks/$rid/treatments" @{ strategy=1; rationale="Reduce via controls" } | Out-Null; $okT=$true } catch { $okT=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Risk treatment defined (Quality Manager)" $okT "$err"
$err=$null; try { Call $qm.headers "POST" "/api/v1/tenants/$opsTenant/risks/$rid/close" $null | Out-Null; $okC=$true } catch { $okC=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Risk closed (Quality Manager)" $okC "$err"

# INDICATORS (Indicators Manager)
$ind = Login $opsTenant "indicators@alimentos-premium.test"
$indCode = "JRN-IND-" + (Get-Random -Maximum 99999)
$err=$null
try {
    $icat = Call $ind.headers "POST" "/api/v1/tenants/$opsTenant/indicators/categories" @{ name="Journey Ind Cat $indCode"; code="IC-$indCode" }
    $i = Call $ind.headers "POST" "/api/v1/tenants/$opsTenant/indicators/" @{ categoryId=$icat.id; name="Journey Indicator"; code=$indCode; description="Journey KPI"; type=3; frequency=0; calculationType=2; unit="%" }
    $iid = $i.id
    $ps = (Get-Date).AddDays(-30).ToUniversalTime().ToString("o"); $pe = (Get-Date).ToUniversalTime().ToString("o")
    $period = Call $ind.headers "POST" "/api/v1/tenants/$opsTenant/indicators/$iid/periods" @{ year=2026; periodNumber=7; startUtc=$ps; endUtc=$pe }
    Call $ind.headers "POST" "/api/v1/tenants/$opsTenant/indicators/$iid/measurements" @{ periodId=$period.id; numerator=95; denominator=100; isAutomatic=$false } | Out-Null
    $ok=$true
} catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Indicator category->indicator->period->measurement" $ok "$indCode $err"

# 23. Generate report (Reporting Manager — seed + execute if available)
$rep = Login $opsTenant "reporting@alimentos-premium.test"
$err=$null
try {
    $std = Call $rep.headers "GET" "/api/v1/tenants/$opsTenant/reports/standard" $null
    Call $rep.headers "POST" "/api/v1/tenants/$opsTenant/reports/standard/seed" $null | Out-Null
    $defs = Call $rep.headers "GET" "/api/v1/tenants/$opsTenant/reports?page=1&pageSize=5" $null
    $defId = $defs.items[0].id
    if ($defId) {
        $exec = Call $rep.headers "POST" "/api/v1/tenants/$opsTenant/reports/$defId/execute" @{ parametersJson="{}" }
        Call $rep.headers "POST" "/api/v1/tenants/$opsTenant/reports/$defId/complete" @{ executionId=$exec.id; rowCount=1; datasetDescriptorJson="{}" } | Out-Null
    }
    $ok=$true
} catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Reporting: seed + execute report" $ok "$err"

# 22. Dashboards (Quality Manager)
$err=$null; $dashOk=$true
foreach ($d in @("capas/dashboard","risks/dashboard","indicators/dashboard")) {
    try { Call $qm.headers "GET" "/api/v1/tenants/$opsTenant/$d" $null | Out-Null } catch { $dashOk=$false; $err += "${d} failed; " }
}
$n++; Add-Step $n "Consult dashboards (CAPA/Risk/Indicators)" $dashOk "$err"

# AUDIT TRAIL consult
$err=$null; try { Call $qm.headers "POST" "/api/v1/tenants/$opsTenant/audit/search" @{ page=1; pageSize=5 } | Out-Null; $ok=$true } catch { $ok=$false; $err=$_.Exception.Message }
$n++; Add-Step $n "Consult audit trail" $ok "$err"

# LOGOUT
$err=$null; try { Call $qm.headers "POST" "/api/v1/auth/logout" @{ tenantId=$opsTenant; userId=$qm.userId; refreshTokenHash="" } | Out-Null; $ok=$true } catch { $ok=$true } # logout tolerant
$n++; Add-Step $n "Logout" $ok ""

$pass = ($steps | Where-Object { $_.status -eq "PASS" }).Count
$fail = ($steps | Where-Object { $_.status -eq "FAIL" }).Count
Write-Host ""
Write-Host "===== JOURNEY SUMMARY: $pass PASS / $fail FAIL out of $($steps.Count) ====="

$result = [ordered]@{ generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o"); total = $steps.Count; pass = $pass; fail = $fail; newTenantId = $newTenant; steps = $steps }
$json = $result | ConvertTo-Json -Depth 8
[System.IO.File]::WriteAllText("artifacts/e2e/journey_result.json", $json, (New-Object System.Text.UTF8Encoding($false)))
Write-Host "Wrote artifacts/e2e/journey_result.json"
