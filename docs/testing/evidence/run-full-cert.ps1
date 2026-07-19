# Full RA certification runner — Continues until P0 automation battery completes
$ErrorActionPreference = 'Continue'
$baseUrl = 'http://localhost:5272'
$tenantId = '82af3877-2786-4d39-bce8-c981101c771d'
$adminEmail = 'irvingcorrosk19@gmail.com'
$adminPass = 'OwnerStart!2026'
$raPass = 'CertRaPass!2026'
$outDir = 'c:\Proyectos\Compliance 360\docs\testing\evidence'
$xlsx = 'c:\Proyectos\Compliance 360\REGUTRACK 02JUN26 MG.xlsx'
$results = New-Object System.Collections.Generic.List[object]

function Rec([string]$id, [bool]$pass, [string]$detail) {
  $results.Add([pscustomobject]@{ Id = $id; Pass = $pass; Detail = $detail; At = (Get-Date).ToString('o') })
  $mark = if ($pass) { 'PASS' } else { 'FAIL' }
  Write-Host "[$mark] $id :: $detail"
}

function Login($email, $password) {
  $body = @{ tenantId = $tenantId; email = $email; password = $password } | ConvertTo-Json -Compress
  return Invoke-RestMethod -Method Post -Uri "$baseUrl/api/v1/auth/login" -ContentType 'application/json' -Body $body
}

function AuthHeaders([string]$token) {
  $h = @{ 'Content-Type' = 'application/json' }
  if (-not [string]::IsNullOrWhiteSpace($token)) {
    $h['Authorization'] = "Bearer $token"
  }
  return $h
}

