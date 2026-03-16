<#
.SYNOPSIS
    MikroUpdate Inno Setup kurulum paketi oluşturma scripti.

.DESCRIPTION
    1. Her iki projeyi publish eder (framework-dependent)
    2. Inno Setup Compiler (ISCC) ile EXE installer derler

.EXAMPLE
    .\Build-Setup.ps1
    .\Build-Setup.ps1 -Configuration Debug
#>

param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$SolutionRoot = Split-Path -Parent $PSScriptRoot
$PublishWin = Join-Path $SolutionRoot "publish\win"
$PublishService = Join-Path $SolutionRoot "publish\service"
$IssFile = Join-Path $PSScriptRoot "MikroUpdate.iss"
$OutputDir = Join-Path $SolutionRoot "installer"

# ── ISCC yolunu bul ──────────────────────────────────────────
$IsccPaths = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe"
)

$IsccExe = $null
foreach ($p in $IsccPaths) {
    if (Test-Path $p) { $IsccExe = $p; break }
}

if (-not $IsccExe) {
    Write-Host "HATA: Inno Setup bulunamadi!" -ForegroundColor Red
    Write-Host "  Inno Setup 6 indirin: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MikroUpdate Inno Setup Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ISCC: $IsccExe" -ForegroundColor Gray
Write-Host ""

# ── 1. Temizlik ──────────────────────────────────────────────
Write-Host "[1/4] Onceki publish ciktilari temizleniyor..." -ForegroundColor Yellow

if (Test-Path $PublishWin) { Remove-Item $PublishWin -Recurse -Force }
if (Test-Path $PublishService) { Remove-Item $PublishService -Recurse -Force }

# ── 2. Publish: Win ─────────────────────────────────────────
Write-Host "[2/4] MikroUpdate.Win publish ediliyor..." -ForegroundColor Yellow
dotnet publish "$SolutionRoot\MikroUpdate.Win" `
    -c $Configuration `
    -o $PublishWin `
    --no-self-contained `
    -v quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "HATA: MikroUpdate.Win publish basarisiz!" -ForegroundColor Red
    exit 1
}

# ── 3. Publish: Service ─────────────────────────────────────
Write-Host "[3/4] MikroUpdate.Service publish ediliyor..." -ForegroundColor Yellow
dotnet publish "$SolutionRoot\MikroUpdate.Service" `
    -c $Configuration `
    -o $PublishService `
    --no-self-contained `
    -v quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "HATA: MikroUpdate.Service publish basarisiz!" -ForegroundColor Red
    exit 1
}

# ── 4. Inno Setup derle ─────────────────────────────────────
Write-Host "[4/4] Inno Setup paketi derleniyor..." -ForegroundColor Yellow

if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null }

& $IsccExe $IssFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "HATA: Inno Setup derleme basarisiz!" -ForegroundColor Red
    exit 1
}

# ── 5. Sonuc ─────────────────────────────────────────────────
$SetupFile = Get-ChildItem $OutputDir -Filter "MikroUpdate_Setup_*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Installer basariyla olusturuldu!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Dosya: $($SetupFile.FullName)" -ForegroundColor White
Write-Host "  Boyut: $([math]::Round($SetupFile.Length / 1KB, 1)) KB" -ForegroundColor White
Write-Host ""
Write-Host "  Kurulum:  .\$($SetupFile.Name)" -ForegroundColor Gray
Write-Host "  Sessiz:   .\$($SetupFile.Name) /VERYSILENT /SUPPRESSMSGBOXES" -ForegroundColor Gray
Write-Host "  Kaldirma: Denetim Masasi veya unins000.exe" -ForegroundColor Gray
Write-Host ""
