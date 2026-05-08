# Codebase Concerns

**Analysis Date:** 2026-05-08

## Overview

This is a **greenfield Phase 1.0 foundation** repository (bootstrapped 2026-05-03). No production business logic exists yet. Most concerns center on **structural governance gaps** and **test/spec infrastructure** that must mature before feature shipping, plus **known Phase 1 scope boundaries** documented in decisions.

---

## Test Infrastructure Gaps

**Placeholder Tests Block Feature Completion:**

- **Files:** `tests/backend/ETOmniverse.Domain.Tests/UnitTest1.cs`, `tests/backend/ETOmniverse.Api.Tests/UnitTest1.cs`
- **Issue:** Both contain empty `Test1()` Fact methods. Per `docs/CONVENTIONS.md` line 63, "沒測試不算完成"—features cannot be shipped without corresponding unit/integration/API tests.
- **Impact:** Feature approval workflows will require replacement before PR merge.
- **Fix approach:** Remove placeholder files during first feature implementation. Each feature must follow test patterns from `docs/CONVENTIONS.md` (xUnit unit tests, Testcontainers integration tests, WebApplicationFactory API tests). GSD `/gsd:execute-phase` will guide test scaffold creation.

**Missing Test Fixtures & Factories:**

- **Issue:** No test data patterns exist yet (no factories, builders, or fixture directories).
- **Files affected:** Will impact all integration/API tests once implemented.
- **Fix approach:** After first vertical slice (likely F-014 NotAvailable placeholder), extract factory pattern into `docs/patterns/testing-fixtures.md` per `docs/WORKFLOW.md` line 120-121.

---

## Governance Gaps (Reviewer Discipline Required)

**Access Control Enforcement Lacks Automation:**

- **Issue:** `scripts/check-doc-governance.py` (lines 34–63) has three rules: source-code-requires-spec-or-kb, infra-requires-infra-doc, adr-summary-sync. **No rule enforces `docs/ACCESS-CONTROL.md` updates when permission logic changes.**
- **Why it matters:** `docs/ACCESS-CONTROL.md` documents the RBAC + org-scope model (D18). Changes to auth checks, role assignments, or scope grants must sync back to that file.
- **Current mitigation:** Relies on reviewer discipline + DOCUMENTATION.md checklist (line 151 requires reviewer to catch permission changes). **Not automated.**
- **Files involved:** `scripts/check-doc-governance.py`, pre-commit hook `.githooks/pre-commit`
- **Fix approach:** Add a fourth rule `access-control-requires-doc` that flags changes to files matching `src/*/Identity/*` or `*Auth*` and requires corresponding `docs/ACCESS-CONTROL.md` update or `no-doc-update-` rationale with `allow_rationale: False` (like the adr-summary-sync rule). Requires code review of permission patterns first (Phase 1.0a) before automation is safe.

**Manual F-XXX Numbering (No Automation):**

- **Issue:** Spec numbering in `docs/specs/` is **manual**. No script auto-assigns `F-XXX` IDs. `docs/specs/README.md` line 10 says "先 `grep -r "^id: F-" .` 取最大值 +1" — human responsibility.
- **Risk:** Merge conflicts, duplicate IDs, off-by-one errors when multiple people work simultaneously.
- **Files:** `docs/specs/README.md` documents the rule; actual specs will be in `docs/specs/F-*.md`.
- **Fix approach:** Create `scripts/assign-spec-id.py` (similar to `build-adr-index.py`) that scans existing specs, finds max F-XXX, and either (a) auto-assigns the next ID to a `_template.md` as user creates new spec, or (b) validates during pre-commit that no ID duplicates exist. Worth doing before 5+ concurrent features start.

---

## Known Phase 1 Scope Boundaries (Documented Risks)

**Identity / Auth Implementation Incomplete:**

- **Decision:** D14 (2026-05-07) defers AD/LDAP to Phase 2; Phase 1 uses "local user store + RBAC".
- **Files:** `docs/DECISIONS.md` lines 23-24, `docs/AI-GUIDE.md` line 78 (red line: "Phase 1 引入 Fugo / AD code").
- **Impact:** `docs/ACCESS-CONTROL.md` describes the **intended** RBAC + OrgUnit tree model, but Phase 1 implementation must **not** build the full tree—only foundation (functional RBAC + org_unit_id scope). Full OrgUnit hierarchy / nested permissions deferred.
- **Current state:** No code yet; spec will need to clarify what subset is built Phase 1.
- **Risk if violated:** Scope creep into Phase 2 work.
- **Mitigation:** `docs/CONVENTIONS.md` lines 79-80 flag AD scope creep as a known trap. GSD specs will have clear `out-of-scope` sections.

