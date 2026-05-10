param(
  [Parameter(Mandatory = $true)]
  [string] $Name,
  [ValidateSet("Debug", "Release")]
  [string] $Configuration = "Debug",
  [string]$ArtifactsPath = $(Join-Path $env:TEMP "et-omniverse-ef-artifacts")
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot\ensure-dotnet-ef.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot\prepare-dotnet-ef-build.ps1" -Configuration $Configuration -ArtifactsPath $ArtifactsPath

dotnet tool run dotnet-ef migrations add $Name `
  --project src/backend/ETOmniverse.Infrastructure `
  --startup-project src/backend/ETOmniverse.Infrastructure `
  --context EtOmniverseDbContext `
  --configuration $Configuration `
  --output-dir Persistence/Migrations `
  --no-build

if ($LASTEXITCODE -ne 0) {
  throw "dotnet-ef migrations add failed."
}
