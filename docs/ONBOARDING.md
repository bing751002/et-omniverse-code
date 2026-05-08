# Onboarding

新成員第一天 SOP。

## Day 1：環境

```
1. git clone <repo>
2. 安裝 .NET 10 SDK
3. 安裝 pnpm + Node 20+
4. 安裝 Docker Desktop
5. 安裝 Claude Code（強制工具，版本由 lead 公告）
6. 在 repo 內跑：npx get-shit-done-cc@latest（GSD 套件）
7. cd src/frontend/ETOmniverse.Web && pnpm install
8. dotnet restore
9. docker compose up -d（看 docker/README.md）
```

## Day 1：閱讀（30-45 分鐘）

依序讀：

1. [`../README.md`](../README.md) — 專案概覽 + 階段
2. [`ARCHITECTURE.md`](ARCHITECTURE.md) — 12 模組 / onion / FaceAI 取捨
3. [`INFRA.md`](INFRA.md) — Docker compose / 環境矩陣
4. [`ACCESS-CONTROL.md`](ACCESS-CONTROL.md) — 公司/事業群/部門/外部公司四層
5. [`DECISIONS.md`](DECISIONS.md) — D10-D17 決策摘要
6. [`CONVENTIONS.md`](CONVENTIONS.md) — 命名 / code style / 已知陷阱
7. [`WORKFLOW.md`](WORKFLOW.md) — SDD 流程 + GSD 對映 ★ 重要
8. [`AI-GUIDE.md`](AI-GUIDE.md) — 鎖定工具棧 + 紅線

## Day 1：對齊工具

- 確認 Claude Code 能讀 `CLAUDE.md` → 自動載入規範
- 跑 `/gsd:health` 檢查 GSD 狀態
- 跑 `/gsd:progress` 看當前 milestone / phase
- 確認 `.claude/settings.json` 生效（permissions 應允許 dotnet/pnpm/docker/git）

## Day 1：第一個練習 PR（不領真實任務前必做）

熟悉流程：

```
1. 跟 lead 拿一個 cross-cutting 練習題目（小範圍、低風險）
2. /gsd:add-phase
3. /gsd:discuss-phase   ← 對齊脈絡
4. /gsd:plan-phase      ← 產 PLAN.md
5. 寫 docs/specs/F-XXX-<name>.md（status: draft → approved）
6. /gsd:execute-phase   ← 實作 + 測試
7. /gsd:verify-work     ← 自驗
8. 開 PR：title 引 F-XXX，description 按 WORKFLOW.md 範本填
9. 等 lead review → squash merge
10. spec status 改 implemented
```

## SDD 鐵則（每天都要記得）

- **沒 spec 不開工**：找不到對應 F-XXX 先停下來建 spec / 問 lead
- **沒測試不算完成**：spec 列驗收條件就要對應測試
- **修 bug 一定先寫 failing test 再修**：紅綠重構（CONVENTIONS.md 紀律）
- **重大決策先寫 ADR**：才能進 code
- **Phase 1 範圍**：看到 Phase 2 code（Fugo / AD / Qdrant / 上線級監控）立刻 reject

## 紅線速查（完整版見 [`AI-GUIDE.md`](AI-GUIDE.md)）

絕對禁：
- 寫死 secret / API key 進 code
- 直接拼 SQL（用 EF / 參數化）
- 改已 commit 的 migration（只能加新 migration）
- 在 Domain 引用 EF / Web / 外部 SDK（違反 onion）
- force push 到任何 protected branch
- 改 `ETOmniverse.sln` / `Directory.Build.props` / `docker/compose/`（基礎工程師職責）

## 求救路徑

| 問題類別 | 找誰 / 看哪 |
|---|---|
| 規範問題 | 看 `docs/`，沒寫的問 lead |
| 工具問題 | `/gsd:help` / Claude Code `/help` |
| 業務問題 | 找對應 stakeholder 開訪談（紀律見 WORKFLOW.md） |
| 環境問題 | 看 `INFRA.md` 或問基礎工程師 |
| AI 寫錯第二次 | 立刻停手 → 寫 retrospective（看 AI-GUIDE.md 失敗模式） |

## Phase 1 範圍提醒（重複講因為很重要）

只能做 Phase 1：
- ❌ Fugo 復購服務 → Phase 2
- ❌ AD/LDAP → Phase 2（D14）
- ❌ Qdrant / RAG → Phase 2
- ❌ Jenkins / Harbor / EFK 上線級監控 → P1.6 之後

看到 Phase 2 code 立刻 reject，不論誰寫的。
