#Requires -Version 5.1
<#
.SYNOPSIS
    Builds a ready-to-install release archive that bundles BepInEx 5 with this plugin.

.DESCRIPTION
    Downloads (or reuses) the official BepInEx 5.4.23.5 Windows x64 archive, verifies its
    SHA-256, adds dist\SP2VariableDialogSize.dll to BepInEx\plugins, and writes
    release\SP2VariableDialogSize-v<Version>-win-x64.zip plus release\SHA256SUMS.txt.

.EXAMPLE
    .\package.ps1 -Version 1.0.0
#>
param(
    [string]$Version = '1.0.0',
    [string]$BepInExZip,
    [string]$OutputDir
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$pluginDll = Join-Path $root 'dist\SP2VariableDialogSize.dll'
if (-not (Test-Path $pluginDll)) {
    throw "dist\SP2VariableDialogSize.dll not found. Run .\build.ps1 first."
}

$bepVersion = '5.4.23.5'
$bepHash = '82F9878551030F54657792C0740D9D51A09500EEAE1FBA21106B0C441E6732C4'
$bepUrl = "https://github.com/BepInEx/BepInEx/releases/download/v$bepVersion/BepInEx_win_x64_$bepVersion.zip"

if (-not $BepInExZip) {
    $BepInExZip = Join-Path $env:TEMP "BepInEx_win_x64_$bepVersion.zip"
    if (-not (Test-Path $BepInExZip)) {
        Write-Host "Downloading BepInEx $bepVersion ..."
        Invoke-WebRequest -Uri $bepUrl -OutFile $BepInExZip -UseBasicParsing
    }
}

$actualHash = (Get-FileHash $BepInExZip -Algorithm SHA256).Hash
if ($actualHash -ne $bepHash) {
    throw "BepInEx archive checksum mismatch. Expected $bepHash, got $actualHash."
}

if (-not $OutputDir) {
    $OutputDir = Join-Path $root 'release'
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$stage = Join-Path $env:TEMP ('sp2pkg-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $stage | Out-Null

try {
    Expand-Archive -Path $BepInExZip -DestinationPath $stage -Force
    $plugins = Join-Path $stage 'BepInEx\plugins'
    New-Item -ItemType Directory -Force -Path $plugins | Out-Null
    Copy-Item $pluginDll $plugins -Force

    $zipName = "SP2VariableDialogSize-v$Version-win-x64.zip"
    $zipPath = Join-Path $OutputDir $zipName
    Remove-Item $zipPath -Force -ErrorAction SilentlyContinue

    Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zipPath -Force

    $zipHash = (Get-FileHash $zipPath -Algorithm SHA256).Hash.ToLower()
    "$zipHash  $zipName" | Set-Content (Join-Path $OutputDir 'SHA256SUMS.txt') -Encoding ASCII

    Write-Host "Packaged: $zipPath"
    Write-Host "SHA-256:  $zipHash"
}
finally {
    Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
}