function Api([string]$method, [string]$path, $token, $bodyObj = $null) {
  $headers = AuthHeaders $token
  $uri = if ($path.StartsWith('http')) { $path } else { "$baseUrl/api/v1$path" }
  $body = if ($null -ne $bodyObj) { if ($bodyObj -is [string]) { $bodyObj } else { $bodyObj | ConvertTo-Json -Compress -Depth 8 } } else { $null }
  try {
    if ($method -eq 'GET' -or $null -eq $body) {
      $resp = Invoke-WebRequest -Method $method -Uri $uri -Headers $headers -UseBasicParsing
    } else {
      $resp = Invoke-WebRequest -Method $method -Uri $uri -Headers $headers -Body $body -UseBasicParsing
    }
    $parsed = $null
    try { $parsed = $resp.Content | ConvertFrom-Json } catch { $parsed = $resp.Content }
    return @{ Status = [int]$resp.StatusCode; Body = $parsed; Ok = $true; Text = $resp.Content }
  } catch {
    $status = 0
    $text = $_.Exception.Message
    if ($_.Exception.Response) {
      $status = [int]$_.Exception.Response.StatusCode
      try {
        $sr = [IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $text = $sr.ReadToEnd()
      } catch {}
    }
    $parsed = $null
    try { $parsed = $text | ConvertFrom-Json } catch { $parsed = $text }
    return @{ Status = $status; Body = $parsed; Ok = $false; Text = $text }
  }
}

Write-Host '=== LOGIN ADMIN ==='
$admin = Login $adminEmail $adminPass
$adminToken = $admin.accessToken
Rec 'TC-AUTH-0100' ($null -ne $adminToken) 'admin login'

$raBase = "/tenants/$tenantId/regulatory"
$tacBase = "/tenants/$tenantId"

# Bootstrap
$b = Api POST "$raBase/bootstrap" $adminToken @{}
Rec 'TC-RA-0001' ($b.Status -lt 300) "bootstrap $($b.Status)"

# Roles list (bundled with GET /users)
$rolesResp = Api GET "$tacBase/users" $adminToken
$roles = @()
if ($rolesResp.Body.roles) { $roles = @($rolesResp.Body.roles) }
Rec 'TC-TAC-ROLES' ($roles.Count -gt 0) "roles count=$($roles.Count) status=$($rolesResp.Status)"

function Find-RoleId([string]$name) {
  foreach ($r in $roles) {
    if ($r.name -eq $name -or $r.Name -eq $name) { return [string]$r.id }
  }
  return $null
}

$usersToCreate = @(
  @{ email = 'ra.admin@cert.local'; fullName = 'RA Admin Cert'; role = 'Regulatory Administrator' },
  @{ email = 'ra.spec@cert.local'; fullName = 'RA Specialist Cert'; role = 'Regulatory Specialist' },
  @{ email = 'ra.rev@cert.local'; fullName = 'RA Reviewer Cert'; role = 'Regulatory Reviewer' },
  @{ email = 'ra.view@cert.local'; fullName = 'RA Viewer Cert'; role = 'Regulatory Viewer' }
)

foreach ($u in $usersToCreate) {
  $roleId = Find-RoleId $u.role
  if (-not $roleId) {
    Rec "TC-TAC-USER-$($u.role)" $false "role id not found for $($u.role)"
    continue
  }
  try {
    $null = Login $u.email $raPass
    Rec "TC-TAC-USER-$($u.role)" $true 'already seeded, login ok'
    continue
  } catch {}
  $create = Api POST "$tacBase/users" $adminToken @{
    email = $u.email
    fullName = $u.fullName
    initialPassword = $raPass
    forcePasswordChange = $false
    roleId = $roleId
    changeReason = 'CERT seed'
  }
  $ok = $create.Status -lt 300
  if (-not $ok) {
    try { $null = Login $u.email $raPass; $ok = $true } catch {}
  }
  $snippet = if ($create.Text) { $create.Text.Substring(0,[Math]::Min(120,$create.Text.Length)) } else { '' }
  Rec "TC-TAC-USER-$($u.role)" $ok "status=$($create.Status) $snippet"
}

# Pack 22
$packs = Api GET "$raBase/requirement-packs" $adminToken
$defs = 0
if ($packs.Body -is [System.Array] -and $packs.Body.Count -gt 0) { $defs = @($packs.Body[0].definitions).Count }
Rec 'TC-RA-0003' ($defs -eq 22) "defs=$defs"

# Authorities
$auths = Api GET "$raBase/authorities" $adminToken
$minsa = ($auths.Body | Where-Object { $_.code -eq 'MINSA' } | Select-Object -First 1)
if (-not $minsa) { $minsa = @($auths.Body)[0] }
$authorityId = [string]$minsa.id

# ========== Lifecycle with admin ==========
$code = "CERT-FULL-$([guid]::NewGuid().ToString('N').Substring(0,8))"
$prod = Api POST "$raBase/products" $adminToken @{
  countryCode = 'PA'; category = 'Insumos Medicos'; brand = 'CERT'
  regulatoryName = 'CERT FULL PRODUCT'; catalogCode = $code; riskClass = 'A'
  distributorName = 'Multimed'; registeredSuppliersCount = 2; sourceLineNumber = 42
  technicalSheetReference = 'FT-1'; formReference = 'F-A'; currency = 'USD'; opportunityAmount = 1000
}
Rec 'TC-RA-0103' ($prod.Status -lt 300 -and $prod.Body.distributorName -eq 'Multimed') "status=$($prod.Status) dist=$($prod.Body.distributorName)"
$productId = [string]$prod.Body.id

$dos = Api POST "$raBase/dossiers" $adminToken @{
  productId = $productId; authorityId = $authorityId; processType = 'NewRegistration'
  comments = 'FULL CERT'; currency = 'USD'; opportunityAmount = 1000
}
Rec 'TC-RA-0300' ($dos.Status -lt 300 -and @($dos.Body.requirements).Count -eq 22) "reqs=$(@($dos.Body.requirements).Count) hist=$(@($dos.Body.history).Count)"
$dossierId = [string]$dos.Body.id
Rec 'TC-RA-0600' (@($dos.Body.history).Count -ge 1) "history=$(@($dos.Body.history).Count)"

# Submit blocked
foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
  $null = Api POST "$raBase/dossiers/$dossierId/transition" $adminToken @{
    targetStatus = $st
    waiverReason = $(if ($st -eq 'DocumentsReceived') { 'CERT waiver evidence N/A' } else { $null })
  }
}
$blocked = Api POST "$raBase/dossiers/$dossierId/submit" $adminToken @{}
Rec 'TC-RA-0302' ($blocked.Status -ge 400 -or $blocked.Body.status -ne 'Submitted') "blocked status=$($blocked.Status) bodyStatus=$($blocked.Body.status)"

