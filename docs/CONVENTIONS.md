# Conventions

## 寫 code（後端 / .NET）

- **Domain 不依賴 EF / Web / 外部 SDK**，全走 Ports interface
- **Endpoint 用 Minimal API**（Controllers 只在必要時）
- **每個 Endpoint 對應一個 UseCase 檔**
- 每個外部 API client 包 wrapper 自動 log latency / result
- DbContext 用 partial class，每模組一個 `EtOmniverseDbContext.<Module>.cs`
- Migration 加新支前先 git pull，避免序列衝突
- `record` for DTO / value object，`class` for entity
- `Nullable` enable，禁 `!`（null-forgiving）除非註解原因
- 例外用 Result type / ProblemDetails，不亂 throw

## Feature Slice 規則（參考 FaceAI 後收斂）

- Endpoint 層只做 HTTP binding、validation、呼叫 use case、回傳 ProblemDetails / response。
- Request model 轉 domain command/query 必須集中在 extension 或 mapper，不散落在 endpoint body。
- FluentValidation 放 Adapter/In，不放 Domain。
- Command repository 實作 Domain port；query repository 若直接回傳 read model，必須命名為 Query/Read 並避免混入 command side。
- 每個 feature 的第一個 endpoint 要同時建立最小測試樣板：UseCase unit test + API/WebApplicationFactory happy path。
## 寫 code（前端 / Vue）

- Composition API + `<script setup lang="ts">`
- Pinia for state（不用 Vuex）
- Router routes 按模組分檔（避免 `router/index.ts` git conflict）
- API client 從 OpenAPI 自動產，不手寫
- 未開放功能用 `<NotAvailableYet :ref-link="..." />` 元件

## 寫 code（通用）

- 結構化 log + CorrelationId（每行 log 自動帶）
- 不寫不必要的 try-catch（內部 code trust，邊界才驗證）
- 不加註解描述 *what*；只在 *why* 不明顯時加一行
- 不寫 backwards-compat shim（pre-1.0 階段）
- 不寫 stub / 半完成 — 不會做就標 placeholder + 連結對應 open item

## 命名

| 領域 | 規則 | 範例 |
|---|---|---|
| C# | PascalCase（method/class/property）/ camelCase（local） | `LoginUseCase` / `userId` |
| TypeScript | camelCase（var/function）/ PascalCase（component/type） | `useUser` / `BatchCard.vue` |
| DB | snake_case + 複數 table 名 | `users`, `batches` |
| 模組命名跟 7-step 對齊 | — | `ProductSchedule`, `MdPicks`, `Audience`, `AiVcr`, `MarketingLink`, `Schedule`, `Sms` |

## Git / Commit

### 分支

- 命名：`feat/<owner>/<short-name>`，例 `feat/p1/auth-rbac`
- 一個 phase / feature 一支分支，PR 進 main 用 squash merge

### Commit message 格式

採用 Conventional Commits 結構（type / scope 英文小寫，subject + body 用中文）：

```
<type>(<scope>): <中文 subject — 50 字內，祈使句不加句號>

<中文 body — 回答 why、列關鍵改動，每行 ≤ 72 字>
<空行隔段>
<可加 Rationale / Refs / Closes 段>
```

**Type**（必填，固定七種）：

| type | 用途 |
|---|---|
| `feat` | 新功能（對應 spec AC 落地） |
| `fix` | Bug 修正 |
| `docs` | 文件 / spec / planning artifact 變動（含 spec status flip） |
| `chore` | 工程設定 / 工具 / 依賴 / .gitignore / hook |
| `refactor` | 不改行為的結構調整 |
| `test` | 新增 / 修改測試（不含 fix 順手補測） |
| `ci` | CI / governance script |

**Scope**（必填，小寫 kebab-case）：

- 功能 spec：`F-001`、`F-002`（對應 docs/specs/）
- Phase 計劃：`phase-02`、`02-01`（plan-level commit 用 plan id）
- 模組：`spec`、`deps`、`vite`、`planning`、`logging`、`infra`
- 跨域工程：`governance`、`hook`、`workflow`

**Subject 規則**：

- 中文祈使句，不加句號，不加表情符號
- ASCII 與中文之間留空格：`新增 vue-router 設定` ✓ /  `新增vue-router設定` ✗
- 50 字以內，超過放 body
- 可附 ` — <一句話 rationale>` 補上 why（破折號用全形 `—` 或 ASCII `--`，全 repo 統一用 `—`）

**Body 規則**（可選，但下列情境必填）：

- commit 影響超過 1 個檔案 / 1 個邏輯區塊
- 引入新概念、新依賴、新慣例
- 推翻之前決策或繞過 governance hook（必寫 Rationale）

格式：

```
<一段話說明改動 + 動機>

- 條列關鍵變更（檔案層級，不重複 diff）
- 條列影響面（哪些後續 phase / spec 受影響）

Rationale: <為何這樣做、為何不那樣做>
Refs: F-002 AC-3, docs/DECISIONS.md D-14
Closes: #123
```

### Atomic commit 紀律（Phase 1 D-08）

一個 commit = 一個邏輯改動，禁止「順手」混入無關修改：

