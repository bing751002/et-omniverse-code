# AI Guide

> 給「**用 AI 工具開發**」的人看（Claude / Codex / Cursor / Copilot…）。不用 AI 的人可跳過。

## 鎖定工具棧（team 全員一致）

| 層 | 工具 | 必裝 / 選用 |
|---|---|---|
| Agent | **Claude Code** | 必裝（強制） |
| SDD 引擎 | **GSD**（`/gsd:*` 指令） | 必裝：`npx get-shit-done-cc@latest` |
| Spec 倉儲 | `docs/specs/F-XXX-*.md` | n/a（純 git） |
| ADR 倉儲 | `docs/decisions/D-XX-*.md` | n/a |
| 共享設定 | `.claude/settings.json`（checked in） | 必用 |
| 個人覆蓋 | `.claude/settings.local.json`（gitignored） | 選用 |

### 不採用（避免雙軌）

- ❌ **OpenSpec** — 跟 GSD 重疊，雙 spec 倉儲會撞車
- ❌ **個人 `~/.claude/skills/` 加進團隊強制** — 不在 repo 無法分享，做為個人加分項可

### 個人加分項（不強制，自由裝）

- mattpocock skills（mp-tdd / mp-diagnose 等）
- SuperClaude（`/sc:`）
- 任何個人 `~/.claude/skills/`

**鐵則：個人加速器產出的 deliverable 最終都要落到 `docs/`，不可只存在個人狀態。**



## 參考既有專案時的規則

可參考 `D:\FaceAI` 的成熟架構，但 AI 不可直接照搬：

- 先看本 repo 的 `docs/ARCHITECTURE.md`、`docs/INFRA.md`、`docs/DECISIONS.md`。
- FaceAI pattern 只能作為候選；若跟 ET 決策衝突，以 ET 文件為準。
- 搬 infra pattern 時必須降階成 Day 1 可維護版本，不可引入未要求的 Jenkins/Harbor/EFK/LocalHybrid。
- DB 決策以 Phase 1 MSSQL 為準；不要照 FaceAI 改成 MySQL，也不要沿用舊文件裡的 PostgreSQL。Qdrant 暫不啟用。
- 搬 code pattern 時要保留 ET 的 Domain purity：Domain 不依賴 EF / Web / 外部 SDK。

## 文件回寫規則

改 code 後不可只停在 diff。依 [`DOCUMENTATION.md`](DOCUMENTATION.md) 檢查是否要更新 spec / KB / ADR / pattern / retrospective。

最低要求：

- feature 完成 → 對應 spec status 要更新。
- 新決策 → ADR + `DECISIONS.md`。
- 新/改 infra → `INFRA.md`。
- 新/改權限 → `ACCESS-CONTROL.md`。
- 第二次出現同一實作寫法 → `patterns/`。
- 不需要更新 KB 時，回報中要說明理由。

## GitLab MR 規則

出貨時不要手動空白建立 MR。優先使用：

```powershell
python scripts/create-gitlab-mr.py --push --title "<title>"
```

沒有 internal GitLab remote/token 時先跑：

```powershell
python scripts/create-gitlab-mr.py --dry-run --source-branch feat/p1/foundation --title "<title>"
```

MR description 必須來自 `.gitlab/merge_request_templates/Default.md` 或等價 description file。
## 紅線（看到這些一定停手問）

| 紅線 | 為什麼 |
|---|---|
| 改 DB migration 檔（已 commit 的） | schema 不可逆；只能加新 migration |
| 改 `ETOmniverse.sln` | 加 csproj 是基礎工程師職責，避免 git conflict |
| 改 `Directory.Build.props` | 影響全 csproj |
| 改 `docker/compose/` | 影響部署 |
| 寫死 secret / API key 進 code | 一律走 user-secrets 或 env |
| 在 Phase 1 引入 Fugo / AD code | 已決策推 Phase 2 |
| 在 Domain 引用 EF Core / ASP.NET / 外部 SDK | 違反 onion，必須走 Ports |
| 修改 Phase 1 範圍 / 重大決策 | 找使用者確認 |
| 寫沒對應 spec / open item 條目的功能 | 會偏離 scope |