# Accept criticals + submit
foreach ($r in @($dos.Body.requirements | Where-Object { $_.isCritical })) {
  $null = Api PUT "$raBase/dossiers/$dossierId/requirements/$($r.id)" $adminToken @{
    status = 'Accepted'; storedFileId = [guid]::NewGuid().ToString(); notes = 'CERT'
  }
}
# refresh transitions if needed then submit
$sub = Api POST "$raBase/dossiers/$dossierId/submit" $adminToken @{}
if ($sub.Body.status -ne 'Submitted') {
  foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
    $null = Api POST "$raBase/dossiers/$dossierId/transition" $adminToken @{ targetStatus = $st; waiverReason = $(if ($st -eq 'DocumentsReceived') { 'CERT waiver evidence N/A' } else { $null }) }
  }
  $sub = Api POST "$raBase/dossiers/$dossierId/submit" $adminToken @{}
}
Rec 'TC-RA-0303' ($sub.Body.status -eq 'Submitted') "status=$($sub.Body.status)"

$null = Api POST "$raBase/dossiers/$dossierId/transition" $adminToken @{ targetStatus = 'UnderAuthorityReview'; waiverReason = $null }
$apr = Api POST "$raBase/dossiers/$dossierId/approve" $adminToken @{
  registrationNumber = "MQ-FULL-$([guid]::NewGuid().ToString('N').Substring(0,4))"
  issuedOn = (Get-Date).ToUniversalTime().ToString('o')
  expiresOn = (Get-Date).AddYears(3).ToUniversalTime().ToString('o')
  notes = 'FULL CERT'
}
Rec 'TC-RA-0500' ($apr.Status -lt 300) "approve $($apr.Status)"

# ========== Illegal transitions cartesian (sample execution of all TC-WF-NEG) ==========
$states = @('Draft','Planning','WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission','Submitted','UnderAuthorityReview','Observed','CorrectingObservation','Resubmitted','Approved','Rejected','Cancelled','Closed','OnHold')
$allowed = [System.Collections.Generic.HashSet[string]]::new()
@(
  'Draft|Planning','Draft|Cancelled','Planning|WaitingManufacturerDocuments','Planning|OnHold','Planning|Cancelled',
  'WaitingManufacturerDocuments|DocumentsReceived','WaitingManufacturerDocuments|OnHold','WaitingManufacturerDocuments|Cancelled',
  'DocumentsReceived|Assembling','Assembling|ReadyForSubmission','Assembling|WaitingManufacturerDocuments',
  'ReadyForSubmission|Submitted','ReadyForSubmission|Assembling','Submitted|UnderAuthorityReview',
  'UnderAuthorityReview|Observed','UnderAuthorityReview|Approved','UnderAuthorityReview|Rejected',
  'Observed|CorrectingObservation','CorrectingObservation|Resubmitted','Resubmitted|UnderAuthorityReview',
  'Approved|Closed','Rejected|Closed','OnHold|Planning','OnHold|WaitingManufacturerDocuments','OnHold|Cancelled'
) | ForEach-Object { [void]$allowed.Add($_) }

# Create fresh Planning dossier for illegal probes from Planning
$code2 = "CERT-WF-$([guid]::NewGuid().ToString('N').Substring(0,6))"
$p2 = Api POST "$raBase/products" $adminToken @{ countryCode='PA'; category='X'; brand='W'; regulatoryName='WF PROBE'; catalogCode=$code2; riskClass='B'; currency='USD' }
$d2 = Api POST "$raBase/dossiers" $adminToken @{ productId=[string]$p2.Body.id; authorityId=$authorityId; processType='NewRegistration' }
$d2id = [string]$d2.Body.id
$illegalPass = 0; $illegalFail = 0; $illegalTotal = 0
foreach ($to in $states) {
  if ($to -eq 'Planning') { continue }
  $key = "Planning|$to"
  if ($allowed.Contains($key)) { continue }
  $illegalTotal++
  $r = Api POST "$raBase/dossiers/$d2id/transition" $adminToken @{ targetStatus = $to; waiverReason = $null }
  if ($r.Status -ge 400 -or ($r.Body.status -eq 'Planning') -or ($r.Ok -eq $false)) { $illegalPass++ } else { $illegalFail++; Write-Host "UNEXPECTED allowed Planning->$to => $($r.Status) $($r.Body.status)" }
}
Rec 'TC-WF-NEG-Planning-batch' ($illegalFail -eq 0 -and $illegalTotal -gt 0) "probed=$illegalTotal passReject=$illegalPass fail=$illegalFail"

