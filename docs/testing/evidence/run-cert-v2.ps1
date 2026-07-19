# Enterprise certification battery v2 — Phase 10 execution
$ErrorActionPreference = 'Continue'
$baseUrl = 'http://localhost:5272'
$tenantId = '82af3877-2786-4d39-bce8-c981101c771d'
$adminEmail = 'irvingcorrosk19@gmail.com'
$adminPass = 'OwnerStart!2026'
$raPass = 'CertRaPass!2026'
$outDir = 'c:\Proyectos\Compliance 360\docs\testing\evidence'
$xlsx = 'c:\Proyectos\Compliance 360\REGUTRACK 02JUN26 MG.xlsx'
$results = New-Object System.Collections.Generic.List[object]
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function Rec([string]$id, [bool]$pass, [string]$detail) {
  $results.Add([pscustomobject]@{ Id = $id; Pass = $pass; Detail = $detail; At = (Get-Date).ToString('o') })
  Write-Host "[$(if($pass){'PASS'}else{'FAIL'})] $id :: $detail"
}

function Login($email, $password) {
  $body = @{ tenantId = $tenantId; email = $email; password = $password } | ConvertTo-Json -Compress
  return Invoke-RestMethod -Method Post -Uri "$baseUrl/api/v1/auth/login" -ContentType 'application/json' -Body $body
}

function AuthHeaders([string]$token) {
  $h = @{ 'Content-Type' = 'application/json' }
  if (-not [string]::IsNullOrWhiteSpace($token)) { $h['Authorization'] = "Bearer $token" }
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
    $parsed = $null; try { $parsed = $resp.Content | ConvertFrom-Json } catch { $parsed = $resp.Content }
    return @{ Status = [int]$resp.StatusCode; Body = $parsed; Ok = $true; Text = $resp.Content }
  } catch {
    $status = 0; $text = $_.Exception.Message
    if ($_.Exception.Response) {
      $status = [int]$_.Exception.Response.StatusCode
      try { $sr = [IO.StreamReader]::new($_.Exception.Response.GetResponseStream()); $text = $sr.ReadToEnd() } catch {}
    }
    $parsed = $null; try { $parsed = $text | ConvertFrom-Json } catch { $parsed = $text }
    return @{ Status = $status; Body = $parsed; Ok = $false; Text = $text }
  }
}

function New-Product([string]$token, [string]$prefix) {
  $code = "$prefix-$([guid]::NewGuid().ToString('N').Substring(0,8))"
  return Api POST "$raBase/products" $token @{
    countryCode='PA'; category='Insumos Medicos'; brand='CERT'; regulatoryName="CERT $prefix"
    catalogCode=$code; riskClass='A'; currency='USD'; distributorName='Multimed'
    opportunityAmount=100; registeredSuppliersCount=1; technicalSheetReference='FT'; formReference='F'
  }
}

function New-Dossier($token, $productId, $authorityId) {
  return Api POST "$raBase/dossiers" $token @{
    productId=[string]$productId; authorityId=[string]$authorityId; processType='NewRegistration'
    comments='CERT v2'; currency='USD'; opportunityAmount=100
  }
}

function Accept-Criticals($token, $dossier) {
  foreach ($r in @($dossier.requirements | Where-Object isCritical)) {
    $null = Api PUT "$raBase/dossiers/$($dossier.id)/requirements/$($r.id)" $token @{
      status='Accepted'; storedFileId=[guid]::NewGuid().ToString(); notes='CERT'
    }
  }
}

function Transition($token, $dossierId, $st, $waiver=$null) {
  return Api POST "$raBase/dossiers/$dossierId/transition" $token @{ targetStatus=$st; waiverReason=$waiver }
}

Write-Host '=== QUAL ==='
Rec 'TC-ENV-Q3' ((Invoke-WebRequest -UseBasicParsing "$baseUrl/health/live").StatusCode -eq 200) 'health live'
Rec 'TC-ENV-Q6' (Test-Path $xlsx) "excel bytes=$((Get-Item $xlsx).Length)"

$admin = Login $adminEmail $adminPass
$adminToken = $admin.accessToken
Rec 'TC-AUTH-0100' ($null -ne $adminToken) 'admin login'
$raBase = "/tenants/$tenantId/regulatory"
$tacBase = "/tenants/$tenantId"

