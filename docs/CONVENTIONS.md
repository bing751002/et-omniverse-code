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

- 分支：`feat/<owner>/<short-name>`，例 `feat/p1/auth-rbac`
- Commit message：祈使句 + 簡短，不寫 Co-Authored-By
- PR：squash merge
- 加新 csproj 是基礎工程師職責（避免 sln 衝突）

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