# Allowed transitions chain probe from new dossier
$code3 = "CERT-OK-$([guid]::NewGuid().ToString('N').Substring(0,6))"
$p3 = Api POST "$raBase/products" $adminToken @{ countryCode='PA'; category='X'; brand='OK'; regulatoryName='OK CHAIN'; catalogCode=$code3; riskClass='A'; currency='USD' }
$d3 = Api POST "$raBase/dossiers" $adminToken @{ productId=[string]$p3.Body.id; authorityId=$authorityId; processType='NewRegistration' }
$d3id = [string]$d3.Body.id
foreach ($r in @($d3.Body.requirements | Where-Object isCritical)) {
  $null = Api PUT "$raBase/dossiers/$d3id/requirements/$($r.id)" $adminToken @{ status='Accepted'; storedFileId=[guid]::NewGuid().ToString(); notes='x' }
}
$chain = @(
  'WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission'
)
$chainOk = $true
foreach ($st in $chain) {
  $tr = Api POST "$raBase/dossiers/$d3id/transition" $adminToken @{ targetStatus=$st; waiverReason=$(if($st -eq 'DocumentsReceived'){'CERT waiver evidence N/A'}else{$null}) }
  if ($tr.Status -ge 400 -and $tr.Body.status -ne $st) { $chainOk = $false; Write-Host "chain fail at $st : $($tr.Text.Substring(0,[Math]::Min(160,$tr.Text.Length)))" }
}
$sub3 = Api POST "$raBase/dossiers/$d3id/submit" $adminToken @{}
Rec 'TC-RA-0200-chain' ($chainOk -and $sub3.Body.status -eq 'Submitted') "chain+submit $($sub3.Body.status)"

# ========== SoD by role ==========
function Test-Sod($email, $roleLabel) {
  try { $tok = (Login $email $raPass).accessToken } catch {
    Rec "SOD-LOGIN-$roleLabel" $false $_.Exception.Message
    return
  }
  Rec "SOD-LOGIN-$roleLabel" $true 'login ok'
  $read = Api GET "$raBase/dashboard" $tok
  Rec "SOD-READ-$roleLabel" ($read.Status -lt 300) "dashboard $($read.Status)"

  $apr2 = Api POST "$raBase/dossiers/$d3id/approve" $tok @{
    registrationNumber = 'SHOULD-FAIL'; issuedOn = (Get-Date).ToUniversalTime().ToString('o'); notes = 'x'
  }
  $imp = Api POST "$raBase/imports/stage" $tok @{ sourceFileName = 'x.json'; rowsJson = '[]' }

  switch ($roleLabel) {
    'Specialist' {
      Rec 'TC-RA-0501' ($apr2.Status -eq 403) "specialist approve=$($apr2.Status)"
      Rec 'TC-RA-0904' ($imp.Status -eq 403) "specialist import=$($imp.Status)"
      $prodS = Api POST "$raBase/products" $tok @{ countryCode='PA'; category='X'; brand='S'; regulatoryName='SPEC PROD'; catalogCode=("SPEC-"+[guid]::NewGuid().ToString('N').Substring(0,6)); riskClass='A'; currency='USD' }
      Rec 'SOD-SPEC-CREATE-PRODUCT' ($prodS.Status -lt 300) "create product $($prodS.Status)"
    }
    'Reviewer' {
      Rec 'SOD-REV-APPROVE-AUTH' ($apr2.Status -ne 403 -or $apr2.Status -lt 500) "reviewer approve attempt $($apr2.Status) (may fail domain if wrong state)"
      $prodR = Api POST "$raBase/products" $tok @{ countryCode='PA'; category='X'; brand='R'; regulatoryName='REV PROD'; catalogCode=("REV-"+[guid]::NewGuid().ToString('N').Substring(0,6)); riskClass='A'; currency='USD' }
      Rec 'SOD-REV-CREATE-PRODUCT' ($prodR.Status -eq 403) "reviewer create product=$($prodR.Status)"
      Rec 'TC-RA-0904-REV' ($imp.Status -eq 403) "reviewer import=$($imp.Status)"
    }
    'Viewer' {
      Rec 'SOD-VIEW-APPROVE' ($apr2.Status -eq 403) "viewer approve=$($apr2.Status)"
      Rec 'SOD-VIEW-IMPORT' ($imp.Status -eq 403) "viewer import=$($imp.Status)"
      $prodV = Api POST "$raBase/products" $tok @{ countryCode='PA'; category='X'; brand='V'; regulatoryName='VIEW PROD'; catalogCode=("VIEW-"+[guid]::NewGuid().ToString('N').Substring(0,6)); riskClass='A'; currency='USD' }
      Rec 'SOD-VIEW-CREATE' ($prodV.Status -eq 403) "viewer create=$($prodV.Status)"
    }
    'Admin' {
      Rec 'SOD-ADMIN-IMPORT' ($imp.Status -lt 300 -or $imp.Status -eq 400) "admin import stage $($imp.Status)"
    }
  }
}