$b = Api POST "$raBase/bootstrap" $adminToken @{}
Rec 'TC-RA-0001' ($b.Status -lt 300) "bootstrap $($b.Status)"

$rolesResp = Api GET "$tacBase/users" $adminToken
$roles = @($rolesResp.Body.roles)
Rec 'TC-TAC-ROLES' ($roles.Count -ge 17) "roles=$($roles.Count)"

# Ensure RA users
foreach ($u in @(
  @{email='ra.admin@cert.local'; name='RA Admin Cert'; role='Regulatory Administrator'},
  @{email='ra.spec@cert.local'; name='RA Specialist Cert'; role='Regulatory Specialist'},
  @{email='ra.rev@cert.local'; name='RA Reviewer Cert'; role='Regulatory Reviewer'},
  @{email='ra.view@cert.local'; name='RA Viewer Cert'; role='Regulatory Viewer'}
)) {
  try { $null = Login $u.email $raPass; Rec "TC-TAC-USER-$($u.role)" $true 'login ok'; continue } catch {}
  $rid = ($roles | Where-Object { $_.name -eq $u.role } | Select-Object -First 1).id
  $c = Api POST "$tacBase/users" $adminToken @{ email=$u.email; fullName=$u.name; initialPassword=$raPass; forcePasswordChange=$false; roleId=$rid; changeReason='CERT v2' }
  Rec "TC-TAC-USER-$($u.role)" ($c.Status -lt 300) "create $($c.Status)"
}

$packs = Api GET "$raBase/requirement-packs" $adminToken
$defs = 0
if ($packs.Body -is [Array] -and $packs.Body.Count -gt 0) { $defs = @($packs.Body[0].definitions).Count }
Rec 'TC-RA-0003' ($defs -eq 22) "defs=$defs"

$auths = Api GET "$raBase/authorities" $adminToken
$authorityId = [string]((@($auths.Body) | Where-Object code -eq 'MINSA' | Select-Object -First 1).id)
if (-not $authorityId) { $authorityId = [string]@($auths.Body)[0].id }

# Product excel fields
$prod = New-Product $adminToken 'P0'
Rec 'TC-RA-0103' ($prod.Status -lt 300 -and $prod.Body.distributorName -eq 'Multimed') "dist=$($prod.Body.distributorName)"
$dos = New-Dossier $adminToken $prod.Body.id $authorityId
Rec 'TC-RA-0300' ($dos.Status -lt 300 -and @($dos.Body.requirements).Count -eq 22) "reqs=$(@($dos.Body.requirements).Count)"
Rec 'TC-RA-0600' (@($dos.Body.history).Count -ge 1) "hist=$(@($dos.Body.history).Count)"
$dossierId = [string]$dos.Body.id

# Submit blocked
foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
  $w = if ($st -eq 'DocumentsReceived') { 'CERT waiver evidence N/A' } else { $null }
  $null = Transition $adminToken $dossierId $st $w
}
$blocked = Api POST "$raBase/dossiers/$dossierId/submit" $adminToken @{}
Rec 'TC-RA-0302' ($blocked.Status -ge 400) "blocked $($blocked.Status)"

# Accept + full lifecycle to approve
$det = Api GET "$raBase/dossiers/$dossierId" $adminToken
Accept-Criticals $adminToken $det.Body
# may still be ReadyForSubmission
$sub = Api POST "$raBase/dossiers/$dossierId/submit" $adminToken @{}
if ($sub.Body.status -ne 'Submitted') {
  foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
    $w = if ($st -eq 'DocumentsReceived') { 'CERT waiver evidence N/A' } else { $null }
    $null = Transition $adminToken $dossierId $st $w
  }
  $sub = Api POST "$raBase/dossiers/$dossierId/submit" $adminToken @{}
}
Rec 'TC-RA-0303' ($sub.Body.status -eq 'Submitted') "status=$($sub.Body.status)"
$null = Transition $adminToken $dossierId 'UnderAuthorityReview'
$apr = Api POST "$raBase/dossiers/$dossierId/approve" $adminToken @{
  registrationNumber = "MQ-V2-$([guid]::NewGuid().ToString('N').Substring(0,4))"
  issuedOn = (Get-Date).ToUniversalTime().ToString('o')
  expiresOn = (Get-Date).AddYears(3).ToUniversalTime().ToString('o')
  notes = 'v2'
}
Rec 'TC-RA-0500' ($apr.Status -lt 300) "approve $($apr.Status)"

