---
id: F-005
title: Persistence foundation
module:
status: draft
owner: jimmyliao
created: 2026-05-09
updated: 2026-05-09
supersedes:
superseded-by:
related-adr: [D17]
related-interview: []
phase:
---

# F-005：Persistence foundation

## 業務背景

D17 已決定 Phase 1 transactional source of truth 是 MSSQL，EF Core 10 migrations 只管理 MSSQL schema。F-005 在業務 entity 出現前先建立 DbContext、naming convention、migration CLI、UoW / repository base、Testcontainers fixture 與 seed 邊界，避免第一個業務模組同時承擔「順便建立 persistence 基礎」造成 commit 混雜與 migration 序列衝突。

對應 7-step：與特定業務 step 無關，是所有需要資料庫的模組共用 persistence foundation。

## 用戶故事

1. As a backend developer adding the first business entity, I want a ready DbContext and migration convention, so that I only add module-specific model mapping instead of inventing database infrastructure.
2. As a reviewer checking schema changes, I want migrations to be generated from a fixed command and directory, so that migration history is reproducible and conflicts are easier to detect.
3. As a developer writing repository tests, I want a Testcontainers MSSQL fixture, so that repository behavior is verified against real MSSQL instead of an in-memory provider with different semantics.

## 範圍

### In scope

- **EtOmniverseDbContext skeleton**
  - 主檔：`Infrastructure/Persistence/EtOmniverseDbContext.cs`
  - partial class；未來每模組補 `EtOmniverseDbContext.<Module>.cs`
  - DbContext 不放 Domain 以外的業務流程邏輯
  - DbSet 先不為了測試建立 dummy business table
  - design-time factory 放 Infrastructure，讓 `dotnet ef` 不依賴本機啟動 API host 成功

- **EF Core 10 + MSSQL provider 註冊**
  - connection string 從 `ConnectionStrings:Default`
  - 禁寫死帳密 / host
  - `EnableRetryOnFailure()` 預設打開
  - ready health check 檢查 MSSQL connectivity
  - `MigrationsAssembly` 指向 Infrastructure project
  - local dev 沒 connection string 時 API startup fail fast，錯誤訊息不得輸出密碼

- **DB naming convention**
  - table / column 使用 snake_case
  - table 使用複數名稱
  - 優先用 EFCore.NamingConventions 類套件自動轉，不每張表手寫 `[Table]`
  - 對 EF migration history table 保持 EF 預設，不自訂成業務表命名規則

- **Baseline migration**
  - 建 `InitialBaseline` 空 migration，釘住 migration sequence 起點
  - 不建立 dummy `__version` table；EF 既有 `__EFMigrationsHistory` 足夠
  - migration 只放 Infrastructure project
  - 本 phase 只驗 migration 可產生 / build clean；不要求自動 `database update` 到開發者本機 MSSQL
  - local `database update` 指令寫入 docs / script，留給需要連 local compose DB 的開發者手動跑

- **Migration CLI 約定**
  - 指令：
    `dotnet ef migrations add <Name> --project src/backend/ETOmniverse.Infrastructure --startup-project src/backend/ETOmniverse.Api --output-dir Persistence/Migrations`
  - 寫入 `docs/CONVENTIONS.md` 或 `scripts/db-add-migration.ps1`
  - migration 前先 pull main / rebase，避免序列衝突

- **Repository base + Unit of Work**
  - Domain Ports：`IRepository<T>`、`IUnitOfWork`
  - Domain 補 `IAggregateRoot` marker interface；`IRepository<T>` 限制 `where T : class, IAggregateRoot`
  - Infrastructure 實作：`RepositoryBase<T>`、`UnitOfWork`
  - `SaveChangesAsync` 統一從 UoW 走
  - repository base 只放真正共用的低階方法：`AddAsync` / `GetByIdAsync` / `ListAsync` / `Remove`
  - 不在 base repository 塞 include graph、pagination、business query
  - query/read repository 若回傳 read model，必須命名為 Query/Read 並在 feature spec 說明 CQRS exception

- **Testcontainers MSSQL fixture**
  - 建 integration test base class / fixture
  - repository tests 使用真 MSSQL container
  - local 沒 Docker 時測試需清楚 skip 或報出可理解錯誤，不假綠
  - fixture 每個 test collection 使用獨立 database name，避免測試互相污染
  - migrations 在 fixture 啟動後套用到 container DB，再執行 repository tests

- **Data seed 入口**
  - 釐清 `ETOmniverse.Tools.ConfigTool` 的責任
  - dev seed 可由 tool 或專用 command 觸發
  - prod migration data 不能混在 dev seed；需 migration / runbook 明確紀錄
  - 本 phase 只建立 README / command skeleton，不寫任何業務 seed data

### Out of scope

- 業務 entity / business table
- Outbox / inbox pattern
- 多 DB provider 抽象
- read replica / CQRS DB 分離
- Vector DB / Qdrant
- Soft delete global filter
- audit log writer
- production backup / restore runbook
- 實作任何 Identity / Batch / AiVcr repository
- 直接套用 migration 到 staging / production

