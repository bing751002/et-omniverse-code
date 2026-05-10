param(
  [ValidateSet("Debug", "Release")]
  [string]$Configuration = "Debug",
  [string]$Environment = "Development",
  [string]$ConnectionString = "",
  [string]$Migration = "",
  [string]$ArtifactsPath = $(Join-Path $env:TEMP "et-omniverse-ef-artifacts")
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot\ensure-dotnet-ef.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot\prepare-dotnet-ef-build.ps1" -Configuration $Configuration -ArtifactsPath $ArtifactsPath

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$previousConnectionString = $env:ConnectionStrings__Default

try {
  $env:ASPNETCORE_ENVIRONMENT = $Environment
  if (-not [string]::IsNullOrWhiteSpace($ConnectionString)) {
    $env:ConnectionStrings__Default = $ConnectionString
  }

  $efArgs = @(
    "tool", "run", "dotnet-ef",
    "database", "update"
  )

  if (-not [string]::IsNullOrWhiteSpace($Migration)) {
    $efArgs += $Migration
  }

  $efArgs += @(
    "--project", "src/backend/ETOmniverse.Infrastructure",
    "--startup-project", "src/backend/ETOmniverse.Infrastructure",
    "--context", "EtOmniverseDbContext",
    "--configuration", $Configuration,
    "--no-build"
  )

  dotnet @efArgs

  if ($LASTEXITCODE -ne 0) {
    throw "dotnet-ef database update failed."
  }
} finally {
  $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
  $env:ConnectionStrings__Default = $previousConnectionString
}