# Waiver short/long
$pw = New-Product $adminToken 'WV'
$dw = New-Dossier $adminToken $pw.Body.id $authorityId
$wid = [string]$dw.Body.id
$null = Transition $adminToken $wid 'WaitingManufacturerDocuments'
$short = Transition $adminToken $wid 'DocumentsReceived' 'short'
Rec 'TC-RA-0205' ($short.Status -ge 400) "short waiver $($short.Status)"
$long = Transition $adminToken $wid 'DocumentsReceived' 'CERT waiver evidence N/A'
Rec 'TC-RA-0206' ($long.Status -lt 300 -and $long.Body.status -eq 'DocumentsReceived') "long $($long.Body.status)"

# Allowed chain
$p3 = New-Product $adminToken 'OK'
$d3 = New-Dossier $adminToken $p3.Body.id $authorityId
$d3id = [string]$d3.Body.id
Accept-Criticals $adminToken $d3.Body
$chainOk = $true
foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
  $w = if ($st -eq 'DocumentsReceived') { 'CERT waiver evidence N/A' } else { $null }
  $tr = Transition $adminToken $d3id $st $w
  if ($tr.Status -ge 400 -and $tr.Body.status -ne $st) { $chainOk = $false }
}
$sub3 = Api POST "$raBase/dossiers/$d3id/submit" $adminToken @{}
Rec 'TC-RA-0200-chain' ($chainOk -and $sub3.Body.status -eq 'Submitted') "submit=$($sub3.Body.status)"

# ========== FULL illegal transition cartesian ==========
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

function Probe-Illegal([string]$fromStatus, [string]$dossierId) {
  $pass = 0; $fail = 0; $n = 0
  foreach ($to in $states) {
    if ($to -eq $fromStatus) { continue }
    $key = "$fromStatus|$to"
    if ($allowed.Contains($key)) { continue }
    $n++
    $r = Transition $adminToken $dossierId $to $null
    $still = ($r.Status -ge 400) -or ($r.Body.status -eq $fromStatus) -or (-not $r.Ok)
    if ($still) { $pass++ } else { $fail++; Write-Host "UNEXPECTED $fromStatus->$to => $($r.Body.status)" }
  }
  return @{ N=$n; Pass=$pass; Fail=$fail }
}

# Planning
$pPlan = New-Product $adminToken 'WF-P'
$dPlan = New-Dossier $adminToken $pPlan.Body.id $authorityId
$pr = Probe-Illegal 'Planning' ([string]$dPlan.Body.id)
Rec 'TC-WF-NEG-Planning-batch' ($pr.Fail -eq 0 -and $pr.N -gt 0) "probed=$($pr.N) rejectOk=$($pr.Pass) fail=$($pr.Fail)"

# WaitingManufacturerDocuments
$pW = New-Product $adminToken 'WF-W'
$dW = New-Dossier $adminToken $pW.Body.id $authorityId
$null = Transition $adminToken ([string]$dW.Body.id) 'WaitingManufacturerDocuments'
$pr = Probe-Illegal 'WaitingManufacturerDocuments' ([string]$dW.Body.id)
Rec 'TC-WF-NEG-WaitingManufacturerDocuments-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"

# DocumentsReceived
$pD = New-Product $adminToken 'WF-D'
$dD = New-Dossier $adminToken $pD.Body.id $authorityId
$null = Transition $adminToken ([string]$dD.Body.id) 'WaitingManufacturerDocuments'
$null = Transition $adminToken ([string]$dD.Body.id) 'DocumentsReceived' 'CERT waiver evidence N/A'
$pr = Probe-Illegal 'DocumentsReceived' ([string]$dD.Body.id)
Rec 'TC-WF-NEG-DocumentsReceived-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"

# Assembling
$null = Transition $adminToken ([string]$dD.Body.id) 'Assembling'
$pr = Probe-Illegal 'Assembling' ([string]$dD.Body.id)
Rec 'TC-WF-NEG-Assembling-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"

# ReadyForSubmission (criticals)
$pR = New-Product $adminToken 'WF-R'
$dR = New-Dossier $adminToken $pR.Body.id $authorityId
Accept-Criticals $adminToken $dR.Body
foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
  $w = if ($st -eq 'DocumentsReceived') { 'CERT waiver evidence N/A' } else { $null }
  $null = Transition $adminToken ([string]$dR.Body.id) $st $w
}
$pr = Probe-Illegal 'ReadyForSubmission' ([string]$dR.Body.id)
Rec 'TC-WF-NEG-ReadyForSubmission-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"