## Stop & Ask 觸發條件

碰到以下情境**先停下來問**，不要硬猜：

- 找不到對應 feature spec（沒 spec 不開工）
- 規劃文件 / 既有決策語焉不詳
- 動到沒 owner 的檔案
- 要改不熟模組的 entity / migration
- 要加新 NuGet / pnpm 套件
- 要寫死任何 magic value（path / token / 檔名）
- 對 Phase 1 vs Phase 2 邊界判斷不清

## 寫 code 前自我檢查

- [ ] 對應到使用者規劃的哪一條（feature id / sub-phase / wave）？
- [ ] 有無未解的開放問題？沒解能不能用 placeholder？
- [ ] 動到哪幾個 csproj？是否碰到 foundation / shared contract？
- [ ] 有對應的 ADR 嗎？是否會跟既有決策衝突？
- [ ] 變更需要對應的測試 / migration / docs 更新嗎？

## 可信度分級（哪種任務 AI 主導 / 共做 / 不可代）

| 任務類型 | AI 程度 |
|---|---|
| Boilerplate / 模板 / 重複結構 | AI 主導 |
| Endpoint / UseCase 實作（spec 清楚時） | AI 主導 |
| 測試 / mock / fixture | AI 主導 |
| 文件草稿 / 註解 | AI 主導 |
| API client 包裝 | AI 主導 |
| Entity / DTO / migration 草稿 | AI 草稿 + 人審 |
| 共編 conflict UX / SignalR 細節 | AI 草稿 + 人審 |
| 業務邏輯（受眾 / 排播規則） | 人主導 + AI 輔助 |
| RBAC / Auth 邏輯 | 人主導 + AI 輔助（安全紅線） |
| DB migration（已 commit 後 alter） | 人主導，禁 AI 直接改 |
| 重大決策 / 範圍變更 | 人決定，AI 不建議自動改 |
| Secret / 連線字串 / 上線設定 | 人決定，AI 禁碰 |

## 失敗模式 / 錯誤恢復

AI 寫錯了的處理順序：

1. **先 git diff** 看 AI 改了什麼
2. 不要 AI 直接覆寫修；先 revert 動到的部分
3. 把錯誤具體寫下來 → 給 AI 新 task 並附上 revert 後的乾淨基礎
4. AI 重做時要求**只改最小範圍**
5. 第二次仍錯 → stop，人接手，記錄到 retrospective

## Prompt 範本（給人用）

```
任務：<一句話目標>
範圍：只改 <files / module>
參考：<spec / pattern>
紅線：<禁碰的東西>
完成定義：
- [ ] code 實作
- [ ] 測試
- [ ] 文件 / migration 同步
驗收：<具體 case>
```

## 不同 AI 工具的入口

| 工具 | 入口 |
|---|---|
| Claude Code | `CLAUDE.md` 自動載入 → 指向本 docs |
| Codex / OpenAI | `AGENTS.md` 自動載入 → 指向本 docs |
| Cursor | `.cursorrules` 自動載入 → 指向本 docs |
| 其他 | 自行手動讀 `docs/` |

**規範一份**（在 `docs/`）**入口多處**，內容不重複維護。

## 給 AI 的開場 prompt（首次接手時用）

```
請先依序讀以下檔案再回應：
- README.md
- docs/ARCHITECTURE.md
- docs/INFRA.md
- docs/DECISIONS.md
- docs/CONVENTIONS.md
- docs/WORKFLOW.md
- docs/DOCUMENTATION.md
- docs/AI-GUIDE.md

讀完回我「OK」加一句話總結你理解的專案階段。然後我會給你具體任務。
```
