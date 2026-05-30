$ErrorActionPreference = "Stop"

$workspace = Split-Path -Parent $PSScriptRoot
$frontendDir = Join-Path $workspace "frontend"
$logDir = Join-Path $workspace ".logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$stdoutLog = Join-Path $logDir "frontend-stdout.log"
$stderrLog = Join-Path $logDir "frontend-stderr.log"
if (Test-Path $stdoutLog) { Remove-Item -Force $stdoutLog }
if (Test-Path $stderrLog) { Remove-Item -Force $stderrLog }
$psi = [System.Diagnostics.ProcessStartInfo]::new()
$psi.FileName = "npm.cmd"
$psi.WorkingDirectory = $frontendDir
$psi.Arguments = "run dev -- --host 127.0.0.1"
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true

$process = [System.Diagnostics.Process]::new()
$process.StartInfo = $psi
$null = $process.Start()
$process.BeginOutputReadLine()
$process.BeginErrorReadLine()
$process.add_OutputDataReceived({
    param($sender, $args)
    if ($args.Data) {
        Add-Content -Path $stdoutLog -Value $args.Data
    }
})
$process.add_ErrorDataReceived({
    param($sender, $args)
    if ($args.Data) {
        Add-Content -Path $stderrLog -Value $args.Data
    }
})
