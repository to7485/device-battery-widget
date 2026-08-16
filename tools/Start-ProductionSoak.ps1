[CmdletBinding()]
param(
    [ValidateRange(10, 259200)]
    [int]$DurationSeconds = 259200,

    [ValidateRange(1, 60)]
    [int]$IntervalSeconds = 60
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$solution = Join-Path $repoRoot 'DeviceBatteryWidget.slnx'
$samplerProject = Join-Path $repoRoot 'poc\DeviceBattery.Poc.ResourceSampler\DeviceBattery.Poc.ResourceSampler.csproj'
$appExe = Join-Path $repoRoot 'src\DeviceBattery.App\bin\Release\net10.0-windows10.0.19041.0\DeviceBatteryWidget.exe'
$samplerExe = Join-Path $repoRoot 'poc\DeviceBattery.Poc.ResourceSampler\bin\Release\net10.0\DeviceBattery.Poc.ResourceSampler.exe'
$outputDirectory = Join-Path $repoRoot 'artifacts\resource'

$existing = @(Get-Process -Name 'DeviceBatteryWidget' -ErrorAction SilentlyContinue)
if ($existing.Count -gt 0) {
    $ids = ($existing.Id -join ', ')
    throw "DeviceBatteryWidget is already running (PID: $ids). Exit it from the tray before starting a new soak."
}

Write-Host 'Building the current Release app and ResourceSampler...'
& dotnet build $solution -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Release solution build failed with exit code $LASTEXITCODE." }
& dotnet build $samplerProject -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "ResourceSampler build failed with exit code $LASTEXITCODE." }

New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$hours = [Math]::Round($DurationSeconds / 3600, 2)
$stage = if ($DurationSeconds -eq 259200) { 'G6-PRODUCTION-72H-USER' } elseif ($DurationSeconds -eq 86400) { 'G6-PRODUCTION-24H-USER' } else { "G6-PRODUCTION-$($DurationSeconds)S-USER" }

Write-Host "Starting DeviceBatteryWidget for a $hours-hour soak..."
$app = Start-Process -FilePath $appExe -WorkingDirectory (Split-Path -Parent $appExe) -PassThru
Start-Sleep -Seconds 3
if ($app.HasExited) { throw "DeviceBatteryWidget exited during startup with code $($app.ExitCode)." }

$startedAt = Get-Date
$expectedAt = $startedAt.AddSeconds($DurationSeconds)
Write-Host "App PID        : $($app.Id)"
Write-Host "Started        : $startedAt"
Write-Host "Expected finish: $expectedAt"
Write-Host "Output         : $outputDirectory"
Write-Host 'Keep this PowerShell window open. Normal PC use and sleep/resume are allowed.'
Write-Host 'Do not log off, reboot, or exit the app from the tray during the run.'
Write-Host ''

try {
    & $samplerExe --pid $app.Id --stage $stage --duration $DurationSeconds --interval $IntervalSeconds --output $outputDirectory
    $samplerExitCode = $LASTEXITCODE
    if ($samplerExitCode -ne 0) { throw "ResourceSampler finished with exit code $samplerExitCode." }
}
finally {
    $app.Refresh()
    if ($app.HasExited) {
        Write-Warning 'DeviceBatteryWidget is no longer running. Preserve the CSV and report the exit time.'
    }
    else {
        Write-Host "Measurement ended; app PID $($app.Id) is still running. Exit it from the tray after saving the result."
    }
    $app.Dispose()
}
