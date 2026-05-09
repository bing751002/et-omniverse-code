param(
  [Parameter(Mandatory = $true)]
  [string] $Name
)

dotnet ef migrations add $Name `
  --project src/backend/ETOmniverse.Infrastructure `
  --startup-project src/backend/ETOmniverse.Api `
  --output-dir Persistence/Migrations
