# Infra

> 參考 `D:\FaceAI` 的成熟作法，但 ET-Omniverse 先採「可漸進擴張」版本；Day 1 不一次搬完整監控、CI/CD、LocalHybrid。

## 原則

- infra 規則屬於 repo 共享契約，不放在個人 dogfood 流程。
- Docker Compose 分成「基礎服務」與「應用服務」，避免 app 改動牽動資料庫、log、監控。
- 環境差異用 overlay / appsettings / env 表達，不在 code 寫死路徑、帳密、host。
- Day 1 只落地 MSSQL + API + Web 可跑通；EFK、Prometheus/Grafana、Jenkins deploy script 分階段補。Qdrant 暫不啟用。

## Docker Compose 形狀

規劃結構：

```text
docker/
  docker-compose.infra.yml        # mssql / redis / seq；獨立啟停
  docker-compose.yml              # local app stack: api + web
  docker-compose.local.yml        # local override
  compose/
    base.api.yml                  # API 共用設定、healthcheck、env defaults
    base.web.yml                  # Vue build 後 nginx static
  scripts/
    start-infra.ps1               # 啟停 infra、status、logs
```

搬 FaceAI 的精神，不搬它的全部重量：

- 採用 base compose + overlay，讓 `api` / `web` 的 healthcheck、restart policy、env defaults 有單一來源。
- infra compose 使用固定 network / volume 名稱，降低 Visual Studio 或 compose project name 造成的混亂。
- Day 1 不需要 LocalHybrid；如果未來要連公司 lab DB/NAS，再另外開 `docker/localhybrid/README.md` 與 setup/cleanup script。

## 環境設定順序

.NET API 設定覆蓋順序固定如下：

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. `appsettings.{Environment}.Ops.json`（不進 git，部署/營運提供）
4. environment variables
5. user-secrets（只限本機開發）

規則：

- 新增 config key 必須同步文件，至少說明用途、預設值、是否 secret、影響服務。
- Secret、connection string、API key 不進 repo。
- `appsettings.*.Ops.json` 只放部署差異，不放 feature 行為邏輯。

### External service config

*F-004 落地後生效（per `docs/specs/F-004-http-outbound-base.md`）。*

`ExternalServices:*` 是 outbound HTTP typed client 的設定根節點。repo 內只允許 schema / placeholder；真實 endpoint、API key、token 一律走 `appsettings.{Environment}.Ops.json`、environment variables 或 user-secrets。

| Key | 用途 | 預設值 | Secret | 影響 |
|---|---|---|---|---|
| `ExternalServices:SampleEcho:BaseUrl` | Sample typed client 測試 pipeline 用 base URL | `http://localhost` placeholder | 否（真實服務 URL 視環境可能敏感，Ops 覆蓋） | typed client startup validation / BaseAddress |
| `ExternalServices:*:TimeoutSeconds` | outbound timeout | `10` | 否 | 超時後回 `Result<T>` failure |
| `ExternalServices:*:Retry:MaxAttempts` | transient retry 次數 | `3` | 否 | 只 retry timeout、5xx、408、429 |
| `ExternalServices:*:Retry:BaseDelayMs` | exponential backoff 起始延遲 | `200` | 否 | retry 間隔 |
| `ExternalServices:*:CircuitBreaker:Enabled` | circuit breaker 開關預留 | `false` | 否 | Day 1 不啟用 |

Day 1 implementation note：原 spec 優先採 `Microsoft.Extensions.Http.Resilience`；目前本機 package/cache 不存在且 restore 受限，因此 F-004 先用共用 `ResilientHttpClientHandler` 實作同等 timeout/retry 行為。未來若引入官方 resilience pipeline，必須保留既有測試語意。

## 環境矩陣

| 環境 | API Environment | Primary DB | Vector DB | Redis/Queue | Web | 用途 |
|---|---|---|---|---|---|---|
| Local | `Development` | local MSSQL compose | 暫不啟用 | Phase 1 暫不啟用 | Vite dev server | 個人開發 |
| LocalDocker | `Development` | compose MSSQL | 暫不啟用 | Phase 1 暫不啟用 | nginx static 或 Vite | 驗證 container |
| Staging/Lab | `Staging` | 公司 VM / lab MSSQL | 暫不啟用 | 視 phase 啟用 | nginx static | UAT / demo |
| Production | `Production` | on-prem MSSQL | 暫不啟用 | 視 phase 啟用 | nginx static | 正式使用 |

## DB 邊界

- MSSQL 是 transactional source of truth，EF Core 10 migrations 只管理 MSSQL schema。
- Qdrant 暫不啟用；Phase 2/RAG 需求確認後再新增 vector DB adapter、compose service、snapshot runbook。
- local infra 至少要有 `mssql-data`、`redis-data`、`seq-data` 三個 volume。

### MSSQL / EF Core config

*F-005 落地後生效（per `docs/specs/F-005-persistence-foundation.md`）。*

