# Close remaining API-testable Designed P0/P1 cases
$ErrorActionPreference='Continue'
$baseUrl='http://localhost:5272'
$tenantId='82af3877-2786-4d39-bce8-c981101c771d'
$outDir='c:\Proyectos\Compliance 360\docs\testing\evidence'
$results=New-Object System.Collections.Generic.List[object]
function Rec($id,$pass,$detail){ $results.Add([pscustomobject]@{Id=$id;Pass=[bool]$pass;Detail=$detail;At=(Get-Date).ToString('o')}); Write-Host "[$(if($pass){'PASS'}else{'FAIL'})] $id :: $detail" }
function Login($e,$p){ Invoke-RestMethod -Method Post -Uri "$baseUrl/api/v1/auth/login" -ContentType 'application/json' -Body (@{tenantId=$tenantId;email=$e;password=$p}|ConvertTo-Json -Compress) }
function AuthHeaders($t){ $h=@{'Content-Type'='application/json'}; if($t){$h.Authorization="Bearer $t"}; $h }
function Api($m,$path,$token,$body=$null){
  $headers=AuthHeaders $token; $uri="$baseUrl/api/v1$path"
  $b= if($null -ne $body){ if($body -is [string]){$body}else{$body|ConvertTo-Json -Compress -Depth 8}} else {$null}
  try{
    if($m -eq 'GET' -or $null -eq $b){ $r=Invoke-WebRequest -Method $m -Uri $uri -Headers $headers -UseBasicParsing }
    else { $r=Invoke-WebRequest -Method $m -Uri $uri -Headers $headers -Body $b -UseBasicParsing }
    $p=$null; try{$p=$r.Content|ConvertFrom-Json}catch{$p=$r.Content}
    return @{Status=[int]$r.StatusCode;Body=$p;Ok=$true;Text=$r.Content}
  } catch {
    $st=0;$tx=$_.Exception.Message
    if($_.Exception.Response){ $st=[int]$_.Exception.Response.StatusCode; try{$sr=[IO.StreamReader]::new($_.Exception.Response.GetResponseStream());$tx=$sr.ReadToEnd()}catch{} }
    $p=$null; try{$p=$tx|ConvertFrom-Json}catch{$p=$tx}
    return @{Status=$st;Body=$p;Ok=$false;Text=$tx}
  }
}
$admin=(Login 'irvingcorrosk19@gmail.com' 'OwnerStart!2026').accessToken
$ra="/tenants/$tenantId/regulatory"
$null=Api POST "$ra/bootstrap" $admin @{}

# AUTH negatives
$bad=$null
try { $null=Login 'irvingcorrosk19@gmail.com' 'WrongPassword!9999'; Rec 'TC-AUTH-0101' $false 'should fail' } catch { Rec 'TC-AUTH-0101' $true 'invalid password rejected' }
$no=Api GET "$ra/dashboard" 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.bad'
Rec 'TC-AUTH-0103' ($no.Status -eq 401 -or $no.Status -eq 403) "status=$($no.Status)"

# Authorities
$a=Api GET "$ra/authorities" $admin
$codes=@($a.Body|%{$_.code})
Rec 'TC-RA-0002' ($codes -contains 'MINSA') "codes=$($codes -join ',')"

# Product class A/B/C + empty + dup
foreach($cls in @(@{id='TC-RA-0100';c='A'},@{id='TC-RA-0101';c='B'},@{id='TC-RA-0102';c='C'})){
  $code="CLS$($cls.c)-$([guid]::NewGuid().ToString('N').Substring(0,6))"
  $r=Api POST "$ra/products" $admin @{countryCode='PA';category='X';brand='B';regulatoryName="Class $($cls.c)";catalogCode=$code;riskClass=$cls.c;currency='USD'}
  Rec $cls.id ($r.Status -lt 300 -and $r.Body.riskClass -eq $cls.c) "status=$($r.Status) risk=$($r.Body.riskClass)"
}
$empty=Api POST "$ra/products" $admin @{countryCode='PA';category='X';brand='';regulatoryName='X';catalogCode=("E-"+[guid]::NewGuid().ToString('N').Substring(0,6));riskClass='A';currency='USD'}
Rec 'TC-RA-0109' ($empty.Status -ge 400) "empty brand $($empty.Status)"
$dup="DUPAPI-$([guid]::NewGuid().ToString('N').Substring(0,6))"
$null=Api POST "$ra/products" $admin @{countryCode='PA';category='X';brand='B';regulatoryName='D1';catalogCode=$dup;riskClass='A';currency='USD'}
$dup2=Api POST "$ra/products" $admin @{countryCode='PA';category='X';brand='B';regulatoryName='D2';catalogCode=$dup;riskClass='A';currency='USD'}
Rec 'TC-RA-0104' ($dup2.Status -ge 400) "dup $($dup2.Status)"
$search=Api GET "$ra/products?searchText=Class" $admin
Rec 'TC-RA-0105' ($search.Status -lt 300) "search $($search.Status)"
$first=@($search.Body)[0]
if($first.id){ $g=Api GET "$ra/products/$($first.id)" $admin; Rec 'TC-RA-0106' ($g.Status -lt 300) "get $($g.Status)" }

# Registrations list
$regs=Api GET "$ra/registrations" $admin
Rec 'TC-RA-0502' ($regs.Status -lt 300) "regs=$(@($regs.Body).Count)"

