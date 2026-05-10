param(
  [ValidateSet("Debug", "Release")]
  [string]$Configuration = "Debug",
  [string]$ArtifactsPath = $(Join-Path $env:TEMP "et-omniverse-ef-artifacts")
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$projectPath = "src/backend/ETOmniverse.Infrastructure/ETOmniverse.Infrastructure.csproj"

dotnet restore $projectPath `
  --artifacts-path $ArtifactsPath `
  /p:NuGetAudit=false `
  /m:1

if ($LASTEXITCODE -ne 0) {
  throw "dotnet restore for EF tooling failed."
}

dotnet build $projectPath `
  --configuration $Configuration `
  --no-restore `
  --artifacts-path $ArtifactsPath `
  /p:NuGetAudit=false `
  /m:1

if ($LASTEXITCODE -ne 0) {
  throw "dotnet build for EF tooling failed."
}

$sourceOutput = Join-Path $ArtifactsPath ("bin/ETOmniverse.Infrastructure/" + $Configuration.ToLowerInvariant())
$targetOutput = Join-Path $repoRoot "src/backend/ETOmniverse.Infrastructure/bin/$Configuration/net10.0"

if (-not (Test-Path $sourceOutput)) {
  throw "Expected EF build output not found: $sourceOutput"
}

New-Item -ItemType Directory -Force -Path $targetOutput | Out-Null
Copy-Item -Path (Join-Path $sourceOutput "*") -Destination $targetOutput -Recurse -Force

