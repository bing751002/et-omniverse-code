# ETOmniverse.Tools.ConfigTool

ConfigTool is the future command-line entry point for explicit environment operations such as development seed commands.

## Commands

```powershell
dotnet run --project src/backend/ETOmniverse.Tools.ConfigTool -- validate
dotnet run --project src/backend/ETOmniverse.Tools.ConfigTool -- print --redacted
```

`validate` checks non-business configuration required by the current foundation.
`print --redacted` prints the merged appsettings JSON with secret-like values masked.

F-005 boundary:

- Dev seed commands may live here when a real module needs local/demo data.
- Production migration data does not belong here unless a deployment runbook explicitly calls a one-off command and records the reason.
- Commands must be explicit and environment-gated; the API host must not run seed behavior implicitly on startup.
- No business seed data is implemented yet.