**Background Jobs Tightly Coupled to API Host:**

- **Decision:** D16 (2026-05-07): Background tasks run **inside API process** via IHostedService + Quartz, no separate Worker.
- **Files:** `docs/ARCHITECTURE.md` line 5, `docs/DECISIONS.md` line 26.
- **Issue:** Single-process deployment limits scalability. If API crashes, background jobs stop.
- **Current code:** Not yet implemented (skeleton only). `ETOmniverse.Api/Program.cs` will register Quartz services.
- **Impact:** Phase 1 acceptable (50 users, on-prem single VM). **Will need refactor to separate Worker for Phase 2 scaling.**
- **Fix approach:** Document in Retrospective when Phase 2 planning starts. No action needed Phase 1, but implementation code should use interfaces/abstraction (Ports pattern) so Worker split is not a breaking change.

**Database Locked to MSSQL Phase 1 Only:**

- **Decision:** D17 (2026-05-08): Phase 1 uses MSSQL; Qdrant **deferred** pending RAG requirements.
- **Files:** `docs/DECISIONS.md` line 27, `docs/AI-GUIDE.md` lines 38-39.
- **Risk:** Hard dependency on MSSQL tooling (EF Core, migrations, SQL Server). If Phase 2 needs vector search, Qdrant integration will require new parallel data flow.
- **Mitigation:** Keep domain layer database-agnostic (already enforced via Ports interface, `docs/ARCHITECTURE.md` line 95). Use read models / projections for complex queries (not raw SQL).
- **Current code:** EF Core DbContext partial-per-module already in place (`docs/ARCHITECTURE.md` line 9). No concerns yet.

**AI VCR Multi-Engine Support Must Not Be Hardcoded:**

- **Issue:** AiVcr module will integrate 4 engines (sora2, kling3, seedance2, wan27) + Gemini voice. Easy to write `if (engine == "sora2")` and ship it.
- **Files:** `docs/CONVENTIONS.md` line 81 flags this as a known trap: "AI VCR 直接寫死支數 / engine".
- **Risk:** If engine logic is hardcoded, adding/swapping engines requires code change + redeploy instead of config-driven.
- **Impact:** Blocks rapid experimentation with new engines, delays iteration cycles.
- **Fix approach:** When AiVcr feature spec is written, require **engine registry / strategy pattern** in design. Use enum or string registry, not hardcoded conditions. Validate in code review against `docs/CONVENTIONS.md` trap list.

**Audience Data Structure Unconfirmed (Q-AU-001):**

- **Issue:** `docs/DECISIONS.md` line 36-37 lists **Q-AU-001**: "受眾契約格式" blocked decision. Step 2 Audience module schema is TBD pending stakeholder interview.
- **Files:** `docs/CONVENTIONS.md` line 82 flags: "受眾欄位（Q-AU-001 未解）寫死 schema".
- **Risk:** If Audience schema is guessed and committed, interview results will require migration + rework.
- **Current state:** No code yet. Placeholder entity can exist, but fields must be marked `OPEN` or use flexible map/JSON until Q-AU-001 is resolved.
- **Fix approach:** Interview must happen before `docs/specs/F-0XX-audience-*.md` is approved. Use `status: blocked` state in spec until answer received. `docs/WORKFLOW.md` line 91 requires spec status update when interview unblocks Q.

**Schedule Conflict Rules Unconfirmed (Q-SCH-001):**

- **Issue:** `docs/DECISIONS.md` line 36-37 also lists **Q-SCH-001**: "排播異動規則 / 通知對象" blocked decision for Step 5 Schedule module.
- **Files:** `docs/CONVENTIONS.md` line 83 flags: "排播衝突規則（Q-SCH-001 未解）寫死邏輯".
- **Risk:** Business rules around schedule conflicts (overlaps, priority, notification targets) are under-specified. Guessing will require rework.
- **Current state:** No code yet.
- **Fix approach:** Same as Q-AU-001: keep Q-SCH-001 interview in backlog, spec status: blocked until resolved.

---

## Missing Long-Term Infrastructure Scaffolding

**Observability: Distributed Tracing Not Planned Phase 1:**

