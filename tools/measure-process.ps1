param(
    [Parameter(Mandatory = $true)][int]$ProcessId,
    [int]$DurationSeconds = 300,
    [int]$IntervalSeconds = 5,
    [string]$OutputPath = ".\resource-samples.csv"
)
$ErrorActionPreference = "Stop"
$logicalProcessors = [Environment]::ProcessorCount
$end = (Get-Date).AddSeconds($DurationSeconds)
$previous = Get-Process -Id $ProcessId
$previousCpu = $previous.CPU
$previousTime = Get-Date
$results = New-Object System.Collections.Generic.List[object]

while ((Get-Date) -lt $end) {
    Start-Sleep -Seconds $IntervalSeconds
    try { $p = Get-Process -Id $ProcessId } catch { break }
    $p.Refresh()
    $now = Get-Date
    $elapsed = ($now - $previousTime).TotalSeconds
    $cpuDelta = $p.CPU - $previousCpu
    $cpuPercent = if ($elapsed -gt 0) { ($cpuDelta / $elapsed / $logicalProcessors) * 100 } else { 0 }
    $row = [PSCustomObject]@{
        Timestamp=$now.ToString("o")
        CpuPercent=[Math]::Round($cpuPercent,3)
        WorkingSetMB=[Math]::Round($p.WorkingSet64/1MB,2)
        PrivateMemoryMB=[Math]::Round($p.PrivateMemorySize64/1MB,2)
        HandleCount=$p.HandleCount
        ThreadCount=$p.Threads.Count
    }
    $results.Add($row)
    Write-Host ("{0} CPU={1}% WS={2}MB Private={3}MB Handles={4} Threads={5}" -f $now.ToString("HH:mm:ss"),$row.CpuPercent,$row.WorkingSetMB,$row.PrivateMemoryMB,$row.HandleCount,$row.ThreadCount)
    $previousCpu=$p.CPU
    $previousTime=$now
}
$results | Export-Csv -Path $OutputPath -NoTypeInformation -Encoding UTF8
if ($results.Count -gt 0) {
    $avgCpu=($results|Measure-Object CpuPercent -Average).Average
    Write-Host ("Average CPU: {0:N3}%" -f $avgCpu)
    Write-Host ("CSV: {0}" -f (Resolve-Path $OutputPath))
}
