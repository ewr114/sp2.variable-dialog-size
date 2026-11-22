#Requires -Version 5.1
<#
.SYNOPSIS
    Runs the offline unit tests for the XML patching logic.

.DESCRIPTION
    Compiles tests/TestPatcher.cs together with src/DialogSizePatcher.cs and runs it
    against the synthetic fixtures in tests/fixtures. No game installation is required.
#>
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$tests = Join-Path $root 'tests'
$bin = Join-Path $tests 'bin'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'

if (-not (Test-Path $csc)) {
    throw "C# compiler not found: $csc"
}

New-Item -ItemType Directory -Force -Path $bin | Out-Null

$output = & $csc /nologo "/out:$bin\TestPatcher.exe" `
    /r:"$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\System.Xml.Linq.dll" `
    /r:"$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\System.Core.dll" `
    (Join-Path $tests 'TestPatcher.cs') `
    (Join-Path $root 'src\DialogSizePatcher.cs') 2>&1

if ($LASTEXITCODE -ne 0) {
    $output | ForEach-Object { Write-Host $_ }
    throw 'Test build failed.'
}

Copy-Item (Join-Path $tests 'fixtures\*.xml') $bin -Force

& (Join-Path $bin 'TestPatcher.exe')
exit $LASTEXITCODE