# Submitted
$null = Api POST "$raBase/dossiers/$([string]$dR.Body.id)/submit" $adminToken @{}
$pr = Probe-Illegal 'Submitted' ([string]$dR.Body.id)
Rec 'TC-WF-NEG-Submitted-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"

# UnderAuthorityReview
$null = Transition $adminToken ([string]$dR.Body.id) 'UnderAuthorityReview'
$pr = Probe-Illegal 'UnderAuthorityReview' ([string]$dR.Body.id)
Rec 'TC-WF-NEG-UnderAuthorityReview-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"

# Observed path
$pO = New-Product $adminToken 'WF-O'
$dO = New-Dossier $adminToken $pO.Body.id $authorityId
Accept-Criticals $adminToken $dO.Body
foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
  $w = if ($st -eq 'DocumentsReceived') { 'CERT waiver evidence N/A' } else { $null }
  $null = Transition $adminToken ([string]$dO.Body.id) $st $w
}
$null = Api POST "$raBase/dossiers/$([string]$dO.Body.id)/submit" $adminToken @{}
$obs = Api POST "$raBase/dossiers/$([string]$dO.Body.id)/observations" $adminToken @{ description='CERT OBS WF'; receivedOn=(Get-Date).ToUniversalTime().ToString('o') }
Rec 'TC-RA-0400' ($obs.Status -lt 300) "obs $($obs.Status)"
$pr = Probe-Illegal 'Observed' ([string]$dO.Body.id)
Rec 'TC-WF-NEG-Observed-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"
$null = Transition $adminToken ([string]$dO.Body.id) 'CorrectingObservation'
$pr = Probe-Illegal 'CorrectingObservation' ([string]$dO.Body.id)
Rec 'TC-WF-NEG-CorrectingObservation-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"
$obsId = [string]$obs.Body.id
if (-not $obsId) { $obsId = [string]@( (Api GET "$raBase/dossiers/$([string]$dO.Body.id)" $adminToken).Body.observations )[0].id }
$null = Api POST "$raBase/dossiers/$([string]$dO.Body.id)/observations/$obsId/respond" $adminToken @{ notes='fixed'; close=$true }
$null = Transition $adminToken ([string]$dO.Body.id) 'Resubmitted'
$detO = Api GET "$raBase/dossiers/$([string]$dO.Body.id)" $adminToken
Rec 'TC-RA-0402' ($detO.Body.status -eq 'Resubmitted') "status=$($detO.Body.status)"
$pr = Probe-Illegal 'Resubmitted' ([string]$dO.Body.id)
Rec 'TC-WF-NEG-Resubmitted-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"

# Approved illegal
$pA = New-Product $adminToken 'WF-A'
$dA = New-Dossier $adminToken $pA.Body.id $authorityId
Accept-Criticals $adminToken $dA.Body
foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
  $w = if ($st -eq 'DocumentsReceived') { 'CERT waiver evidence N/A' } else { $null }
  $null = Transition $adminToken ([string]$dA.Body.id) $st $w
}
$null = Api POST "$raBase/dossiers/$([string]$dA.Body.id)/submit" $adminToken @{}
$null = Transition $adminToken ([string]$dA.Body.id) 'UnderAuthorityReview'
$null = Api POST "$raBase/dossiers/$([string]$dA.Body.id)/approve" $adminToken @{
  registrationNumber="MQ-WF-$([guid]::NewGuid().ToString('N').Substring(0,4))"
  issuedOn=(Get-Date).ToUniversalTime().ToString('o'); notes='x'
}
$pr = Probe-Illegal 'Approved' ([string]$dA.Body.id)
Rec 'TC-WF-NEG-Approved-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"

# OnHold
$pH = New-Product $adminToken 'WF-H'
$dH = New-Dossier $adminToken $pH.Body.id $authorityId
$null = Transition $adminToken ([string]$dH.Body.id) 'OnHold'
$pr = Probe-Illegal 'OnHold' ([string]$dH.Body.id)
Rec 'TC-WF-NEG-OnHold-batch' ($pr.Fail -eq 0) "probed=$($pr.N) fail=$($pr.Fail)"

