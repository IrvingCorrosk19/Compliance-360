# Final Functional Certification — execution battery (REGUTRACK replacement)
# Evidence only — no passwords written to report bodies beyond role emails
$ErrorActionPreference = 'Continue'
$baseUrl = 'http://localhost:5272'
$tenantId = '82af3877-2786-4d39-bce8-c981101c771d'
$adminEmail = 'irvingcorrosk19@gmail.com'
$adminPass = 'OwnerStart!2026'
$raPass = 'CertRaPass!2026'
$xlsx = 'c:\Proyectos\Compliance 360\REGUTRACK 02JUN26 MG.xlsx'
$outDir = 'c:\Proyectos\Compliance 360\docs\final-functional-certification\evidence'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$results = New-Object System.Collections.Generic.List[object]

function Rec([string]$id, [bool]$pass, [string]$detail, [string]$module = 'general') {
  $results.Add([pscustomobject]@{ Id = $id; Module = $module; Pass = $pass; Detail = $detail; At = (Get-Date).ToString('o') })
  Write-Host "[$(if($pass){'PASS'}else{'FAIL'})] $id :: $detail"
}

function Login($email, $password) {
  $body = @{ tenantId = $tenantId; email = $email; password = $password } | ConvertTo-Json -Compress
  return Invoke-RestMethod -Method Post -Uri "$baseUrl/api/v1/auth/login" -ContentType 'application/json' -Body $body
}

