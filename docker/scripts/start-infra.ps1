param(
  [switch]$Down,
  [switch]$Logs,
  [switch]$Status
)

$ErrorActionPreference = "Stop"
$compose = Join-Path $PSScriptRoot "..\docker-compose.infra.yml"

if ($Down) {
  docker compose -f $compose down
  exit $LASTEXITCODE
}

if ($Logs) {
  docker compose -f $compose logs -f
  exit $LASTEXITCODE
}

if ($Status) {
  docker compose -f $compose ps
  exit $LASTEXITCODE
}

docker compose -f $compose up -d
