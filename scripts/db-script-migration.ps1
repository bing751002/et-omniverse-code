param(
  [ValidateSet("Debug", "Release")]
  [string]$Configuration = "Debug",
  [string]$OutputPath = "artifacts/migrations/et-omniverse-idempotent.sql",
  [string]$From = "0",
  [string]$To = "",
  [string]$ArtifactsPath = $(Join-Path $env:TEMP "et-omniverse-ef-artifacts")
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot\ensure-dotnet-ef.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot\prepare-dotnet-ef-build.ps1" -Configuration $Configuration -ArtifactsPath $ArtifactsPath

$outputFullPath = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
  $OutputPath
} else {
  Join-Path $repoRoot $OutputPath
}
New-Item -ItemType Directory -Force -Path (Split-Path $outputFullPath -Parent) | Out-Null

$efArgs = @(
  "tool", "run", "dotnet-ef",
  "migrations", "script",
  $From,
  "--project", "src/backend/ETOmniverse.Infrastructure",
  "--startup-project", "src/backend/ETOmniverse.Infrastructure",
  "--context", "EtOmniverseDbContext",
  "--configuration", $Configuration,
  "--idempotent",
  "--no-build",
  "--output", $outputFullPath
)

if (-not [string]::IsNullOrWhiteSpace($To)) {
  $efArgs = @(
    "tool", "run", "dotnet-ef",
    "migrations", "script",
    $From,
    $To,
    "--project", "src/backend/ETOmniverse.Infrastructure",
    "--startup-project", "src/backend/ETOmniverse.Infrastructure",
    "--context", "EtOmniverseDbContext",
    "--configuration", $Configuration,
    "--idempotent",
    "--no-build",
    "--output", $outputFullPath
  )
}

dotnet @efArgs

if ($LASTEXITCODE -ne 0) {
  throw "dotnet-ef migrations script failed."
}

Write-Host "Migration script written to $outputFullPath"