- spec status 變動獨立 commit（`docs(spec): F-002 status: draft -> approved`），4-step 流轉就要 4 個 commit
- code 與對應 doc / CONVENTIONS / INFRA 補丁可同 commit（同一邏輯改動的兩面）
- 但測試補丁建議獨立 commit（除非該測試是 feat 的驗收前置）
- pre-commit hook 失敗 → 修問題後**新建 commit**，禁用 `--amend`（會誤改前一 commit）

### 禁止事項

- ❌ `--no-verify` 跳 hook（governance hook 是強制護欄；要繞過走 D-11/D-18 rationale-bypass，commit message 必寫 Rationale）
- ❌ 使用 `Co-Authored-By` 行（個人 repo / 內部專案不需要）
- ❌ subject 用「update」、「fix」、「wip」這種無資訊量字眼
- ❌ 一 commit 包含多個 plan / 多個 AC 的改動（除非該 plan 明確要求）
- ❌ `git push --force` 到 `main`（其他分支須使用者明確同意）

### 範例

✅ 好範例（取自 repo history）：

```
docs(spec): F-001 status: draft -> approved
```

```
chore(planning): adopt SDD purist policy — only spec on git

- .gitignore: ignore .planning/phases/, STATE.md, codebase/, config.json
- untrack STATE.md + config.json via git rm --cached
- docs/WORKFLOW.md 新增 Phase Close Checklist

Rationale: 2-人團隊 feature 級分工 — phase recipe (HOW) 過完就是雜訊；
永久契約 (spec / ADR) 已涵蓋團隊所需。
```

```
feat(F-001): 落地 /login 表單與 /welcome 頁（vue-router 串接）

- src/views/Login.vue 新增 username/password 表單 + submit 跳轉
- src/views/Welcome.vue 新增歡迎文字
- src/router/index.ts 註冊兩個 route
- main.ts 掛載 router

Refs: F-001 AC-1 ~ AC-3
```

❌ 反範例：

- `update` — 零資訊量
- `fix bug` — 沒 scope 沒 why
- `feat: 一次寫完整個 phase 02` — 違反 atomic
- `chore: 順手調整 .gitignore 並加 vue-router` — 兩件無關事混 commit

### 工具與檢核

- pre-commit hook (`.githooks/pre-commit`) 自動執行：
  - `check-doc-governance.py` — spec / ADR frontmatter 完整性
  - `check-spec-links.py` — spec 引用連結存活
  - `build-adr-index.py` — ADR 索引重建
  - （Phase 2 起加上）`check-no-console-write.py` — 業務 code 禁區掃描
- 加新 csproj 是基礎工程師職責（避免 sln 衝突）
- PR：squash merge，PR title 沿用主要 commit subject 格式

## 測試紀律（開工後生效）

| 層級 | 對象 | 工具 |
|---|---|---|
| Unit | Domain UseCase / Service | xUnit |
| Integration | Repository + EF | Testcontainers MSSQL |
| API | Endpoint / 整合 | WebApplicationFactory |
| E2E | 跨前後端 happy path | Playwright |

- 沒測試不算完成（feature spec 列驗收條件就要對應測試）
- 修 bug 一定先寫 failing test 再修

## 依賴管理

加 NuGet / pnpm 套件前先問：
1. 真的需要嗎？標準庫 / 既有套件能不能解？
2. 是否會引入新概念 / 學習成本？
3. License 是否相容（內部商業）？
4. 維護狀態（last commit / star / issue）？

## 已知陷阱（這專案特有）

| 陷阱 | 預防 |
|---|---|
| 7-step 流程描述容易散在多處 | 單一 source of truth 由規劃文件指定，其他檔提到流程一律連結 |
| Fugo 串接想偷做 | Phase 2，看到任何 Fugo 相關 code → reject |
| AD 串接想偷做 | Phase 2（D14），同上 |
| AI VCR 直接寫死支數 / engine | 多 engine 多版本是核心特性，別寫死 |
| 受眾欄位（Q-AU-001 未解）寫死 schema | 用 placeholder，待訪談確認 |
| 排播衝突規則（Q-SCH-001 未解）寫死邏輯 | 同上，待訪談確認 |
| 共編 conflict UX 隨便做 | 待設計，先 placeholder |
| 業務 metric（VCR 用量 / 批次完成）寫到 Prometheus | 業務 metric 走 MSSQL（不要混進時序 DB） |
| 提前接 Qdrant | Qdrant 暫不啟用；Phase 2/RAG 需求確認後再加 |

## 安全 / 效能紅線

- **絕對禁**：寫死 secret / API key / 連線字串進 code（一律走 user-secrets / env）
- **絕對禁**：未經 sanitize 的使用者輸入直接拼 SQL（用 EF / 參數化）
- **絕對禁**：上傳檔案不檢副檔名 + Content-Type
- **避免**：N+1 query（複雜 list 必用 `.Include` 或 split query）
- **避免**：同步 I/O（全 async）
- **避免**：在 controller / endpoint 內寫業務邏輯（要進 UseCase）

## 詞彙

詳見 [`GLOSSARY.md`](GLOSSARY.md)。
