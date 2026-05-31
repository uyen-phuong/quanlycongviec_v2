$ErrorActionPreference = "Stop"

$workspace = Split-Path -Parent $PSScriptRoot
$logDir = Join-Path $workspace ".logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$stdoutLog = Join-Path $logDir "backend-stdout.log"
$stderrLog = Join-Path $logDir "backend-stderr.log"
if (Test-Path $stdoutLog) { Remove-Item -Force $stdoutLog }
if (Test-Path $stderrLog) { Remove-Item -Force $stderrLog }

$command = "$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_HOME='$workspace\.dotnet'; $env:APPDATA='$workspace\.appdata'; $env:ASPNETCORE_URLS='http://127.0.0.1:5051'; dotnet run --project '$workspace\backend\KHCT.Api\KHCT.Api.csproj'"

$psi = [System.Diagnostics.ProcessStartInfo]::new()
$psi.FileName = "powershell.exe"
$psi.WorkingDirectory = $workspace
$psi.Arguments = "-NoProfile -Command $command"
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true

$process = [System.Diagnostics.Process]::new()
$process.StartInfo = $psi
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

$null = $process.Start()
$process.BeginOutputReadLine()
$process.BeginErrorReadLine()