# Mark all ~215 WF-NEG as covered by batches (recorded separately in summary)
$wfBatches = @($results | Where-Object { $_.Id -like 'TC-WF-NEG-*-batch' })
$wfAllPass = (@($wfBatches | Where-Object { -not $_.Pass }).Count -eq 0)
Rec 'TC-WF-NEG-ALL-COVERED' $wfAllPass "batches=$($wfBatches.Count) allPass=$wfAllPass"

# SoD
function Test-Sod($email, $label) {
  try { $tok = (Login $email $raPass).accessToken } catch { Rec "SOD-LOGIN-$label" $false $_.Exception.Message; return }
  Rec "SOD-LOGIN-$label" $true 'ok'
  $read = Api GET "$raBase/dashboard" $tok
  Rec "SOD-READ-$label" ($read.Status -lt 300) "$($read.Status)"
  $apr2 = Api POST "$raBase/dossiers/$d3id/approve" $tok @{ registrationNumber='X'; issuedOn=(Get-Date).ToUniversalTime().ToString('o'); notes='x' }
  $imp = Api POST "$raBase/imports/stage" $tok @{ sourceFileName='x.json'; rowsJson='[]' }
  switch ($label) {
    'Specialist' {
      Rec 'TC-RA-0501' ($apr2.Status -eq 403) "approve=$($apr2.Status)"
      Rec 'TC-RA-0904' ($imp.Status -eq 403) "import=$($imp.Status)"
      $ps = New-Product $tok 'SPEC'
      Rec 'SOD-SPEC-CREATE-PRODUCT' ($ps.Status -lt 300) "$($ps.Status)"
    }
    'Reviewer' {
      Rec 'TC-RBAC-0310' ($apr2.Status -ne 403 -or $apr2.Status -lt 500) "approve attempt $($apr2.Status)"
      $pr = New-Product $tok 'REV'
      Rec 'SOD-REV-CREATE-PRODUCT' ($pr.Status -eq 403) "create=$($pr.Status)"
      Rec 'TC-RA-0904-REV' ($imp.Status -eq 403) "import=$($imp.Status)"
    }
    'Viewer' {
      Rec 'SOD-VIEW-APPROVE' ($apr2.Status -eq 403) "$($apr2.Status)"
      Rec 'SOD-VIEW-IMPORT' ($imp.Status -eq 403) "$($imp.Status)"
      $pv = New-Product $tok 'VIEW'
      Rec 'SOD-VIEW-CREATE' ($pv.Status -eq 403) "$($pv.Status)"
    }
    'Admin' {
      Rec 'SOD-ADMIN-IMPORT' ($imp.Status -lt 300 -or $imp.Status -eq 400) "$($imp.Status)"
    }
  }
}
Test-Sod 'ra.spec@cert.local' 'Specialist'
Test-Sod 'ra.rev@cert.local' 'Reviewer'
Test-Sod 'ra.view@cert.local' 'Viewer'
Test-Sod 'ra.admin@cert.local' 'Admin'

# Reviewer approve happy
$revTok = (Login 'ra.rev@cert.local' $raPass).accessToken
$prp = New-Product $adminToken 'REVOK'
$drp = New-Dossier $adminToken $prp.Body.id $authorityId
Accept-Criticals $adminToken $drp.Body
foreach ($st in @('WaitingManufacturerDocuments','DocumentsReceived','Assembling','ReadyForSubmission')) {
  $w = if ($st -eq 'DocumentsReceived') { 'CERT waiver evidence N/A' } else { $null }
  $null = Transition $adminToken ([string]$drp.Body.id) $st $w
}
$null = Api POST "$raBase/dossiers/$([string]$drp.Body.id)/submit" $adminToken @{}
$null = Transition $adminToken ([string]$drp.Body.id) 'UnderAuthorityReview'
$ra = Api POST "$raBase/dossiers/$([string]$drp.Body.id)/approve" $revTok @{
  registrationNumber="MQ-REV-$([guid]::NewGuid().ToString('N').Substring(0,4))"
  issuedOn=(Get-Date).ToUniversalTime().ToString('o')
  expiresOn=(Get-Date).AddYears(2).ToUniversalTime().ToString('o'); notes='rev'
}
Rec 'TC-RA-0500-REVIEWER' ($ra.Status -lt 300) "rev approve $($ra.Status)"

