#Requires -Version 5.1
<#
.SYNOPSIS
    Builds the SP2 Variable Dialog Size BepInEx plugin.

.DESCRIPTION
    Compiles src/*.cs against the assemblies shipped with SimplePlanes 2.
    The game path is auto-detected from Steam, or can be passed explicitly.

.EXAMPLE
    .\build.ps1

.EXAMPLE
    .\build.ps1 -GamePath "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
#>
param(
    [string]$GamePath
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$outDir = Join-Path $root 'dist'
$outDll = Join-Path $outDir 'SP2VariableDialogSize.dll'

function Find-SimplePlanes2 {
    $candidates = New-Object System.Collections.Generic.List[string]

    $steamPath = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction SilentlyContinue).SteamPath
    if ($steamPath) {
        $steamPath = $steamPath -replace '/', '\'
        $candidates.Add((Join-Path $steamPath 'steamapps\common\SimplePlanes 2'))

        $vdf = Join-Path $steamPath 'steamapps\libraryfolders.vdf'
        if (Test-Path $vdf) {
            $content = Get-Content $vdf -Raw
            foreach ($m in [regex]::Matches($content, '"path"\s+"([^"]+)"')) {
                $lib = $m.Groups[1].Value -replace '\\\\', '\'
                $candidates.Add((Join-Path $lib 'steamapps\common\SimplePlanes 2'))
            }
        }
    }

    $candidates.Add('C:\Program Files (x86)\Steam\steamapps\common\SimplePlanes 2')
    $candidates.Add('D:\SteamLibrary\steamapps\common\SimplePlanes 2')

    foreach ($c in $candidates) {
        if ($c -and (Test-Path (Join-Path $c 'SimplePlanes 2_Data\Managed'))) {
            return $c
        }
    }

    return $null
}

if (-not $GamePath) {
    $GamePath = Find-SimplePlanes2
}

if (-not $GamePath) {
    throw 'Could not locate SimplePlanes 2. Pass -GamePath "<path to the game folder>".'
}

$managed = Join-Path $GamePath 'SimplePlanes 2_Data\Managed'
$core = Join-Path $GamePath 'BepInEx\core'

foreach ($p in @($managed, $core)) {
    if (-not (Test-Path $p)) { throw "Directory not found: $p" }
}

$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { throw "C# compiler not found: $csc" }

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

# Compile against the game's own Mono / netstandard 2.1 reference assemblies.
# /noconfig + /nostdlib+ keep the .NET Framework's own BCL copies out of the way.
$refs = @(
    (Join-Path $managed 'mscorlib.dll'),
    (Join-Path $managed 'netstandard.dll'),
    (Join-Path $managed 'System.dll'),
    (Join-Path $managed 'System.Core.dll'),
    (Join-Path $managed 'System.Xml.dll'),
    (Join-Path $managed 'System.Xml.Linq.dll'),
    (Join-Path $managed 'System.Runtime.dll'),
    (Join-Path $managed 'Jundroo.Packages.dll'),
    (Join-Path $managed 'Game.dll'),
    (Join-Path $managed 'UnityEngine.dll'),
    (Join-Path $managed 'UnityEngine.CoreModule.dll'),
    (Join-Path $managed 'UnityEngine.UIModule.dll'),
    (Join-Path $managed 'UnityEngine.UI.dll'),
    (Join-Path $core 'BepInEx.dll'),
    (Join-Path $core '0Harmony.dll')
) | ForEach-Object { "/r:$_" }

$sources = @(
    (Join-Path $root 'src\Plugin.cs'),
    (Join-Path $root 'src\DialogSizePatcher.cs'),
    (Join-Path $root 'src\DialogResize.cs')
)

$arguments = @(
    '/nologo', '/noconfig', '/nostdlib+', '/target:library',
    "/out:$outDll"
) + $refs + $sources

$output = & $csc $arguments 2>&1
if ($LASTEXITCODE -ne 0) {
    $output | ForEach-Object { Write-Host $_ }
    throw 'Build failed.'
}

Write-Host "Built: $outDll"
Write-Host ("Size: " + (Get-Item $outDll).Length + " bytes")
