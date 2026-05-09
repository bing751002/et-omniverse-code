# Seed

Development seed runners and seed data live here.

Seed code must be explicit and environment-gated.

Boundary:

- Dev seed is allowed to create demo/local data for developer environments only.
- Prod migration data must be represented by an EF migration or an explicit deployment runbook, not by dev seed code.
- Seed commands must require an explicit environment/command flag. Startup must not silently seed data.
- No business seed data is implemented in F-005; this folder only defines the entry point and boundary.
