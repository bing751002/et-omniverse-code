$ErrorActionPreference = "Stop"

dotnet tool run dotnet-ef --version | Out-Null

if ($LASTEXITCODE -ne 0) {
  throw "dotnet-ef is not restored. Run: dotnet tool restore"
}
