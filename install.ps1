#Requires -Version 5.1
<#
.SYNOPSIS
    Installs the built plugin into SimplePlanes 2's BepInEx\plugins folder.

.EXAMPLE
    .\install.ps1

.EXAMPLE
    .\install.ps1 -GamePath "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
#>
param(
    [string]$GamePath
)

$ErrorActionPreference = 'Stop'

$dll = Join-Path $PSScriptRoot 'dist\SP2VariableDialogSize.dll'
if (-not (Test-Path $dll)) {
    throw "dist\SP2VariableDialogSize.dll not found. Run .\build.ps1 first."
}

if (-not $GamePath) {
    $steamPath = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction SilentlyContinue).SteamPath
    $candidates = @()
    if ($steamPath) {
        $steamPath = $steamPath -replace '/', '\'
        $candidates += Join-Path $steamPath 'steamapps\common\SimplePlanes 2'
    }
    $candidates += 'C:\Program Files (x86)\Steam\steamapps\common\SimplePlanes 2'
    $candidates += 'D:\SteamLibrary\steamapps\common\SimplePlanes 2'

    foreach ($c in $candidates) {
        if (Test-Path (Join-Path $c 'SimplePlanes 2_Data\Managed')) {
            $GamePath = $c
            break
        }
    }
}

if (-not $GamePath -or -not (Test-Path (Join-Path $GamePath 'SimplePlanes 2_Data\Managed'))) {
    throw 'Could not locate SimplePlanes 2. Pass -GamePath "<path to the game folder>".'
}

$plugins = Join-Path $GamePath 'BepInEx\plugins'
if (-not (Test-Path $plugins)) {
    throw "BepInEx\plugins not found in $GamePath. Install BepInEx 5 first."
}

Copy-Item $dll (Join-Path $plugins 'SP2VariableDialogSize.dll') -Force
Write-Host "Installed to: $plugins"
Write-Host "Config file: $(Join-Path $GamePath 'BepInEx\config\sp2.variable-dialog-size.cfg')"