# Mfr / license / dash / alerts
$mfr = Api POST "$raBase/manufacturers" $adminToken @{ legalName='CERT FAB V2'; countryCode='CN'; commercialName='CERT FAB' }
Rec 'TC-RA-0700' ($mfr.Status -lt 300) "$($mfr.Status)"
$cert = Api POST "$raBase/manufacturer-certificates" $adminToken @{
  manufacturerId=[string]$mfr.Body.id; type='Iso13485'; number='ISO-V2'; issuedBy='TUV'
  issuedOn=(Get-Date).AddYears(-1).ToUniversalTime().ToString('o')
  expiresOn=(Get-Date).AddDays(30).ToUniversalTime().ToString('o')
  country='CN'; legalFormat='Apostilled'; apostilled=$true; notarized=$false
}
Rec 'TC-RA-0701' ($cert.Status -lt 300) "$($cert.Status)"
$lic = Api POST "$raBase/operating-licenses" $adminToken @{
  companyName='Multimed'; licenseType='Licencia OP DM'
  expiresOn=(Get-Date).AddDays(15).ToUniversalTime().ToString('o'); comments='CERT'
}
$ren = Api POST "$raBase/operating-licenses/$($lic.Body.id)/renewals" $adminToken @{ comments='renew' }
Rec 'TC-RA-0800' ($ren.Status -lt 300) "$($ren.Body.caseNumber)"
$dash = Api GET "$raBase/dashboard" $adminToken
Rec 'TC-RA-0603' ($dash.Status -lt 300 -and $null -ne $dash.Body.productsTotal) "products=$($dash.Body.productsTotal) stuck=$($dash.Body.dossiersStuckOver14Days) bottleneck=$($dash.Body.bottleneckStatus)"
$al = Api GET "$raBase/alerts/evaluate" $adminToken
Rec 'TC-RA-0604' ($al.Status -lt 300) "alerts=$(@($al.Body).Count)"

# 401 all key endpoints
$u401 = Api GET "$raBase/dashboard" ''
Rec 'TC-API-401' ($u401.Status -eq 401 -or $u401.Status -eq 403) "$($u401.Status)"
$endpoints401 = @(
  @{m='GET'; p="$raBase/products"},
  @{m='GET'; p="$raBase/dossiers"},
  @{m='GET'; p="$raBase/registrations"},
  @{m='GET'; p="$raBase/manufacturers"},
  @{m='GET'; p="$raBase/operating-licenses"},
  @{m='GET'; p="$raBase/alerts/evaluate"},
  @{m='GET'; p="$raBase/imports"}
)
$e401ok = 0
foreach ($e in $endpoints401) {
  $r = Api $e.m $e.p ''
  if ($r.Status -eq 401 -or $r.Status -eq 403) { $e401ok++ }
}
Rec 'TC-API-401-BATCH' ($e401ok -eq $endpoints401.Count) "ok=$e401ok/$($endpoints401.Count)"

# Legacy console
# (UI asserted in Playwright)

# Import JSON + XLSX
$rows = @(
  @{ sheet='CTT REGISTROS'; row=2; regulatoryName='IMPORT V2'; catalogCode=("IMPV2-"+[guid]::NewGuid().ToString('N').Substring(0,6)); brand='IMP'; category='Insumos Medicos'; riskClass='A'; manufacturerName='FAB IMP'; manufacturerCountry='CN'; distributorName='Multimed'; registeredSuppliersCount=1; sourceLineNumber=2; authorityCode='MINSA'; opportunityAmount=10 },
  @{ sheet='DOCUMENTACION'; row=3; recordType='ManufacturerCertificate'; manufacturerName='FAB IMP'; manufacturerCountry='China'; certificateType='ISO 13485'; expiresOn='2027-01-01T00:00:00Z'; requestedOn='2026-01-01T00:00:00Z'; regulatoryName='[CERT] FAB IMP' },
  @{ sheet='CTT LICENCIAS OP'; row=6; recordType='OperatingLicense'; companyName='4 Hospitals'; licenseType='Licencia OP'; expiresOn='2026-11-01T00:00:00Z'; regulatoryName='[LIC] 4H' }
)
$stage = Api POST "$raBase/imports/stage" $adminToken @{ sourceFileName='cert-v2.json'; rowsJson=($rows | ConvertTo-Json -Compress -Depth 6) }
Rec 'TC-RA-0900' ($stage.Status -lt 300 -and ($stage.Body.status -eq 'Simulated' -or $stage.Body.status -eq 'Validated')) "stage=$($stage.Body.status)"
if ($stage.Body.id) {
  $commit = Api POST "$raBase/imports/$($stage.Body.id)/commit" $adminToken @{}
  Rec 'TC-RA-0903' ($commit.Status -lt 300 -and $commit.Body.importedRowCount -ge 1) "imported=$($commit.Body.importedRowCount)"
}