- **Issue:** `docs/ARCHITECTURE.md` line 8: "**不做 distributed tracing**". Single Serilog JSON + CorrelationId only.
- **Risk:** When feature complexity grows or cross-module debugging needed, lack of trace context will slow investigation.
- **Impact:** Phase 1 acceptable (simple flow). **Will need OpenTelemetry or similar Phase 1.6+ (observability stage).**
- **Mitigation:** Pre-stage with proper CorrelationId propagation now (headers, async context) so switching to tracing later is localized config, not code refactor.
- **Current code:** Result.cs, clock abstraction in place. No observability instrumentation yet.

**Monitoring & EFK Deferred:**

- **Issue:** `docs/ARCHITECTURE.md` line 8: Elasticsearch + Kibana are "P1.6" (phase 1.6), not P1.0.
- **Current state:** No EFK stack running locally. Logs go to console only (Serilog stdout).
- **Risk:** P1.0 features deployed without full observability; operator has no dashboard to detect issues.
- **Mitigation:** Clear; this is a known staged rollout per `docs/ARCHITECTURE.md`. Jenkins will skip EFK integration until P1.6.

---

## Testing Pattern Fragility

**WebApplicationFactory Setup May Require Per-Feature Customization:**

- **Issue:** `docs/CONVENTIONS.md` line 60 requires WebApplicationFactory for API-layer tests. No starter template or base class exists yet.
- **Risk:** First team member to write API test will establish pattern (good or bad). If pattern is brittle (e.g., swaps entire DbContext, doesn't run migrations), second feature's test becomes hard to maintain.
- **Fix approach:** After first vertical slice ships (F-014 NotAvailable), extract WebApplicationFactory base class and test template into `docs/patterns/api-test-fixture.md` with concrete example from repo code.

**Integration Test DB Spin-Up Time Unknown:**

- **Issue:** Tests use "Testcontainers MSSQL" (line 59). First run will download container image; subsequent runs spin new DB per test class (slow).
- **Risk:** Test suite runtime could become prohibitive if not optimized early (shared container, test database strategy).
- **Impact:** If test feedback loop grows > 2–3 min, developers will skip local tests before push.
- **Fix approach:** Measure test runtime after first 5–10 integration tests written. If > 30s, implement: (a) single shared container for test session, (b) database snapshot/restore between tests, or (c) in-memory SQL (SQLite with EF Core fallback). Document choice in retrospective.

---

## Governance Reliance on Reviewer Discipline

**Pre-Commit Hooks Wired, But No Integration Tests for Hooks Themselves:**

- **Files:** `.githooks/pre-commit` (lines 17-34) runs `build-adr-index.py`, `check-spec-links.py`, `check-doc-governance.py`.
- **Issue:** If governance script breaks (e.g., regex bug in spec link check), developers can't commit. No fallback or bypass documented.
- **Mitigation from DOCUMENTATION.md:** Line 180-181 mentions `docs/no-doc-update-*.md` as rationale escape hatch, but **only for content changes that truly don't need docs**, not for hook infrastructure failures.
- **Risk:** If hook fails on bogus input, developer has no clear recovery path. Will be blamed on "docs governance is too strict."
- **Fix approach:** Document in `docs/WORKFLOW.md` under a new "If Hooks Break" section: (a) which hooks do what, (b) how to bypass temporarily with `git commit --no-verify` for emergency, (c) how to report hook failures. Wire pre-commit test into CI: run hooks on sample changes as part of setup validation.

**No Automated Enforcement of Spec Status Transitions:**

- **Issue:** Spec frontmatter `status` field has lifecycle (draft → blocked → approved → implementing → implemented). `docs/specs/README.md` line 14-22 documents transitions, but nothing prevents `status: implemented` without passing tests or spec links being valid.
- **Risk:** Spec status field becomes documentation only, not a reliable gate.
- **Fix approach:** Add rule to `check-spec-links.py` that validates status-to-file-state consistency: e.g., if status is `implemented`, spec must have "실작 連結" section with non-empty endpoint/usecase/PR fields. Warn (not error) if inconsistent—allow reviewer override via `no-doc-update-*.md`.

---

## Dependency Management Gaps

**No Package Lock Version Enforcement:**

- **Issue:** `.NET`: Directory.Build.props controls version matrices, but no lock file mechanism (unlike npm pnpm-lock.yaml).
- **Risk:** Different developers, different build times, subtly different transitive dependency versions. CI builds may not match local.
- **Mitigation:** Directory.Build.props pin major.minor (e.g., `17.14.*` for Test.Sdk) so patch updates are controlled.
- **Current state:** Good baseline established. No action needed Phase 1.

**Frontend Package Manager Locked to pnpm:**

- **Issue:** `docs/CONVENTIONS.md` line 26 mandates pnpm (not npm). Verified in `docs/ARCHITECTURE.md` line 6.
- **Risk:** If developer uses npm install instead, lockfile gets committed and breaks others.
- **Mitigation:** Document in `docs/ONBOARDING.md` (exists, but verify content). Add pre-commit hook to reject `package-lock.json` commits.
- **Current state:** `docs/ONBOARDING.md` likely covers this. Worth spot-checking.

---

## Documentation Entropy Risks

**Specs Directory Structure Unplanned:**

- **Issue:** `docs/specs/README.md` lines 3-5: "**模組子目錄暫未劃分** — 先全部平鋪在 `specs/` 下. 等 spec 累積到一定數量…再決定怎麼分".
- **Risk:** Once 30+ specs exist, flat directory becomes hard to navigate. But splitting into subdirs requires bulk rename + link updates.
- **Impact:** Not a blocker, but will cause friction around F-050+.
- **Fix approach:** Document in retrospective around F-020 milestone: "time to organize specs by module / 7-step phase / business domain?" + decision. No action needed now.

**No Central KB Index Yet:**

- **Files:** `docs/INDEX.md` exists (confirmed by ls output) but content unknown.
- **Issue:** If INDEX.md is incomplete or not linked from README.md, newcomers won't find the KB.
- **Fix approach:** Verify INDEX.md is comprehensive and README.md points to it. Should be done during Phase 1.0a onboarding pass.

**AI-GUIDE Prescriptive Decisions Can Drift from Practice:**

- **Files:** `docs/AI-GUIDE.md` documents red lines, stop-asks, and credibility tiers. This is **prescriptive** (how to use AI), not **descriptive** (what AI has done).
- **Risk:** Once code is written, AI-GUIDE rules (e.g., "Entity/DTO/migration = grass draft + human audit") can become aspirational rather than actual practice.
- **Impact:** Future AI agents will inherit stale rules.
- **Fix approach:** After first 3–5 features, retrospective should review which AI-GUIDE rules were followed and which were bent. Update AI-GUIDE with actual practice + document exceptions.

---

## Technical Debt Tracking (None Yet)

**No Retrospectives Directory Populated:**

- **Files:** `docs/retrospectives/` directory exists but is empty.
- **Issue:** First post-mortems, AI failure modes,踩坑 will go here. Structure not yet tested.
- **Risk:** If retrospectives are written ad-hoc without consistent format, they become hard to search / summarize.
- **Fix approach:** Create `docs/retrospectives/_template.md` with sections: [Date] [Incident/Lesson] [Root Cause] [Action Items] [Status]. Use in first bug/incident.

---

## Summary Table

| Category | Issue | Severity | Trigger | Owner |
|----------|-------|----------|---------|-------|
| Test infrastructure | Empty UnitTest1.cs placeholders | High | First feature approval | Dev + Code Review |
| Governance | No ACCESS-CONTROL.md automation in pre-commit | Medium | First identity feature | DevOps + Lead |
| Governance | Manual F-XXX numbering (merge conflict risk) | Medium | 3+ concurrent specs | Tooling |
| Scope boundary | Q-AU-001 audience schema unresolved | High | Before Audience impl | PM + Stakeholder |
| Scope boundary | Q-SCH-001 schedule rules unresolved | High | Before Schedule impl | PM + Stakeholder |
| Scope boundary | AiVcr engine hardcoding trap | Medium | During AiVcr code review | Code Review |
| Infrastructure | Distributed tracing deferred (Phase 1.6) | Low | Monitoring rollout | Infra |
| Testing | WebApplicationFactory pattern not yet established | Medium | After first API test | Dev + Code Review |
| Testing | Integration test DB performance unknown | Medium | After 5+ integration tests | Perf testing |
| Process | Pre-commit hook failure recovery not documented | Low | First hook failure | Docs |
| Process | Spec status transitions not machine-validated | Low | Spec governance tightening | Tooling |
| Docs | Specs flat directory scale (FYI for F-050+) | Low | 30+ specs reached | IA |
| Docs | KB INDEX.md completeness unknown | Low | Onboarding review | Docs |

---

*Concerns audit: 2026-05-08*
