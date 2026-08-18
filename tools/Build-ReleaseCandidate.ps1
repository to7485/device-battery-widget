[CmdletBinding()]
param(
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version = '1.0.0',
    [switch]$BuildInstaller
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot 'src\DeviceBattery.App\DeviceBattery.App.csproj'
$releaseRoot = Join-Path $repoRoot "artifacts\release\v$Version"
$scd = Join-Path $releaseRoot 'win-x64-self-contained'
$fdd = Join-Path $releaseRoot 'win-x64-framework-dependent'
$buildOutput = Join-Path $releaseRoot '_build'

$resolvedRepo = [System.IO.Path]::GetFullPath($repoRoot).TrimEnd('\')
$resolvedRelease = [System.IO.Path]::GetFullPath($releaseRoot)
if (-not $resolvedRelease.StartsWith("$resolvedRepo\artifacts\release\v", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Unsafe release output path: $resolvedRelease"
}

if (Test-Path -LiteralPath $releaseRoot) {
    throw "Release output already exists: $releaseRoot. Preserve or remove it explicitly before rebuilding."
}

New-Item -ItemType Directory -Path $releaseRoot | Out-Null

& dotnet publish $project -c Release -r win-x64 --self-contained true `
    -p:Version=$Version -p:DebugType=None -p:DebugSymbols=false `
    -p:PublishSingleFile=false -p:PublishTrimmed=false `
    -p:BaseOutputPath="$buildOutput\scd\" -o $scd
if ($LASTEXITCODE -ne 0) { throw "Self-contained publish failed with exit code $LASTEXITCODE." }

& dotnet publish $project -c Release -r win-x64 --self-contained false `
    -p:Version=$Version -p:DebugType=None -p:DebugSymbols=false `
    -p:PublishSingleFile=false -p:PublishTrimmed=false `
    -p:BaseOutputPath="$buildOutput\fdd\" -o $fdd
if ($LASTEXITCODE -ne 0) { throw "Framework-dependent publish failed with exit code $LASTEXITCODE." }

$scdZip = Join-Path $releaseRoot "DeviceBatteryWidget-$Version-win-x64-self-contained.zip"
$fddZip = Join-Path $releaseRoot "DeviceBatteryWidget-$Version-win-x64-framework-dependent.zip"
Compress-Archive -Path (Join-Path $scd '*') -DestinationPath $scdZip -CompressionLevel Optimal
Compress-Archive -Path (Join-Path $fdd '*') -DestinationPath $fddZip -CompressionLevel Optimal

if ($BuildInstaller) {
    $iscc = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    $isccPath = if ($null -ne $iscc) { $iscc.Source } else { $null }
    if ([string]::IsNullOrWhiteSpace($isccPath)) {
        $userIscc = Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'
        $machineIscc = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'
        if (Test-Path -LiteralPath $userIscc) { $isccPath = $userIscc }
        elseif (Test-Path -LiteralPath $machineIscc) { $isccPath = $machineIscc }
    }
    if ([string]::IsNullOrWhiteSpace($isccPath)) { throw 'Inno Setup ISCC.exe is required to build the installer.' }
    & $isccPath "/DMyAppVersion=$Version" (Join-Path $repoRoot 'installer\DeviceBatteryWidget.iss')
    if ($LASTEXITCODE -ne 0) { throw "Installer build failed with exit code $LASTEXITCODE." }
}

$hashFile = Join-Path $releaseRoot 'SHA256SUMS.txt'
Get-ChildItem -LiteralPath $releaseRoot -File |
    Where-Object Name -Like "DeviceBatteryWidget-$Version-*" |
    Where-Object Extension -In '.zip', '.exe' |
    Sort-Object Name |
    ForEach-Object {
        $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $_.FullName).Hash.ToLowerInvariant()
        "$hash  $($_.Name)"
    } | Set-Content -LiteralPath $hashFile -Encoding ascii

Write-Host "Release candidate prepared at $releaseRoot"
Get-Content -LiteralPath $hashFile