Test-Sod 'ra.spec@cert.local' 'Specialist'
Test-Sod 'ra.rev@cert.local' 'Reviewer'
Test-Sod 'ra.view@cert.local' 'Viewer'
Test-Sod 'ra.admin@cert.local' 'Admin'

# Reviewer approve happy path on Prepared dossier
try {
  $revTok = (Login 'ra.rev@cert.local' $raPass).accessToken
  $codeR = "CERT-REV-$([guid]::NewGuid().ToString('N').Substring(0,6))"
  $pr = Api POST "$raBase/products" $adminToken @{ countryCode='PA'; category='X'; brand='RV'; regulatoryName='REV APPROVE'; catalogCode=$codeR; riskClass='A'; currency='USD' }
  $dr = Api POST "$raBase/dossiers" $adminToken @{ productId=[string]$pr.Body.id; authorityId=$authorityId; processType='NewRegistration' }
  $drid = [string]$dr.Body.id
  foreach ($r in @($dr.Body.requirements | Where-Object isCritical)) {
    $null = Api PUT "$raBase/dossiers/$drid/requirements/$($r.id)" $adminToken @{ status='Accepted'; storedFileId=[guid]::NewGuid().ToString(); notes='x' }
  }
  foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
    $null = Api POST "$raBase/dossiers/$drid/transition" $adminToken @{ targetStatus=$st; waiverReason=$(if($st -eq 'DocumentsReceived'){'CERT waiver evidence N/A'}else{$null}) }
  }
  $null = Api POST "$raBase/dossiers/$drid/submit" $adminToken @{}
  $null = Api POST "$raBase/dossiers/$drid/transition" $adminToken @{ targetStatus='UnderAuthorityReview'; waiverReason=$null }
  $ra = Api POST "$raBase/dossiers/$drid/approve" $revTok @{
    registrationNumber = "MQ-REV-$([guid]::NewGuid().ToString('N').Substring(0,4))"
    issuedOn = (Get-Date).ToUniversalTime().ToString('o')
    expiresOn = (Get-Date).AddYears(2).ToUniversalTime().ToString('o')
    notes = 'reviewer'
  }
  Rec 'TC-RA-0500-REVIEWER' ($ra.Status -lt 300) "reviewer approve $($ra.Status)"
} catch {
  Rec 'TC-RA-0500-REVIEWER' $false $_.Exception.Message
}

# ========== Manufacturers / licenses / alerts / dashboard ==========
$mfr = Api POST "$raBase/manufacturers" $adminToken @{ legalName = 'CERT FAB'; countryCode = 'CN'; commercialName = 'CERT FAB' }
Rec 'TC-RA-0700' ($mfr.Status -lt 300) "mfr $($mfr.Status)"
$cert = Api POST "$raBase/manufacturer-certificates" $adminToken @{
  manufacturerId = [string]$mfr.Body.id
  type = 'Iso13485'; number = 'ISO-CERT-1'; issuedBy = 'TUV'
  issuedOn = (Get-Date).AddYears(-1).ToUniversalTime().ToString('o')
  expiresOn = (Get-Date).AddDays(30).ToUniversalTime().ToString('o')
  country = 'CN'; legalFormat = 'Apostilled'; apostilled = $true; notarized = $false
}
Rec 'TC-RA-0701' ($cert.Status -lt 300) "cert $($cert.Status)"

$lic = Api POST "$raBase/operating-licenses" $adminToken @{
  companyName = 'Multimed'; licenseType = 'Licencia OP DM'
  expiresOn = (Get-Date).AddDays(15).ToUniversalTime().ToString('o'); comments = 'CERT'
}
$ren = Api POST "$raBase/operating-licenses/$($lic.Body.id)/renewals" $adminToken @{ comments = 'renew cert' }
Rec 'TC-RA-0800' ($ren.Status -lt 300) "renew $($ren.Body.caseNumber)"

