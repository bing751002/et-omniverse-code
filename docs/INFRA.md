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

## Observability Staging

| 階段 | 做法 |
|---|---|
| Day 1 | Serilog JSON + CorrelationId + console/file sink |
| P1.x | 加 Seq 或簡單 log sink，方便本機與 lab 查 log |
| P1.6+ | EFK / Fluent Bit；必要時再加 Prometheus/Grafana |

不把業務 metric（例如 VCR 用量、批次完成率）塞進 Prometheus；業務報表走 MSSQL。

## CI/CD Staging

- Day 1：先有 `dotnet build`、`dotnet test`、frontend build 的本機命令。
- Repo skeleton 穩定後：補 Jenkinsfile，只做 build/test/package。
- 有第一個可部署 demo 後：補 Docker image build + VM deploy + healthcheck。
- Production 前：才拆 CI/CD 權限、config bundle、rollback、雙階段 healthcheck。

FaceAI 的 Jenkins/Harbor/config-bundle/雙階段 healthcheck 是後期目標，不是 P1.0 開工門檻。
