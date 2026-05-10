param(
  [ValidateSet("Debug", "Release")]
  [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$tempRoot = if ($env:TEMP) { $env:TEMP } else { Join-Path (Get-Location) ".tmp" }
$artifacts = Join-Path $tempRoot "et-omniverse-ci-artifacts"
$packages = Join-Path $tempRoot "et-omniverse-ci-nuget"

& (Join-Path $PSScriptRoot "verify-local.ps1") `
  -Configuration $Configuration `
  -ArtifactsPath $artifacts `
  -PackagesPath $packages

if ($LASTEXITCODE -ne 0) {
  exit $LASTEXITCODE
}