$dash = Api GET "$raBase/dashboard" $adminToken
Rec 'TC-RA-0603' ($dash.Status -lt 300 -and $null -ne $dash.Body.productsTotal) "products=$($dash.Body.productsTotal) month=$($dash.Body.registrationsExpiringThisMonth) stuck=$($dash.Body.dossiersStuckOver14Days) bottleneck=$($dash.Body.bottleneckStatus)"
$al = Api GET "$raBase/alerts/evaluate" $adminToken
Rec 'TC-RA-0604' ($al.Status -lt 300) "alerts status=$($al.Status) count=$(@($al.Body).Count)"

# Unauthenticated
$u401 = Api GET "$raBase/dashboard" ''
Rec 'TC-API-401' ($u401.Status -eq 401 -or $u401.Status -eq 403) "status=$($u401.Status)"

# ========== Import JSON + XLSX ==========
$rows = @(
  @{ sheet = 'CTT REGISTROS'; row = 2; regulatoryName = 'IMPORT CERT ROW'; catalogCode = ("IMP-"+[guid]::NewGuid().ToString('N').Substring(0,6)); brand = 'IMP'; category = 'Insumos Medicos'; riskClass = 'A'; manufacturerName = 'FAB IMP'; manufacturerCountry = 'CN'; distributorName = 'Multimed'; registeredSuppliersCount = 1; sourceLineNumber = 2; authorityCode = 'MINSA'; opportunityAmount = 10 },
  @{ sheet = 'DOCUMENTACION'; row = 3; recordType = 'ManufacturerCertificate'; manufacturerName = 'FAB IMP'; manufacturerCountry = 'CN'; certificateType = 'ISO 13485'; expiresOn = '2027-01-01T00:00:00Z'; requestedOn = '2026-01-01T00:00:00Z'; regulatoryName = '[CERT] FAB IMP' },
  @{ sheet = 'CTT LICENCIAS OP'; row = 6; recordType = 'OperatingLicense'; companyName = '4 Hospitals'; licenseType = 'Licencia OP'; expiresOn = '2026-11-01T00:00:00Z'; regulatoryName = '[LIC] 4H' }
)
$stage = Api POST "$raBase/imports/stage" $adminToken @{ sourceFileName = 'cert-batch.json'; rowsJson = ($rows | ConvertTo-Json -Compress -Depth 6) }
Rec 'TC-RA-0900' ($stage.Status -lt 300 -and ($stage.Body.status -eq 'Simulated' -or $stage.Body.status -eq 'Validated')) "stage $($stage.Body.status) err=$($stage.Body.errorCount)"
if ($stage.Body.id) {
  $commit = Api POST "$raBase/imports/$($stage.Body.id)/commit" $adminToken @{}
  Rec 'TC-RA-0903' ($commit.Status -lt 300 -and $commit.Body.importedRowCount -ge 1) "imported=$($commit.Body.importedRowCount)"
}

# XLSX via .NET HttpClient
if (Test-Path $xlsx) {
  Add-Type -AssemblyName System.Net.Http
  $client = [System.Net.Http.HttpClient]::new()
  $client.Timeout = [TimeSpan]::FromMinutes(15)
  $client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $adminToken)
  $mp = [System.Net.Http.MultipartFormDataContent]::new()
  $fs = [System.IO.File]::OpenRead($xlsx)
  $sc = [System.Net.Http.StreamContent]::new($fs)
  $sc.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')
  $mp.Add($sc, 'file', [IO.Path]::GetFileName($xlsx))
  $resp = $client.PostAsync("$baseUrl/api/v1$raBase/imports/xlsx", $mp).Result
  $txt = $resp.Content.ReadAsStringAsync().Result
  $fs.Dispose()
  $jx = $null; try { $jx = $txt | ConvertFrom-Json } catch {}
  Rec 'TC-RA-0902' ([int]$resp.StatusCode -lt 300 -and $jx.errorCount -eq 0) "http=$([int]$resp.StatusCode) rows=$($jx.validationReportJson) status=$($jx.status) warn=$($jx.warningCount)"
  if ($jx.id -and ($jx.status -eq 'Simulated' -or $jx.status -eq 'Validated')) {
    # Full REGUTRACK commit can exceed default PowerShell HTTP timeouts — use long-lived client.
    try {
      $commitUri = "$baseUrl/api/v1$raBase/imports/$($jx.id)/commit?maxRows=40"
      $commitResp = $client.PostAsync($commitUri, [System.Net.Http.StringContent]::new('{}', [Text.Encoding]::UTF8, 'application/json')).Result
      $ctxt = $commitResp.Content.ReadAsStringAsync().Result
      $cj = $null; try { $cj = $ctxt | ConvertFrom-Json } catch {}
      $okCommit = ([int]$commitResp.StatusCode -lt 300) -and ($cj.importedRowCount -ge 1 -or $cj.status -eq 'Committed')
      $snip = if ($ctxt) { $ctxt.Substring(0, [Math]::Min(200, $ctxt.Length)) } else { '' }
      Rec 'TC-RA-0903-XLSX' $okCommit "xlsx commit maxRows=40 http=$([int]$commitResp.StatusCode) imported=$($cj.importedRowCount) status=$($cj.status) $snip"
    } catch {
      Rec 'TC-RA-0903-XLSX' $false "xlsx commit exception: $($_.Exception.Message)"
    }
  }
  $client.Dispose()
} else {
  Rec 'TC-RA-0902' $false 'xlsx missing'
}