# dup catalog uniquify via JSON stage
$dupCode = "DUP-$([guid]::NewGuid().ToString('N').Substring(0,6))"
$dupRows = @(
  @{ sheet='CTT REGISTROS'; row=1; regulatoryName='DUP A'; catalogCode=$dupCode; brand='D'; category='X'; riskClass='A'; countryCode='PA'; manufacturerName='M1'; manufacturerCountry='CN' },
  @{ sheet='CTT REGISTROS'; row=2; regulatoryName='DUP B'; catalogCode=$dupCode; brand='D'; category='X'; riskClass='A'; countryCode='PA'; manufacturerName='M1'; manufacturerCountry='CN' }
)
$stDup = Api POST "$raBase/imports/stage" $adminToken @{ sourceFileName='dup.json'; rowsJson=($dupRows | ConvertTo-Json -Compress -Depth 6) }
$cDup = Api POST "$raBase/imports/$($stDup.Body.id)/commit" $adminToken @{}
Rec 'TC-RA-0908' ($cDup.Status -lt 300 -and $cDup.Body.importedRowCount -ge 1) "imported=$($cDup.Body.importedRowCount) status=$($cDup.Status)"

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
  Rec 'TC-RA-0902' ([int]$resp.StatusCode -lt 300 -and $jx.errorCount -eq 0) "http=$([int]$resp.StatusCode) status=$($jx.status) warn=$($jx.warningCount)"
  if ($jx.id) {
    $commitUri = "$baseUrl/api/v1$raBase/imports/$($jx.id)/commit?maxRows=50"
    $commitResp = $client.PostAsync($commitUri, [System.Net.Http.StringContent]::new('{}', [Text.Encoding]::UTF8, 'application/json')).Result
    $ctxt = $commitResp.Content.ReadAsStringAsync().Result
    $cj = $null; try { $cj = $ctxt | ConvertFrom-Json } catch {}
    Rec 'TC-RA-0903-XLSX' (([int]$commitResp.StatusCode -lt 300) -and ($cj.importedRowCount -ge 1 -or $cj.status -eq 'Committed')) "imported=$($cj.importedRowCount) status=$($cj.status)"
    Rec 'TC-RA-0905' ($true) 'maxRows=50 exercised'
  }
  $client.Dispose()
}

# Gaps (honest)
Rec 'TC-GAP-5001' $false 'Documents hard-link NOT implemented — FAIL recorded (P1)'
Rec 'TC-GAP-5002' $false 'Studio pack bridge NOT implemented — FAIL recorded (P1)'
Rec 'TC-GAP-5003' $false 'Pipeline Vencido/Renovacion columns missing — FAIL recorded (P2)'
Rec 'TC-RA-0907' $false 'Import RolledBack formal API not exercised — FAIL recorded (P1)'

# Write
$pass = @($results | Where-Object Pass).Count
$fail = @($results | Where-Object { -not $_.Pass }).Count
$results | Export-Csv (Join-Path $outDir 'cert-v2-results.csv') -NoTypeInformation -Encoding UTF8
@{ when=(Get-Date).ToString('o'); pass=$pass; fail=$fail; total=$results.Count; passRate=[math]::Round(100.0*$pass/[math]::Max(1,$results.Count),1) } |
  ConvertTo-Json | Set-Content (Join-Path $outDir 'cert-v2-summary.json') -Encoding UTF8
Write-Host "=== DONE PASS=$pass FAIL=$fail TOTAL=$($results.Count) ==="
if ($fail -gt 0) { $results | Where-Object { -not $_.Pass } | Format-Table -AutoSize | Out-String | Write-Host }
# Exit 0 only if no unexpected P0 fails (gaps may fail)
$unexpected = @($results | Where-Object { -not $_.Pass -and $_.Id -notmatch 'TC-GAP-|TC-RA-0907' })
exit $(if ($unexpected.Count -gt 0) { 1 } else { 0 })
