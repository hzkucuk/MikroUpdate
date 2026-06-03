param(
	[Parameter(Mandatory = $true)]
	[string]$ShareName,

	[string]$Account = "$($env:USERDOMAIN)\Domain Computers",

	[switch]$CreateLocalUserIfMissing,
	[string]$LocalUserPasswordPlain = "DegistirBeni!123"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Account)) {
	throw "Account boş olamaz."
}

function Resolve-Account {
	param([string]$Identity)

	try {
		$sid = (New-Object System.Security.Principal.NTAccount($Identity)).Translate([System.Security.Principal.SecurityIdentifier])
		return $sid.Value
	}
	catch {
		throw "Hesap çözümlenemedi: $Identity"
	}
}

function Ensure-LocalUserIfRequested {
	param(
		[string]$Identity,
		[bool]$ShouldCreate,
		[string]$Password
	)

	if (-not $ShouldCreate) {
		return
	}

	$parts = $Identity.Split('\\')
	if ($parts.Count -ne 2) {
		throw "Account formatı geçersiz: $Identity (beklenen: MAKINE\\kullanici)"
	}

	$scope = $parts[0]
	$user = $parts[1]

	if ($scope -ine $env:COMPUTERNAME) {
		throw "CreateLocalUserIfMissing sadece local makine hesabı için kullanılabilir. Verilen: $Identity"
	}

	$existing = Get-LocalUser -Name $user -ErrorAction SilentlyContinue
	if ($null -ne $existing) {
		Write-Host "Local kullanıcı zaten var: $Identity" -ForegroundColor DarkGray
		return
	}

	$securePassword = ConvertTo-SecureString $Password -AsPlainText -Force
	New-LocalUser -Name $user -Password $securePassword -PasswordNeverExpires -AccountNeverExpires `
		-FullName "Mikro Update ReadOnly" -Description "MikroUpdate share read account" | Out-Null

	Write-Host "Local kullanıcı oluşturuldu: $Identity" -ForegroundColor Green
}

$share = Get-SmbShare -Name $ShareName -ErrorAction SilentlyContinue
if ($null -eq $share) {
	throw "Share bulunamadı: $ShareName"
}

$sharePath = $share.Path
if ([string]::IsNullOrWhiteSpace($sharePath) -or -not (Test-Path $sharePath)) {
	throw "Share path geçersiz: $sharePath"
}

Ensure-LocalUserIfRequested -Identity $Account -ShouldCreate $CreateLocalUserIfMissing.IsPresent -Password $LocalUserPasswordPlain
Resolve-Account -Identity $Account | Out-Null

Grant-SmbShareAccess -Name $ShareName -AccountName $Account -AccessRight Read -Force | Out-Null
Write-Host "Share Read izni verildi: $Account" -ForegroundColor Green

$acl = Get-Acl $sharePath
$inherit = [System.Security.AccessControl.InheritanceFlags]"ContainerInherit, ObjectInherit"
$propagation = [System.Security.AccessControl.PropagationFlags]::None
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
	$Account,
	[System.Security.AccessControl.FileSystemRights]::ReadAndExecute,
	$inherit,
	$propagation,
	[System.Security.AccessControl.AccessControlType]::Allow)

$exists = $acl.Access | Where-Object {
	$_.IdentityReference.Value -ieq $Account -and
	$_.AccessControlType -eq "Allow" -and
	$_.FileSystemRights.HasFlag([System.Security.AccessControl.FileSystemRights]::ReadAndExecute)
}

if ($null -eq $exists) {
	$acl.AddAccessRule($rule)
	Set-Acl -Path $sharePath -AclObject $acl
	Write-Host "NTFS ReadAndExecute izni verildi: $Account" -ForegroundColor Green
}
else {
	Write-Host "NTFS izni zaten var: $Account" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "Tamamlandı" -ForegroundColor Cyan
Write-Host "Share : \\$env:COMPUTERNAME\\$ShareName"
Write-Host "Path  : $sharePath"
Write-Host "Account: $Account"