## 驗收條件

- [ ] **AC-1 DbContext skeleton**：`EtOmniverseDbContext` 存在且為 partial class；未來模組 mapping 可拆 partial 檔 — 對應測試：unit / code review
- [ ] **AC-2 SQL Server registration**：Infrastructure DI 從 `ConnectionStrings:Default` 註冊 EF Core SQL Server provider，且啟用 transient retry — 對應測試：unit / integration
- [ ] **AC-3 DB health check**：`/health/ready` 會檢查 MSSQL connectivity — 對應測試：api / integration
- [ ] **AC-4 naming convention**：測試 aggregate 的 table / column mapping 為 snake_case，且 table 為複數；migration history table 不被自訂命名 — 對應測試：unit
- [ ] **AC-5 baseline migration**：存在 `InitialBaseline` migration，內容不建立 dummy table，只建立 EF migration 起點；本 phase 不自動 update 本機 DB — 對應測試：manual review / migration smoke
- [ ] **AC-6 migration CLI**：文件或 script 提供固定 `dotnet ef migrations add` 指令 — 對應測試：manual review
- [ ] **AC-7 UoW / repository base**：Domain 有 `IAggregateRoot` / repository / UoW ports，Infrastructure 有實作，`SaveChangesAsync` 經 UoW — 對應測試：unit
- [ ] **AC-8 Testcontainers fixture**：一個 sample aggregate repository integration test 可在 MSSQL container 上 migrate + CRUD 跑通 — 對應測試：integration
- [ ] **AC-9 seed boundary**：ConfigTool README 或 docs 說清 dev seed vs prod migration data 邊界 — 對應測試：manual review
- [ ] **AC-10 build clean**：`dotnet build` / `dotnet test` 通過 — 對應測試：build smoke

## 實作連結（完工後填）

- DbContext：`<src/backend/ETOmniverse.Infrastructure/Persistence/EtOmniverseDbContext.cs>`
- Design-time factory：`<src/backend/ETOmniverse.Infrastructure/Persistence/EtOmniverseDbContextFactory.cs>`
- DI registration：`<src/backend/ETOmniverse.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs>`
- Migrations：`<src/backend/ETOmniverse.Infrastructure/Persistence/Migrations/>`
- Aggregate marker：`<src/backend/ETOmniverse.Domain/Common/Entity/IAggregateRoot.cs>`
- UoW port：`<src/backend/ETOmniverse.Domain/Common/Ports/IUnitOfWork.cs>`
- Repository port：`<src/backend/ETOmniverse.Domain/Common/Ports/IRepository.cs>`
- UoW implementation：`<src/backend/ETOmniverse.Infrastructure/Persistence/UnitOfWork.cs>`
- Repository base：`<src/backend/ETOmniverse.Infrastructure/Persistence/RepositoryBase.cs>`
- Testcontainers fixture：`<tests/backend/ETOmniverse.Infrastructure.Tests/Integration/MsSqlFixture.cs>`
- Migration script：`<scripts/db-add-migration.ps1>`
- Seed boundary docs：`<src/backend/ETOmniverse.Infrastructure/Seed/README.md>` / `<src/backend/ETOmniverse.Tools.ConfigTool/README.md>`
- 主要 PR：#TBD

## 依賴決策（NuGet）

| 套件 | 用途 | 為什麼必要 | 替代方案評估 |
|---|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | MSSQL EF provider | D17 鎖 MSSQL + EF Core 10 | Dapper / MySQL / PostgreSQL 都與決策衝突 |
| `Microsoft.EntityFrameworkCore.Design` | migration CLI | `dotnet ef migrations add` 需要 design-time services | 手寫 migration 不可維護 |
| `EFCore.NamingConventions` | snake_case mapping | CONVENTIONS 要求 snake_case，避免每張表手寫 mapping | 手寫 `[Table]` / Fluent API 易漏且增加衝突 |
| `Testcontainers.MsSql` | MSSQL integration test fixture | CONVENTIONS 指定 integration 用 Testcontainers MSSQL | InMemory provider 與 MSSQL 語意不同，不適合 repository 驗證 |

## Open questions

- [x] Q-F005-001: `IRepository<T>` 是否要求 aggregate root marker interface，或先以 `class` entity 約束即可？— **Resolved 2026-05-09**：新增 `IAggregateRoot` marker，`IRepository<T>` 限制 `where T : class, IAggregateRoot`。理由：避免 repository base 被拿去包 value object / read model；query/read model 需走 Query/Read repository 並在 feature spec 說明。
- [x] Q-F005-002: baseline migration 是否要在本 phase 執行 `database update` 到 local compose MSSQL，或只驗 migration 可產生 / build clean？— **Resolved 2026-05-09**：本 phase 只要求 migration 可產生、build/test clean、Testcontainers fixture 能 migrate container DB；不要求自動 update 開發者本機 compose DB。理由：避免把個人 local DB 狀態變成驗收前提。

## 變更記錄

| 日期 | 變更 | PR |
|---|---|---|
| 2026-05-09 | 初版 (status: draft) | #TBD |
