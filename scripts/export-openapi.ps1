param(
  [string]$OutputPath = "src/frontend/ETOmniverse.Web/src/api/openapi.json",
  [int]$Port = 5097,
  [string]$ArtifactsPath = $(Join-Path $env:TEMP "et-omniverse-artifacts"),
  [int]$TimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$outputFullPath = Join-Path $repoRoot $OutputPath
$outputDirectory = Split-Path $outputFullPath -Parent
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $ArtifactsPath | Out-Null

$stdout = Join-Path $ArtifactsPath "openapi-export.stdout.log"
$stderr = Join-Path $ArtifactsPath "openapi-export.stderr.log"
$baseUrl = "http://127.0.0.1:$Port"
$openApiUrl = "$baseUrl/openapi/v1.json"

$buildArgs = @(
  "build", "src/backend/ETOmniverse.Api/ETOmniverse.Api.csproj",
  "--no-restore",
  "--artifacts-path", $ArtifactsPath,
  "/p:NuGetAudit=false",
  "/m:1",
  "-warnaserror"
)
dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
  throw "API build failed before OpenAPI export."
}

$previousAspNetCoreEnvironment = $env:ASPNETCORE_ENVIRONMENT
$previousAspNetCoreUrls = $env:ASPNETCORE_URLS
$previousOpenApiEnabled = $env:OpenApi__Enabled
$previousConnectionString = $env:ConnectionStrings__Default

$process = $null

try {
  $env:ASPNETCORE_ENVIRONMENT = "Development"
  $env:ASPNETCORE_URLS = $baseUrl
  $env:OpenApi__Enabled = "true"
  if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__Default)) {
    $env:ConnectionStrings__Default = "Server=localhost;Database=ETOmniverse;Trusted_Connection=True;TrustServerCertificate=True"
  }

  $process = Start-Process dotnet -ArgumentList @(
    "run",
    "--project", "src/backend/ETOmniverse.Api",
    "--no-restore",
    "--no-build",
    "--artifacts-path", $ArtifactsPath,
    "--no-launch-profile"
  ) -PassThru -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  $response = $null

  while ((Get-Date) -lt $deadline) {
    if ($process.HasExited) {
      throw "API host exited before OpenAPI was available. See $stdout and $stderr"
    }

    try {
      $response = Invoke-WebRequest -Uri $openApiUrl -UseBasicParsing -TimeoutSec 2
      if ($response.StatusCode -eq 200) {
        break
      }
    } catch {
      Start-Sleep -Milliseconds 500
    }
  }

  if ($null -eq $response -or $response.StatusCode -ne 200) {
    throw "Timed out waiting for $openApiUrl"
  }

  $json = $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 100
  $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
  [System.IO.File]::WriteAllText($outputFullPath, $json + [Environment]::NewLine, $utf8NoBom)
  Write-Host "OpenAPI exported to $OutputPath"
} finally {
  if ($null -ne $process -and -not $process.HasExited) {
    Stop-Process -Id $process.Id -Force
    $process.WaitForExit()
  }

  $env:ASPNETCORE_ENVIRONMENT = $previousAspNetCoreEnvironment
  $env:ASPNETCORE_URLS = $previousAspNetCoreUrls
  $env:OpenApi__Enabled = $previousOpenApiEnabled
  $env:ConnectionStrings__Default = $previousConnectionString
}
