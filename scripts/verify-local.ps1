param(
  [ValidateSet("Debug", "Release")]
  [string]$Configuration = "Debug",
  [string]$ArtifactsPath = $(Join-Path $env:TEMP "et-omniverse-artifacts"),
  [string]$PackagesPath = $(Join-Path $env:TEMP "et-omniverse-nuget"),
  [switch]$SkipRestore,
  [switch]$SkipBuild,
  [switch]$SkipTest,
  [switch]$SkipFrontend,
  [switch]$SkipDocker
)

$ErrorActionPreference = "Stop"

function Invoke-Step {
  param(
    [Parameter(Mandatory = $true)][string]$Name,
    [Parameter(Mandatory = $true)][scriptblock]$Script
  )

  Write-Host "==> $Name"
  & $Script
  Write-Host "OK  $Name"
}

function Invoke-External {
  param(
    [Parameter(Mandatory = $true)][string]$FilePath,
    [Parameter(Mandatory = $true)][string[]]$Arguments,
    [string]$WorkingDirectory = (Get-Location).Path
  )

  & $FilePath @Arguments
  if ($LASTEXITCODE -ne 0) {
    throw "Command failed ($LASTEXITCODE): $FilePath $($Arguments -join ' ')"
  }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

New-Item -ItemType Directory -Force -Path $ArtifactsPath | Out-Null
New-Item -ItemType Directory -Force -Path $PackagesPath | Out-Null

Invoke-Step "docs governance" {
  Invoke-External python @("scripts/check-spec-links.py")
  Invoke-External python @("scripts/build-adr-index.py", "--check")
  Invoke-External python @("scripts/check-doc-governance.py")
}

Invoke-Step "backend guard scripts" {
  Invoke-External python @("scripts/check-no-console-write.py")
  Invoke-External python @("scripts/check-no-datetime-now.py")
  Invoke-External python @("scripts/check-test-endpoints.py")
}

if (-not $SkipRestore) {
  Invoke-Step "dotnet restore" {
    Invoke-External dotnet @(
      "restore", "ETOmniverse.sln",
      "--artifacts-path", $ArtifactsPath,
      "--packages", $PackagesPath,
      "/p:NuGetAudit=false"
    )
  }
}

if (-not $SkipBuild) {
  Invoke-Step "dotnet build" {
    Invoke-External dotnet @(
      "build", "ETOmniverse.sln",
      "--configuration", $Configuration,
      "--no-restore",
      "--artifacts-path", $ArtifactsPath,
      "/p:NuGetAudit=false",
      "-warnaserror"
    )
  }
}

if (-not $SkipTest) {
  Invoke-Step "dotnet test" {
    Invoke-External dotnet @(
      "test", "ETOmniverse.sln",
      "--configuration", $Configuration,
      "--no-build",
      "--artifacts-path", $ArtifactsPath,
      "/p:NuGetAudit=false"
    )
  }
}

if (-not $SkipFrontend) {
  Invoke-Step "frontend build" {
    Push-Location "src/frontend/ETOmniverse.Web"
    try {
      if (Test-Path "pnpm-lock.yaml") {
        Invoke-External pnpm @("install", "--frozen-lockfile")
      } else {
        Invoke-External pnpm @("install")
      }
      Invoke-External pnpm @("run", "build")
    } finally {
      Pop-Location
    }
  }
}

if (-not $SkipDocker) {
  Invoke-Step "docker compose config" {
    Invoke-External docker @(
      "compose",
      "--env-file", "docker/.env.example",
      "-f", "docker/docker-compose.yml",
      "config",
      "--quiet"
    )
  }
}

Write-Host "Verification complete."
