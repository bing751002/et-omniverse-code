# Architecture

## 技術棧（鎖定）

- **後端**：.NET 10 + EF Core 10 + MSSQL + Quartz.NET（背景任務跑在 API host 內，**沒有獨立 Worker**；Qdrant 暫不啟用）
- **前端**：Vue 3 + TypeScript + Vite + pnpm
- **部署**：On-prem VM + Docker Compose 單機
- **觀測**：Serilog JSON + CorrelationId（**不做 distributed tracing**）；EFK 上線階段才掛
- **CI/CD**：Jenkins
- **外部服務**：kie.ai（4 個 AI engine：sora2/kling3/seedance2/wan27）+ Gemini（口白）+ 大數據受眾 + 派報 + SMTP

## 範圍邊界

- **Fugo 復購服務**全部 Phase 2，Phase 1 不做
- **AD/LDAP** 全部 Phase 2，Phase 1 用 local user store

## Repo 結構

採 5 個 backend csproj（4 層 + config tool）DDD onion + Domain folder 切模組 + docker/ 集中，外形參考 `D:\FaceAI`，但依 ET 決策調整。

```
et-omniverse/
├── ETOmniverse.sln
├── Directory.Build.props
├── src/
│   ├── backend/
│   │   ├── ETOmniverse.Api/            # Web host + Features/ + 背景任務註冊
│   │   ├── ETOmniverse.Domain/         # 12 模組 folder（Identity, BatchWorkspace, AiVcr, ...）
│   │   ├── ETOmniverse.Infrastructure/ # Repository + EF + 外部 API client + Auth + Job 實作
│   │   ├── ETOmniverse.Common/         # utility + Job/BaseHostService
│   │   └── ETOmniverse.Tools.ConfigTool/
│   └── frontend/
│       └── ETOmniverse.Web/
├── docs/                               # KB（開工後建）：specs / decisions / interviews / patterns / ...
├── docker/                             # compose / Dockerfile / nginx / fluent-bit
├── ci/jenkins/
└── tests/
```

## FaceAI 參考取捨

`D:\FaceAI` 是成熟專案，可參考它的 repo 分層與 infra 分段，但 ET-Omniverse 仍以本專案決策為準：

- 採用：`Api / Domain / Infrastructure / Common` 四層、feature slice、compose base + overlay、環境設定矩陣、healthcheck、Nullable + warnings-as-errors。
- 不採用：MySQL + Dapper / PostgreSQL（本專案 Phase 1 鎖 MSSQL + EF Core 10；Qdrant 暫不啟用）、一開始就完整 Jenkins/Harbor/EFK/Prometheus/Grafana、LocalHybrid lab 連線流程。
- 需刻意保持：Domain 不依賴 EF / Web / 外部 SDK；FaceAI 有些 domain package 依賴不視為本專案先例。

## Api Feature Slice 形狀

Api 專案採 vertical feature folder，避免所有 endpoint、validator、mapper 擠在全域資料夾：

```text
ETOmniverse.Api/Features/<Feature>/
  Adapter/In/Endpoints/          # Minimal API endpoint registration
  Adapter/In/Model/              # Request / response model
  Adapter/In/Validation/         # FluentValidation
  Adapter/In/Extensions/         # Request -> Domain command/query mapping
  Adapter/Out/Model/             # Query/read model when needed
  Adapter/Out/Mapper/            # Domain/db/read model mapping
```

Command side 走 Domain `UseCase` + `Ports`；query side 可以用 read repository / projection，但必須在 spec 或 pattern 中說明它是 deliberate CQRS exception。
## 模組內部結構（hexagonal）

每個 `Domain/<Module>/`：

```
Entity/      # POCO entity
Enum/
Model/       # value object / 純 domain DTO
Ports/       # interface — 對 Infrastructure 的契約
UseCase/     # 業務動作（一檔一 use case）
Service/     # 跨 use case 的 domain service
```

## 12 個業務模組

| 模組 | 用途 |
|---|---|
| Identity | Auth + RBAC + scoped org permission（詳見 `ACCESS-CONTROL.md`） |
| BatchWorkspace | 批次容器 |
| ProductSchedule | Step 0 商品排播 |
| MdPicks | Step 1 MD 挑品 |
| Audience | Step 2 受眾（read-only） |
| AiVcr | Step 3 AI VCR ★ |
| MarketingLink | Step 4 行銷連結 |
| Schedule | Step 5 排播派報 |
| Sms | Step 6 簡訊（read-only Phase 1） |
| Collaboration | SignalR 共編 |
| Notification | 站內 + Email |
| Audit | Audit log |

## 跨層原則

- **Domain 不依賴 EF / Web / 外部 SDK**，全走 Ports interface
- **Infrastructure 實作 Ports**，包 EF Core / Repository / 外部 API client / Auth
- **Api 是 composition root**，組裝 Infrastructure + 註冊 IHostedService + Quartz
- **Common 不依賴 Domain**，只放純 utility（時間 / errors / Job base）
- 跨模組溝通走 Ports 或 Event，不直接互相 reference Service / UseCase

## 部署形態

```
Company VM
└── Docker
    ├── api               (.NET 10，含背景任務 IHostedService + Quartz)
    ├── web               (nginx + Vue static)
    ├── mssql
    ├── elasticsearch     (P1.6)
    ├── kibana            (P1.6)
    ├── apm-server        (P1.6)
    └── fluent-bit        (P1.6)
```

Volumes：`mssql-data` / `es-data` / `media-data`（host mount，路徑 e.g. `/data/media/{batch-id}/{product-id}/{vcr-id}.mp4`）。Qdrant 相關 volume 等 Phase 2/RAG 需求確認後再加。