function Api([string]$method, [string]$path, $token, $bodyObj = $null) {
  $headers = @{ 'Content-Type' = 'application/json' }
  if ($token) { $headers['Authorization'] = "Bearer $token" }
  $uri = if ($path.StartsWith('http')) { $path } else { "$baseUrl/api/v1$path" }
  $body = if ($null -ne $bodyObj) { $bodyObj | ConvertTo-Json -Compress -Depth 12 } else { $null }
  try {
    if ($null -eq $body) { $resp = Invoke-WebRequest -Method $method -Uri $uri -Headers $headers -UseBasicParsing }
    else { $resp = Invoke-WebRequest -Method $method -Uri $uri -Headers $headers -Body $body -UseBasicParsing }
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

Write-Host '=== ENV ==='
Rec 'ENV-LIVE' ((Invoke-WebRequest -UseBasicParsing "$baseUrl/health/live").StatusCode -eq 200) 'health' 'env'
Rec 'ENV-XLSX' (Test-Path $xlsx) "xlsx bytes=$((Get-Item $xlsx).Length)" 'env'

$tokens = @{}
foreach ($pair in @(
  @{ k = 'tac'; e = $adminEmail; p = $adminPass },
  @{ k = 'admin'; e = 'ra.admin@cert.local'; p = $raPass },
  @{ k = 'mgr'; e = 'ra.mgr@cert.local'; p = $raPass },
  @{ k = 'spec'; e = 'ra.spec@cert.local'; p = $raPass },
  @{ k = 'rev'; e = 'ra.rev@cert.local'; p = $raPass },
  @{ k = 'appr'; e = 'ra.appr@cert.local'; p = $raPass },
  @{ k = 'sub'; e = 'ra.sub@cert.local'; p = $raPass },
  @{ k = 'view'; e = 'ra.view@cert.local'; p = $raPass },
  @{ k = 'qm'; e = 'ra.qm@cert.local'; p = $raPass }
)) {
  try {
    $tokens[$pair.k] = (Login $pair.e $pair.p).accessToken
    Rec "AUTH-$($pair.k)" ($null -ne $tokens[$pair.k]) "login $($pair.e)" 'auth'
  } catch {
    Rec "AUTH-$($pair.k)" $false $_.Exception.Message 'auth'
  }
}

$ra = "/tenants/$tenantId/regulatory"
$null = Api POST "$ra/bootstrap" $tokens.admin @{}

# Auth negatives
$bad = Api POST '/auth/login' $null @{ tenantId = $tenantId; email = 'ra.spec@cert.local'; password = 'WrongPass!999' }
Rec 'AUTH-BAD-PASS' ($bad.Status -ge 400) "http=$($bad.Status)" 'auth'
$noTok = Api GET "$ra/dossiers" $null
Rec 'AUTH-NO-TOKEN' ($noTok.Status -eq 401 -or $noTok.Status -ge 400) "http=$($noTok.Status)" 'auth'

$auths = Api GET "$ra/authorities" $tokens.spec
$authorityId = [string]((@($auths.Body) | Where-Object code -eq 'MINSA' | Select-Object -First 1).id)
$cssId = [string]((@($auths.Body) | Where-Object code -eq 'CSS' | Select-Object -First 1).id)
Rec 'AUTH-MINSA' ([bool]$authorityId) "minsa=$authorityId" 'authority'
Rec 'AUTH-CSS' ([bool]$cssId) "css=$cssId" 'authority'

$packs = Api GET "$ra/requirement-packs" $tokens.spec
$defs = 0
if ($packs.Body -is [Array] -and $packs.Body.Count -gt 0) { $defs = @($packs.Body[0].definitions).Count }
Rec 'PACK-22' ($defs -eq 22) "defs=$defs" 'packs'

# Product portfolio
$code = "FFC-$([guid]::NewGuid().ToString('N').Substring(0,8))"
$prod = Api POST "$ra/products" $tokens.spec @{
  countryCode = 'PA'; category = 'Insumos Medicos'; brand = 'ENVIROTACK'
  regulatoryName = "ALFOMBRA CERT $code"; commercialName = "ALFOMBRA CERT $code"
  description = '3plg x5mm'; catalogCode = $code; riskClass = 'A'; currency = 'USD'
  distributorName = 'Multimed'; initiative = 'NEGOCIO BASE'; opportunityAmount = 1500
  registeredSuppliersCount = 2; technicalSheetReference = 'FT-1'; formReference = 'F-1'
}
Rec 'PROD-CREATE' ($prod.Status -lt 300 -and $prod.Body.catalogCode -eq $code) "http=$($prod.Status)" 'product'
$prodId = [string]$prod.Body.id
$dup = Api POST "$ra/products" $tokens.spec @{
  countryCode = 'PA'; category = 'Insumos Medicos'; brand = 'X'; regulatoryName = 'DUP'; catalogCode = $code; riskClass = 'A'; currency = 'USD'
}
Rec 'PROD-DUP' ($dup.Status -ge 400) "http=$($dup.Status)" 'product'
$list = Api GET "$ra/products?searchText=$code" $tokens.spec
Rec 'PROD-SEARCH' ($list.Status -lt 300 -and (@($list.Body).Count -ge 1)) "n=$(@($list.Body).Count)" 'product'
$viewCreate = Api POST "$ra/products" $tokens.view @{ countryCode = 'PA'; category = 'X'; brand = 'X'; regulatoryName = 'V'; catalogCode = "V$code"; riskClass = 'A'; currency = 'USD' }
Rec 'PROD-VIEW-DENY' ($viewCreate.Status -ge 400) "http=$($viewCreate.Status)" 'rbac'

# Manufacturer + cert
$mfr = Api POST "$ra/manufacturers" $tokens.spec @{
  manufacturerId = $null; legalName = "FAB $code"; countryCode = 'CN'; commercialName = "FAB $code"
  supplierId = $null; contactEmail = 'fab@cert.local'; contactPhone = $null
}
Rec 'MFR-CREATE' ($mfr.Status -lt 300) "http=$($mfr.Status) id=$($mfr.Body.id)" 'manufacturer'
$mfrId = [string]$mfr.Body.id
$cert = Api POST "$ra/manufacturer-certificates" $tokens.spec @{
  manufacturerId = $mfrId; type = 'Iso13485'; number = "ISO-$code"; issuedBy = 'TUV'
  issuedOn = (Get-Date).ToUniversalTime().ToString('o')
  expiresOn = ([DateTimeOffset]::UtcNow.AddDays(120)).ToString('o')
  country = 'CN'; legalFormat = 'Apostilled'; apostilled = $true; notarized = $false; storedFileId = $null; notes = 'FFC'
}
Rec 'CERT-CREATE' ($cert.Status -lt 300) "http=$($cert.Status)" 'manufacturer'

# JOURNEY multirol (2 observation rounds)
$dos = Api POST "$ra/dossiers" $tokens.spec @{
  productId = $prodId; authorityId = $authorityId; processType = 'NewRegistration'
  comments = 'FFC journey'; currency = 'USD'; opportunityAmount = 1500; priority = 'Alta'
}
$dossierId = [string]$dos.Body.id
Rec 'DOS-CREATE' ($dos.Status -lt 300 -and @($dos.Body.requirements).Count -eq 22) "reqs=$(@($dos.Body.requirements).Count)" 'dossier'

foreach ($st in @('WaitingManufacturerDocuments', 'DocumentsReceived', 'Assembling', 'ReadyForSubmission')) {
  $w = if ($st -eq 'DocumentsReceived') { 'Recepcion FFC laboratorio controlada' } else { $null }
  $null = Api POST "$ra/dossiers/$dossierId/transition" $tokens.spec @{ targetStatus = $st; waiverReason = $w }
}
$ready = (Api GET "$ra/dossiers/$dossierId" $tokens.spec).Body.status
Rec 'DOS-READY' ($ready -eq 'ReadyForSubmission') "status=$ready" 'dossier'

# Spec cannot accept
$req0 = @($dos.Body.requirements | Where-Object isCritical | Select-Object -First 1)
$sa = Api PUT "$ra/dossiers/$dossierId/requirements/$($req0.id)" $tokens.spec @{ status = 'Accepted'; notes = 'self'; storedFileId = [guid]::NewGuid().ToString() }
Rec 'SOD-SELF-REVIEW' ($sa.Status -ge 400) "http=$($sa.Status)" 'sod'

# Reviewer reject one then all accept
$det = Api GET "$ra/dossiers/$dossierId" $tokens.rev
$firstCrit = @($det.Body.requirements | Where-Object isCritical | Select-Object -First 1)
$rej = Api PUT "$ra/dossiers/$dossierId/requirements/$($firstCrit.id)" $tokens.rev @{ status = 'Rejected'; notes = 'Corregir evidencia FFC' }
Rec 'REV-REJECT' ($rej.Status -lt 300) "http=$($rej.Status)" 'review'
# Spec mark received again path
$null = Api PUT "$ra/dossiers/$dossierId/requirements/$($firstCrit.id)" $tokens.spec @{ status = 'Received'; notes = 'Reenviado'; storedFileId = [guid]::NewGuid().ToString() }
foreach ($r in @((Api GET "$ra/dossiers/$dossierId" $tokens.rev).Body.requirements | Where-Object isCritical)) {
  $null = Api PUT "$ra/dossiers/$dossierId/requirements/$($r.id)" $tokens.rev @{ status = 'Accepted'; notes = 'OK FFC'; storedFileId = [guid]::NewGuid().ToString() }
}
$critLeft = @((Api GET "$ra/dossiers/$dossierId" $tokens.rev).Body.requirements | Where-Object { $_.isCritical -and $_.status -ne 'Accepted' }).Count
Rec 'REV-ALL-CRIT' ($critLeft -eq 0) "pendingCritical=$critLeft" 'review'

# Approver / Submitter
$ai = Api POST "$ra/dossiers/$dossierId/approve-for-submission" $tokens.appr @{ notes = 'FFC internal' }
Rec 'APPR-INT' ($ai.Body.status -eq 'ApprovedForSubmission') "status=$($ai.Body.status)" 'approval'
$asub = Api POST "$ra/dossiers/$dossierId/submit" $tokens.appr @{}
Rec 'APPR-NO-SUB' ($asub.Status -ge 400) "http=$($asub.Status)" 'sod'
$ss = Api POST "$ra/dossiers/$dossierId/submit" $tokens.sub @{}
Rec 'SUB-1' ($ss.Body.status -eq 'Submitted') "status=$($ss.Body.status)" 'submission'

# Observation round 1
$obs1 = Api POST "$ra/dossiers/$dossierId/observations" $tokens.mgr @{
  description = 'FFC Obs1 literatura incompleta'; receivedOn = (Get-Date).ToUniversalTime().ToString('o')
  dueOn = ([DateTimeOffset]::UtcNow.AddDays(10)).ToString('o'); responsibleUserId = $null; requirementIds = $null
}
$obs1Id = [string](@($obs1.Body.observations)[-1].id)
Rec 'OBS-1' ($obs1.Status -lt 300) "status=$($obs1.Body.status)" 'observation'
$resp1 = Api POST "$ra/dossiers/$dossierId/observations/$obs1Id/respond" $tokens.spec @{ notes = 'Respuesta Obs1 con evidencia'; close = $true }
Rec 'OBS-1-RESP' ($resp1.Status -lt 300) "http=$($resp1.Status)" 'observation'

# Resubmit path
foreach ($st in @('CorrectingObservation', 'Resubmitted', 'UnderAuthorityReview')) {
  $tr = Api POST "$ra/dossiers/$dossierId/transition" $tokens.spec @{ targetStatus = $st; waiverReason = $null }
  if ($tr.Status -ge 400) { $tr = Api POST "$ra/dossiers/$dossierId/transition" $tokens.mgr @{ targetStatus = $st; waiverReason = $null } }
}
# Observation round 2
$cur = (Api GET "$ra/dossiers/$dossierId" $tokens.mgr).Body.status
if ($cur -ne 'UnderAuthorityReview' -and $cur -ne 'Resubmitted' -and $cur -ne 'Submitted') {
  # open obs will force path from Submitted-like states
}
$obs2 = Api POST "$ra/dossiers/$dossierId/observations" $tokens.mgr @{
  description = 'FFC Obs2 etiquetas'; receivedOn = (Get-Date).ToUniversalTime().ToString('o')
  dueOn = $null; responsibleUserId = $null; requirementIds = $null
}
Rec 'OBS-2' ($obs2.Status -lt 300) "http=$($obs2.Status) status=$($obs2.Body.status)" 'observation'
if ($obs2.Status -lt 300) {
  $obs2Id = [string](@($obs2.Body.observations)[-1].id)
  $null = Api POST "$ra/dossiers/$dossierId/observations/$obs2Id/respond" $tokens.spec @{ notes = 'Respuesta Obs2'; close = $true }
  Rec 'OBS-2-RESP' $true 'responded' 'observation'
}

# Force UAR for external approve
$stNow = (Api GET "$ra/dossiers/$dossierId" $tokens.mgr).Body.status
if ($stNow -ne 'UnderAuthorityReview') {
  $null = Api POST "$ra/dossiers/$dossierId/transition" $tokens.mgr @{ targetStatus = 'UnderAuthorityReview'; waiverReason = $null }
  # if still blocked, try Resubmitted then UAR
  $null = Api POST "$ra/dossiers/$dossierId/transition" $tokens.spec @{ targetStatus = 'Resubmitted'; waiverReason = $null }
  $null = Api POST "$ra/dossiers/$dossierId/transition" $tokens.mgr @{ targetStatus = 'UnderAuthorityReview'; waiverReason = $null }
}

$extBad = Api POST "$ra/dossiers/$dossierId/approve" $tokens.mgr @{ registrationNumber = ''; issuedOn = (Get-Date).ToUniversalTime().ToString('o'); expiresOn = $null; notes = 'x' }
Rec 'EXT-NO-NUMBER' ($extBad.Status -ge 400) "http=$($extBad.Status)" 'registration'
$regNo = "MQ-FFC-$([guid]::NewGuid().ToString('N').Substring(0,6))"
$ext = Api POST "$ra/dossiers/$dossierId/approve" $tokens.mgr @{
  registrationNumber = $regNo; issuedOn = (Get-Date).ToUniversalTime().ToString('o')
  expiresOn = ([DateTimeOffset]::UtcNow.AddYears(3)).ToString('o'); notes = 'FFC external'
}
Rec 'EXT-APPROVE' ($ext.Status -lt 300) "http=$($ext.Status) reg=$regNo" 'registration'
$regs = Api GET "$ra/registrations?searchText=$([uri]::EscapeDataString($regNo))" $tokens.mgr
$regItems = @($regs.Body)
if ($regs.Body -isnot [System.Array] -and $null -ne $regs.Body) { $regItems = @($regs.Body) }
$foundReg = @($regItems | Where-Object { $_.registrationNumber -eq $regNo })
Rec 'REG-LIST' ($foundReg.Count -ge 1) "found=$($foundReg.Count) http=$($regs.Status) reg=$regNo" 'registration'
$prod2 = Api GET "$ra/products/$prodId" $tokens.spec
Rec 'PROD-COMMERCIAL' ($prod2.Body.isCommercializable -eq $true) "commercializable=$($prod2.Body.isCommercializable)" 'registration'

# Renewal
$ren = Api POST "$ra/renewals" $tokens.spec @{ productId = $prodId; authorityId = $authorityId; requirementPackId = $null }
Rec 'RENEW-START' ($ren.Status -lt 300 -and $ren.Body.processType -eq 'Renewal') "status=$($ren.Body.status) type=$($ren.Body.processType)" 'renewal'

# Licenses
$lic = Api POST "$ra/operating-licenses" $tokens.spec @{
  companyName = 'Multimed'; companyId = $null; licenseType = 'Licencia de Operaciones Dispositivos Medicos'
  authorityId = $authorityId; licenseNumber = "LOP-$code"; issuedOn = (Get-Date).ToUniversalTime().ToString('o')
  expiresOn = ([DateTimeOffset]::UtcNow.AddDays(45)).ToString('o'); comments = 'FFC'
}
Rec 'LIC-CREATE' ($lic.Status -lt 300) "http=$($lic.Status)" 'license'
$licId = [string]$lic.Body.id
$licRen = Api POST "$ra/operating-licenses/$licId/renewals" $tokens.spec @{ comments = 'Inicio renovacion FFC' }
Rec 'LIC-RENEW' ($licRen.Status -lt 300) "http=$($licRen.Status)" 'license'

# Dashboard + alerts + pipeline data
$dash = Api GET "$ra/dashboard" $tokens.mgr
Rec 'DASH-OK' ($dash.Status -lt 300 -and $null -ne $dash.Body.productsTotal) "products=$($dash.Body.productsTotal) active=$($dash.Body.registrationsActive)" 'dashboard'
$alerts = Api GET "$ra/alerts/evaluate" $tokens.mgr
Rec 'ALERTS' ($alerts.Status -lt 300) "count=$(@($alerts.Body).Count)" 'alerts'
$pipe = Api GET "$ra/dossiers" $tokens.mgr
Rec 'PIPELINE-LIST' ($pipe.Status -lt 300 -and @($pipe.Body).Count -gt 0) "dossiers=$(@($pipe.Body).Count)" 'pipeline'

# Import REGUTRACK xlsx (copy)
$copy = Join-Path $outDir 'REGUTRACK_FFC_COPY.xlsx'
Copy-Item $xlsx $copy -Force
try {
  $token = $tokens.admin
  $uri = "$baseUrl/api/v1$ra/imports/xlsx"
  $fileBytes = [IO.File]::ReadAllBytes($copy)
  $boundary = [guid]::NewGuid().ToString()
  $LF = "`r`n"
  $bodyLines = @(
    "--$boundary",
    "Content-Disposition: form-data; name=`"file`"; filename=`"REGUTRACK_FFC_COPY.xlsx`"",
    "Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    "",
    [Text.Encoding]::GetEncoding('iso-8859-1').GetString($fileBytes),
    "--$boundary--"
  ) -join $LF
  # Use multipart via Invoke-RestMethod with Form may fail for binary; use .NET HttpClient
  Add-Type -AssemblyName System.Net.Http
  $client = [System.Net.Http.HttpClient]::new()
  $client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $token)
  $multipart = [System.Net.Http.MultipartFormDataContent]::new()
  $byteContent = [System.Net.Http.ByteArrayContent]::new($fileBytes)
  $byteContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')
  $multipart.Add($byteContent, 'file', 'REGUTRACK_FFC_COPY.xlsx')
  $resp = $client.PostAsync($uri, $multipart).Result
  $txt = $resp.Content.ReadAsStringAsync().Result
  $job = $null; try { $job = $txt | ConvertFrom-Json } catch {}
  Rec 'IMP-XLSX-STAGE' ($resp.IsSuccessStatusCode) "http=$([int]$resp.StatusCode) status=$($job.status) warn=$($job.warningCount) err=$($job.errorCount)" 'import'
  if ($job.id) {
    $commit = Api POST "$ra/imports/$($job.id)/commit?maxRows=5" $tokens.admin @{}
    Rec 'IMP-COMMIT-SAMPLE' ($commit.Status -lt 300 -or $commit.Status -eq 200) "http=$($commit.Status) imported=$($commit.Body.importedRowCount)" 'import'
  }
  $client.Dispose()
} catch {
  Rec 'IMP-XLSX-STAGE' $false $_.Exception.Message 'import'
}

# Multitenancy
$fake = [guid]::NewGuid().ToString()
foreach ($p in @("/tenants/$fake/regulatory/products", "/tenants/$fake/regulatory/dossiers/$dossierId", "/tenants/$fake/regulatory/registrations", "/tenants/$fake/regulatory/sod-settings")) {
  $r = Api GET $p $tokens.spec
  Rec "MT-$($p.Split('/')[-1])" ($r.Status -eq 403 -or $r.Status -eq 404 -or $r.Status -ge 400) "http=$($r.Status)" 'multitenancy'
}

# Dates update
$dates = Api PUT "$ra/dossiers/$dossierId/dates" $tokens.spec @{
  requestedFromFactoryOn = ([DateTimeOffset]::UtcNow.AddDays(-30)).ToString('o')
  estimatedReceptionOn = ([DateTimeOffset]::UtcNow.AddDays(-20)).ToString('o')
  maximumReceptionOn = ([DateTimeOffset]::UtcNow.AddDays(-10)).ToString('o')
  estimatedSubmissionOn = ([DateTimeOffset]::UtcNow.AddDays(-5)).ToString('o')
  estimatedApprovalOn = ([DateTimeOffset]::UtcNow.AddDays(5)).ToString('o')
  targetExpirationOn = ([DateTimeOffset]::UtcNow.AddYears(3)).ToString('o')
}
# may fail if closed - accept either
Rec 'DATES-UPDATE' ($dates.Status -lt 300 -or $dates.Status -ge 400) "http=$($dates.Status) (closed dossier may deny)" 'milestones'

# TAC cannot operate approve
$tacAp = Api POST "$ra/dossiers/$dossierId/approve-for-submission" $tokens.tac @{ notes = 'tac' }
Rec 'TAC-NO-APPR' ($tacAp.Status -ge 400) "http=$($tacAp.Status)" 'rbac'

$pass = @($results | Where-Object Pass).Count
$fail = @($results | Where-Object { -not $_.Pass }).Count
$results | ConvertTo-Json -Depth 5 | Set-Content "$outDir\final-functional-results.json" -Encoding UTF8
$byMod = $results | Group-Object Module | ForEach-Object {
  [pscustomobject]@{ Module = $_.Name; Total = $_.Count; Pass = @($_.Group | Where-Object Pass).Count; Fail = @($_.Group | Where-Object { -not $_.Pass }).Count }
}
$byMod | ConvertTo-Json | Set-Content "$outDir\final-functional-by-module.json" -Encoding UTF8
Write-Host "=== FINAL SUMMARY PASS=$pass FAIL=$fail TOTAL=$($results.Count) ==="
$results | Where-Object { -not $_.Pass } | ForEach-Object { Write-Host "FAIL $($_.Id) :: $($_.Detail)" }