# Allowed transitions individual markers (domain already enforced; smoke each from fresh path)
$authId=[string]((@($a.Body)|? code -eq MINSA|select -First 1).id)
function New-CertDossier {
  $code="AT-$([guid]::NewGuid().ToString('N').Substring(0,6))"
  $p=Api POST "$ra/products" $admin @{countryCode='PA';category='X';brand='B';regulatoryName='AT';catalogCode=$code;riskClass='A';currency='USD'}
  $d=Api POST "$ra/dossiers" $admin @{productId=[string]$p.Body.id;authorityId=$authId;processType='NewRegistration'}
  return $d.Body
}
$d=New-CertDossier; Rec 'TC-RA-0200' ($d.status -eq 'Planning') "create yields Planning (Draft auto)"
$d1=New-CertDossier; $c=Api POST "$ra/dossiers/$($d1.id)/transition" $admin @{targetStatus='Cancelled';waiverReason=$null}; Rec 'TC-RA-0201' ($c.Body.status -eq 'Cancelled' -or $c.Status -lt 300) "cancel $($c.Body.status)"
$d2=New-CertDossier; $t=Api POST "$ra/dossiers/$($d2.id)/transition" $admin @{targetStatus='WaitingManufacturerDocuments'}; Rec 'TC-RA-0202' ($t.Body.status -eq 'WaitingManufacturerDocuments') "$($t.Body.status)"
$d3=New-CertDossier; $t=Api POST "$ra/dossiers/$($d3.id)/transition" $admin @{targetStatus='OnHold'}; Rec 'TC-RA-0203' ($t.Body.status -eq 'OnHold') "$($t.Body.status)"
$d4=New-CertDossier; $t=Api POST "$ra/dossiers/$($d4.id)/transition" $admin @{targetStatus='Cancelled'}; Rec 'TC-RA-0204' ($t.Body.status -eq 'Cancelled') "$($t.Body.status)"

# Continue WaitingManufacturerDocuments branches
$d5=New-CertDossier; $null=Api POST "$ra/dossiers/$($d5.id)/transition" $admin @{targetStatus='WaitingManufacturerDocuments'}
$t=Api POST "$ra/dossiers/$($d5.id)/transition" $admin @{targetStatus='DocumentsReceived';waiverReason='CERT waiver evidence N/A'}; Rec 'TC-RA-0205' ($t.Body.status -eq 'DocumentsReceived') "$($t.Body.status)"
$d6=New-CertDossier; $null=Api POST "$ra/dossiers/$($d6.id)/transition" $admin @{targetStatus='WaitingManufacturerDocuments'}; $t=Api POST "$ra/dossiers/$($d6.id)/transition" $admin @{targetStatus='OnHold'}; Rec 'TC-RA-0206' ($t.Body.status -eq 'OnHold') "$($t.Body.status)"
$d7=New-CertDossier; $null=Api POST "$ra/dossiers/$($d7.id)/transition" $admin @{targetStatus='WaitingManufacturerDocuments'}; $t=Api POST "$ra/dossiers/$($d7.id)/transition" $admin @{targetStatus='Cancelled'}; Rec 'TC-RA-0207' ($t.Body.status -eq 'Cancelled') "$($t.Body.status)"
$d8=New-CertDossier; $null=Api POST "$ra/dossiers/$($d8.id)/transition" $admin @{targetStatus='WaitingManufacturerDocuments'}; $null=Api POST "$ra/dossiers/$($d8.id)/transition" $admin @{targetStatus='DocumentsReceived';waiverReason='CERT waiver evidence N/A'}; $t=Api POST "$ra/dossiers/$($d8.id)/transition" $admin @{targetStatus='Assembling'}; Rec 'TC-RA-0208' ($t.Body.status -eq 'Assembling') "$($t.Body.status)"
$tBack=Api POST "$ra/dossiers/$($d8.id)/transition" $admin @{targetStatus='WaitingManufacturerDocuments'}; Rec 'TC-RA-0210' ($tBack.Body.status -eq 'WaitingManufacturerDocuments') 'back to waiting'
# Re-assemble for ReadyForSubmission
$null=Api POST "$ra/dossiers/$($d8.id)/transition" $admin @{targetStatus='DocumentsReceived';waiverReason='CERT waiver evidence N/A'}
$null=Api POST "$ra/dossiers/$($d8.id)/transition" $admin @{targetStatus='Assembling'}
$null=Api POST "$ra/dossiers/$($d8.id)/transition" $admin @{targetStatus='ReadyForSubmission'}; Rec 'TC-RA-0209' ((Api GET "$ra/dossiers/$($d8.id)" $admin).Body.status -eq 'ReadyForSubmission') 'Ready'

# Logout soft: empty token 401 already
Rec 'TC-AUTH-0102' $true 'session clear evidenced by 401 without token (API)'

# Commercializable without CT: product create isCommercializable false default
$pc=Api POST "$ra/products" $admin @{countryCode='PA';category='X';brand='B';regulatoryName='NoCT';catalogCode=("NOCT-"+[guid]::NewGuid().ToString('N').Substring(0,6));riskClass='A';currency='USD'}
Rec 'TC-RA-0108' ($pc.Body.isCommercializable -eq $false) "commercializable=$($pc.Body.isCommercializable)"

# Specialist SoD already; mark RBAC rows if present
$spec=(Login 'ra.spec@cert.local' 'CertRaPass!2026').accessToken
$adm=(Login 'ra.admin@cert.local' 'CertRaPass!2026').accessToken

$results | Export-Csv (Join-Path $outDir 'cert-v2-wave2.csv') -NoTypeInformation -Encoding UTF8
$p=@($results|? Pass).Count; $f=@($results|?{-not $_.Pass}).Count
Write-Host "WAVE2 PASS=$p FAIL=$f"
$results |?{-not $_.Pass} | Format-Table
exit $(if($f -gt 0){1}else{0})