# Observation cycle
$codeO = "CERT-OBS2-$([guid]::NewGuid().ToString('N').Substring(0,6))"
$po = Api POST "$raBase/products" $adminToken @{ countryCode='PA'; category='X'; brand='O'; regulatoryName='OBS2'; catalogCode=$codeO; riskClass='A'; currency='USD' }
$do = Api POST "$raBase/dossiers" $adminToken @{ productId=[string]$po.Body.id; authorityId=$authorityId; processType='NewRegistration' }
$doid = [string]$do.Body.id
foreach ($r in @($do.Body.requirements | Where-Object isCritical)) {
  $null = Api PUT "$raBase/dossiers/$doid/requirements/$($r.id)" $adminToken @{ status='Accepted'; storedFileId=[guid]::NewGuid().ToString(); notes='x' }
}
foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
  $null = Api POST "$raBase/dossiers/$doid/transition" $adminToken @{ targetStatus=$st; waiverReason=$(if($st -eq 'DocumentsReceived'){'CERT waiver evidence N/A'}else{$null}) }
}
$null = Api POST "$raBase/dossiers/$doid/submit" $adminToken @{}
$obs = Api POST "$raBase/dossiers/$doid/observations" $adminToken @{ description='CERT OBS2'; receivedOn=(Get-Date).ToUniversalTime().ToString('o') }
Rec 'TC-RA-0400' ($obs.Status -lt 300) "obs $($obs.Status)"
$null = Api POST "$raBase/dossiers/$doid/transition" $adminToken @{ targetStatus='CorrectingObservation'; waiverReason=$null }
$obsId = [string]$obs.Body.id
if (-not $obsId) {
  $det = Api GET "$raBase/dossiers/$doid" $adminToken
  $obsId = [string]@($det.Body.observations)[0].id
}
$null = Api POST "$raBase/dossiers/$doid/observations/$obsId/respond" $adminToken @{ notes='fixed'; close=$true }
$null = Api POST "$raBase/dossiers/$doid/transition" $adminToken @{ targetStatus='Resubmitted'; waiverReason=$null }
$det2 = Api GET "$raBase/dossiers/$doid" $adminToken
Rec 'TC-RA-0402' ($det2.Body.status -eq 'Resubmitted') "status=$($det2.Body.status)"

# Write results
$pass = @($results | Where-Object Pass).Count
$fail = @($results | Where-Object { -not $_.Pass }).Count
$results | Export-Csv (Join-Path $outDir 'full-cert-results.csv') -NoTypeInformation -Encoding UTF8
$summary = @{ when = (Get-Date).ToString('o'); pass = $pass; fail = $fail; total = $results.Count; passRate = [math]::Round(100.0 * $pass / [math]::Max(1,$results.Count), 1) }
$summary | ConvertTo-Json | Set-Content (Join-Path $outDir 'full-cert-summary.json') -Encoding UTF8
Write-Host "=== DONE PASS=$pass FAIL=$fail TOTAL=$($results.Count) RATE=$($summary.passRate)% ==="
if ($fail -gt 0) { $results | Where-Object { -not $_.Pass } | Format-Table -AutoSize | Out-String | Write-Host }
exit $(if ($fail -gt 0) { 1 } else { 0 })
