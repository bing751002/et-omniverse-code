param(
  [string]$OpenApiPath = "src/frontend/ETOmniverse.Web/src/api/openapi.json",
  [string]$GeneratedPath = "src/frontend/ETOmniverse.Web/src/api/generated/openapi-contract.ts"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

if (-not (Test-Path $OpenApiPath)) {
  throw "OpenAPI snapshot not found: $OpenApiPath"
}

if (-not (Test-Path $GeneratedPath)) {
  throw "Generated API contract not found: $GeneratedPath"
}

$tempOutput = Join-Path $env:TEMP ("et-omniverse-openapi-contract-" + [guid]::NewGuid().ToString("N") + ".ts")
node scripts/generate-frontend-api-contract.mjs $OpenApiPath $tempOutput
if ($LASTEXITCODE -ne 0) {
  throw "Contract generation failed."
}

$expected = Get-Content $GeneratedPath -Raw
$actual = Get-Content $tempOutput -Raw

if ($expected -ne $actual) {
  throw "Generated frontend API contract is stale. Run: node scripts/generate-frontend-api-contract.mjs"
}

Write-Host "OK frontend API contract is up to date"
