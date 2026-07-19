$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5272'
$t = '82af3877-2786-4d39-bce8-c981101c771d'
function Login($e, $p) {
  $b = @{ tenantId = $t; email = $e; password = $p } | ConvertTo-Json -Compress
  return Invoke-RestMethod -Method Post -Uri "$base/api/v1/auth/login" -ContentType 'application/json' -Body $b
}
function AuthHeaders($tok) { @{ Authorization = "Bearer $tok"; 'Content-Type' = 'application/json' } }

$spec = (Login 'ra.spec@cert.local' 'CertRaPass!2026').accessToken
$rev = (Login 'ra.rev@cert.local' 'CertRaPass!2026').accessToken
$appr = (Login 'ra.appr@cert.local' 'CertRaPass!2026').accessToken
$hs = AuthHeaders $spec
$auths = Invoke-RestMethod -Headers $hs -Uri "$base/api/v1/tenants/$t/regulatory/authorities"
$aid = ($auths | Where-Object code -eq 'MINSA' | Select-Object -First 1).id
$code = "N" + [guid]::NewGuid().ToString('N').Substring(0, 8)
$prod = Invoke-RestMethod -Method Post -Headers $hs -Uri "$base/api/v1/tenants/$t/regulatory/products" -Body (@{
    countryCode = 'PA'; category = 'Insumos Medicos'; brand = 'N'; regulatoryName = "Notify $code"
    catalogCode = $code; riskClass = 'A'; currency = 'USD'
  } | ConvertTo-Json)
$dos = Invoke-RestMethod -Method Post -Headers $hs -Uri "$base/api/v1/tenants/$t/regulatory/dossiers" -Body (@{
    productId = $prod.id; authorityId = $aid; processType = 'NewRegistration'; comments = 'notify'; currency = 'USD'
  } | ConvertTo-Json)
$id = $dos.id
foreach ($st in @('WaitingManufacturerDocuments', 'DocumentsReceived', 'Assembling', 'ReadyForSubmission')) {
  $w = if ($st -eq 'DocumentsReceived') { 'Recepcion notify SoD' } else { $null }
  try {
    Invoke-RestMethod -Method Post -Headers $hs -Uri "$base/api/v1/tenants/$t/regulatory/dossiers/$id/transition" -Body (@{ targetStatus = $st; waiverReason = $w } | ConvertTo-Json) | Out-Null
  } catch {}
}
$hr = AuthHeaders $rev
$det = Invoke-RestMethod -Headers $hr -Uri "$base/api/v1/tenants/$t/regulatory/dossiers/$id"
foreach ($r in @($det.requirements | Where-Object isCritical)) {
  Invoke-RestMethod -Method Put -Headers $hr -Uri "$base/api/v1/tenants/$t/regulatory/dossiers/$id/requirements/$($r.id)" -Body (@{
      status = 'Accepted'; notes = 'ok'; storedFileId = [guid]::NewGuid().ToString()
    } | ConvertTo-Json) | Out-Null
}
$ha = AuthHeaders $appr
$ai = Invoke-RestMethod -Method Post -Headers $ha -Uri "$base/api/v1/tenants/$t/regulatory/dossiers/$id/approve-for-submission" -Body (@{ notes = 'notify internal' } | ConvertTo-Json)
Write-Host "APPROVE_STATUS=$($ai.status) DOSSIER=$id"
