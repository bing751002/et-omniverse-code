# Decisions

> 重大決策摘要。詳細 ADR 在 [`decisions/D-*.md`](decisions/)。
>
> **ADR Summary 區塊由 `scripts/build-adr-index.py` 自動產出**，pre-commit hook 自動重寫，不要手改。

## ADR Summary

<!-- BEGIN AUTO-GENERATED ADR INDEX -->
| ID | 日期 | 狀態 | 決策 | 鏈結 |
|---|---|---|---|---|
| [D-19](decisions/D-19-test-mode-auth-bypass.md) | 2026-05-09 | accepted | Test-mode authentication bypass via env-guarded TestAuthenticationHandler | — |
| [D-20](decisions/D-20-timeprovider-mandatory.md) | 2026-05-09 | accepted | TimeProvider mandatory for all time-dependent code; ban DateTime.Now / DateTime.UtcNow | — |
| [D-21](decisions/D-21-testcontainers-respawn.md) | 2026-05-09 | accepted | Testcontainers MSSQL + Respawn for integration / E2E test data lifecycle | — |
| [D-22](decisions/D-22-test-endpoints-namespace.md) | 2026-05-09 | accepted | Test-only endpoints under /api/test/* namespace with startup hard-fail in Production | — |
<!-- END AUTO-GENERATED ADR INDEX -->

## 規劃期決策（待搬入 `docs/decisions/`）

> 以下 D10-D18 暫存於此，逐一搬入對應 `docs/decisions/D-XX-*.md` 後**從本表移除**（搬完後此整段刪除）。

| ID | 日期 | 決策 |
|---|---|---|
| **D10** | 2026-05-03 | Phase 1 直接做 7-step 新流程 MVP（不走「舊結構 + Phase 2 大改」） |
| **D11** | 2026-05-03 | 分眾改大數據算（廢除節目部填分眾） |
| **D12** | 2026-05-03 | 取消「大數據發送」獨立 step → 合併到 Step 6（後段角色從 3 → 2） |
| **D13** | 2026-05-03 | VCR Studio 設計有外部參考來源（使用者提供） |
| **D14** | 2026-05-07 | Phase 1 用 local user store + RBAC，**不做 AD/LDAP**（推 Phase 2） |
| **D15** | 2026-05-07 | **demo-first 策略**：先做空殼骨架 + placeholder，再依 AI VCR ★ → 主流程 → 共編順序填內容 |
| **D16** | 2026-05-07 | 背景任務跑在 **API host 內**（IHostedService + Quartz），不拆獨立 Worker |
| **D17** | 2026-05-08 | Phase 1 DB 配置改為 **MSSQL**：MSSQL 是 transactional source of truth；Qdrant 暫不啟用，等 Phase 2/RAG 需求確認 |
| **D18** | 2026-05-08 | 權限模型 Phase 1 採 **功能面 RBAC + 事業群 scope**，預留 OrgUnit tree / scoped RoleAssignment / grants，細粒度權限用 feature flag 漸進啟用 |

## 規則

- 已 accepted 的不改寫；要變更開新 ADR 並標 `supersedes`
- 改決策前先看是否跟既有 D 衝突
- 新增決策編號續 **D19** 起
- 寫完 ADR 不需手動同步本檔上方 ADR Summary — pre-commit hook 自動跑 `scripts/build-adr-index.py`

## 訪談卡點

P0 議題未確認前**不要寫實作 spec**，碰到的功能用 placeholder 撐：

| 議題 | 影響 |
|---|---|
| Q-AU-001 受眾契約格式 | Step 2、SMS dispatch、資料模型 |
| Q-SCH-001 排播異動規則 / 通知對象 | Step 5、Notification |