| Key | 用途 | Repo 預設值 | Secret | 影響 |
|---|---|---|---|---|
| `ConnectionStrings:Default` | API transactional MSSQL connection string | local placeholder with trusted connection | 是（真實帳密 / host 不進 repo） | API startup、EF Core DbContext、ready health check、migration CLI |

規則：

- 真實 lab / staging / production connection string 只能放 `appsettings.{Environment}.Ops.json`、environment variables 或 user-secrets。
- API startup 會在 `ConnectionStrings:Default` 缺失時 fail fast；錯誤訊息不得輸出密碼。
- EF Core migration assembly 固定為 `ETOmniverse.Infrastructure`。
- 本 phase 不自動對開發者 local DB 執行 `database update`；需要本機套 migration 時，由開發者明確執行 `dotnet ef database update`。

## Observability Staging

| 階段 | 做法 |
|---|---|
| Day 1 | Serilog JSON + CorrelationId + console sink only + docker json-file rotation |
| P1.x | 加 Seq 或簡單 log sink，方便本機與 lab 查 log |
| P1.6+ | EFK / Fluent Bit；必要時再加 Prometheus/Grafana |

不把業務 metric（例如 VCR 用量、批次完成率）塞進 Prometheus；業務報表走 MSSQL。

### Day 1 log retention policy

*F-002 落地後生效（per `docs/specs/F-002-backend-logging-foundation.md`）。*

**App 端**：
- 只寫 console JSON sink（CLEF / `RenderedCompactJsonFormatter`）
- **不**做 in-app file sink — 避免 fluent-bit 上線後雙寫、host 權限糾纏
- 啟動異常透過 `BootstrapLogger` 落 stderr，不靜默

**Compose 端**：
- 對 `api` / `web` service 設 `logging.driver: json-file` + `options.max-size: 50m` + `options.max-file: "5"`
- 設定位置：`docker/compose/base.api.yml` 與 `docker/compose/base.web.yml`（overlay；root `docker/docker-compose.yml` 用 `include` 串）
- 避免 container log 撐爆 host 磁碟

**限制聲明（重要）**：
- docker `json-file` rotation 是 **size-based 不是 time-based**
- **不保證**「保留一週」或任何時間級別追查能力（log 量大可能幾小時就轉走）
- **staging / lab 若需固定保留 N 天日誌，P1.x 必須提前上 Seq 或其他 log collector**
- 不能假裝 docker rotation 撐得到 P1.6 EFK 才上來

**Seq schema hook**：
- `docker/docker-compose.infra.yml` 的 `seq` service 保留
- API 端 Day 1 不掛 Seq sink
- P1.x 開啟時改 `appsettings` 即可（schema 已預留）

## CI/CD Staging

- Day 1：先有 `dotnet build`、`dotnet test`、frontend build 的本機命令。
- Frontend container build 必須使用 repo 鎖定的 pnpm toolchain；`docker/Dockerfile.frontend` 以 `corepack enable` + `pnpm install` + `pnpm run build` 為準，不回退到 npm。
- v1.1 起本機與 CI 共同入口為：
  - Local: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\verify-local.ps1`
  - CI: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\verify-ci.ps1 -Configuration Release`
- Verification scripts 使用 temp artifacts 目錄（預設 `$env:TEMP\et-omniverse-artifacts`），避免 Windows checkout 內 `obj` / artifacts ACL 問題污染結果。
- NuGet packages 預設使用 developer/CI 的 global cache；只有在明確傳入 `-PackagesPath` 時才覆蓋。離線環境不可預設指向空 packages 目錄，否則會嘗試連到 nuget.org 並讓 restore gate 失敗。
- `dotnet restore` / `dotnet build` / `dotnet test` 在 verification scripts 中固定使用 `/m:1`；目前 .NET 10 SDK 在此 checkout 的 parallel MSBuild graph 會出現 0 errors 但 exit 1 的失敗，單節點執行是已驗證 workaround。
- Frontend verification 會把 `src/frontend/ETOmniverse.Web` 複製到 temp artifacts 目錄後執行 `pnpm install` / `pnpm run build`，避免 Windows checkout ACL 造成 pnpm `_tmp_*` unlink 失敗。
- NuGet audit 在 verification scripts 中以 `/p:NuGetAudit=false` 關閉；安全稽核應由可連 registry 的獨立 CI job 處理，不能讓 registry/network 抖動阻塞一般 build/test gate。
- Config validation 入口為 `dotnet run --project src/backend/ETOmniverse.Tools.ConfigTool -- validate`；需要檢視設定時用 `print --redacted`，不可把 secret-like 值輸出到 CI log。
- Repo skeleton 穩定後：補 Jenkinsfile，只做 build/test/package。
- 有第一個可部署 demo 後：補 Docker image build + VM deploy + healthcheck。
- Production 前：才拆 CI/CD 權限、config bundle、rollback、雙階段 healthcheck。

FaceAI 的 Jenkins/Harbor/config-bundle/雙階段 healthcheck 是後期目標，不是 P1.0 開工門檻。
