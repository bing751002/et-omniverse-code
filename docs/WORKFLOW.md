# Workflow

## SDD（Spec-Driven Development）

**沒 spec 不寫 code**。流程：

```
feature 起點
   ↓
spec 草稿（人 / AI 起草）
   ↓
訪談對齊（需要時）
   ↓
plan 拆解任務
   ↓
plan-check（確認跟 spec / 既有決策不衝突）
   ↓
code
   ↓
test（unit / integration / e2e 視層級）
   ↓
self review
   ↓
PR review（使用者或工具）
   ↓
merge
```

每個 feature 對應一份 `docs/specs/F-XXX-*.md`（開工後）。

## SDD 流程詳細（GSD 對映）

團隊鎖定用 **Claude Code + GSD** 跑 SDD。每個情境對應的指令鏈：

### 新 feature

| # | 動作 | GSD 指令 / 對應 | 產出 |
|---|---|---|---|
| 1 | 受影響面盤點 | `grep -r "^id: F-" docs/specs/` + 看 `docs/DECISIONS.md` | 影響清單 |
| 2 | 開 phase | `/gsd:add-phase` | `.planning/milestones/<m>/phases/<p>/` |
| 3 | 對齊脈絡 | `/gsd:discuss-phase` | DISCUSSION.md |
| 4 | plan | `/gsd:plan-phase`（preflight check 跑完） | PLAN.md |
| 5 | 寫 spec | 主 Claude 寫 / 共寫 | `docs/specs/F-XXX-*.md`（status: draft → approved） |
| 6 | 重大決策 | 寫 ADR | `docs/decisions/D-XX-*.md`（status: accepted） |
| 7 | 實作 | `/gsd:execute-phase` | code + 測試 |
| 8 | 驗收 | `/gsd:verify-work` | UAT 結果 |
| 9 | 出貨 | `/gsd:ship` | PR + spec status: implemented |

預設 **F-XXX : phase = 1 : 1**。一個 feature 太大時拆多個 F-XXX，每個各自走一遍。

### 修改既有 spec

```
1. grep -r "^id: F-" docs/specs/ → 列出所有受影響 F-XXX
2. 判斷修改類型：
   - 小改（行為微調 / 欄位增刪不改契約）
     → 同 F-XXX patch，frontmatter updated 改今天，status 不變
   - 大改（行為改變 / 模組邊界異動）
     → status: implemented → modifying，PR merge 後改回 implemented
   - 替換（舊邏輯整段廢）
     → 舊 F-XXX status: deprecated，加 superseded-by: F-YYY
     → 新 F-YYY status: draft，加 supersedes: F-XXX
3. 決策變了 → docs/decisions/ 新 ADR：
   → 舊 ADR status: superseded，加 superseded-by: D-YY
   → 新 ADR status: accepted，加 supersedes: D-XX
   → 同步更新 docs/DECISIONS.md 摘要表
4. /gsd:insert-phase（不是 add-phase — 修改是插入既有 milestone）
5. 後續流程同新 feature 步驟 3-9
```

### Bug fix

```
1. /gsd:debug → 系統化除錯（持久 state、checkpoint）
2. 必先寫 failing test 再修（CONVENTIONS.md 紀律）
3. 修完跑 /gsd:verify-work
4. 若反映 spec 漏寫 → 補 spec / 補 ADR
5. 若 AI 寫錯第二次 → 寫 retrospective（AI-GUIDE.md 失敗模式）
```

### 跨模組大重構

```
1. /gsd:new-milestone → 開新 milestone
2. 多個 phase 對應多個 F-XXX
3. 完工後跑 /gsd:audit-milestone + /gsd:complete-milestone
```

## Phase Close Checklist（SDD 純粹派回填規則）

`.planning/phases/` 不上 git（見 `.gitignore`）— phase close 前**必須**把以下軌跡手動摘要回 `docs/`，否則決策軌跡會永久遺失：

| 從 .planning/phases/`<phase>`/ | 摘要到 docs/ | 紀律 |
|---|---|---|
| `<p>-CONTEXT.md` 的 D-XX 決策 | `docs/decisions/D-XX-*.md`（一個 D 一份） | 同步更新 `docs/DECISIONS.md` 摘要表 |
| `<p>-RESEARCH.md` 的關鍵技術發現 | `docs/specs/F-XXX-*.md` 的 Background / Notes 區塊 | 引用驗證指令（curl / grep）留痕 |
| `<p>-VERIFICATION.md` 的 UAT 結果 | spec status: implementing → implemented + commit message | spec frontmatter `updated` 改今天 |
| 踩到的 gotcha / 反模式 | `docs/CONVENTIONS.md` 「已知陷阱」段 | 寫具體例子，下次別人才看得懂 |
| Demo / 走讀素材 | `docs/walkthroughs/phase-XX.md` | pointer 為主（連結到 spec / commit / decision） |
| 失敗的 AI 互動 / 反覆踩坑 | `docs/retrospectives/phase-XX.md` | 對應 AI-GUIDE.md 失敗模式分類 |

**何時跑**：
- 主要時機：`/gsd:verify-work` 通過後、`/gsd:ship` 之前
- `/gsd:ship` PR description 應引用回填到的 `D-XX` / `F-XXX` / walkthrough 連結
- 只有 spec + ADR + walkthrough + retrospective 進 git，phase 食譜本身丟掉

**為什麼不自動產**：摘要需要判斷「哪些是永久決策、哪些是一次性 noise」— 認知 cost 在使用者 / 主 Claude 不在工具，符合 AI-GUIDE.md「事實層自動、洞察層留摩擦」原則。

**例外**：milestone v1.0 Phase 01 的 `.planning/phases/01-frontend-login-demo/` 已 commit 在 git history，作為「process validation 第一輪證據」永久保留，不追溯重構（符合 Kill Switch Day 紀律）。下個 phase 起套此 checklist。

## Spec status 流轉

| status | 意義 | 何時改 |
|---|---|---|
| `draft` | 草稿，未審 | 剛起草、訪談中 |
| `blocked` | 等訪談 / 等決策 | 有 open question 卡關 |
| `approved` | 通過 review，可開工 | plan 完、ADR 對齊 |
| `implementing` | 實作中 | execute-phase 進行 |
| `implemented` | 已上線 | verify-work 通過、ship |
| `modifying` | 既有 spec 在改 | insert-phase 開始時 |
| `deprecated` | 已棄用 | 被新 F-YYY supersedes |

## ADR（Architecture Decision Records）

每個重大決策一個 D 編號 + `docs/decisions/DXX-*.md`。

- 已 accepted 的不改寫；要變更開新 ADR + `supersedes`
- 改決策前先看 `docs/DECISIONS.md` 是否衝突
- 新編號續 D17 起

## 訪談紀律

- 訪談完當天必建 `docs/interviews/*.md`
- 結論區 **append-only**，不改寫
- My Take 區可改寫，改寫加 `> [!tip] 框架升級 (date)`
- 訪談 unblock 對應 spec → 在 spec 標 status 從 `blocked` → `draft`

## Pattern

- 新發明 pattern 前先看 `docs/patterns/`
- 第二次用同個寫法 → 抽出 pattern 寫一份 `docs/patterns/X.md`
- pattern 範例不寫太長（>20 行指向 repo 內實作檔）

## Infra / Config 變更流程

參考 FaceAI 的 infra 紀律，但以漸進式落地：

1. 先更新 `docs/INFRA.md` 的環境矩陣或 compose 形狀。
2. 再改 `docker/`、`appsettings.*.json`、CI script。
3. PR description 必須列出 config key、secret 來源、healthcheck、rollback 影響。
4. 新增服務要提供本機啟停命令與最小驗證命令。

Day 1 只允許建立可跑 foundation 的 infra；監控、Harbor、production deploy 不可偷渡。
## PR 規則

- 一個 PR 對應一個 feature spec / 一個 bug fix
- 分支：`feat/<owner>/<short-name>`（例 `feat/p1/auth-rbac`）；squash merge
- migration 改動要寫升級 / 降級指引
- 公司內部 GitLab 使用 Merge Request；建立 MR 優先跑 `python scripts/create-gitlab-mr.py --push --title "<title>"`，由腳本注入模板，再依變更內容補齊欄位。

### PR description 必填欄位（缺一不收）

```markdown
## 對應 spec
- F-XXX [連結到 docs/specs/F-XXX-*.md]
- 修改類型：新建 / 小改 / 大改 / 替換

## 受影響範圍
- spec：F-XXX, F-YYY
- ADR：D-XX（新增 / 修改 / supersedes 鏈）
- csproj：<列出>
- migration：<是否影響 DB schema、升降級指引>

## 驗收
- [ ] 對應測試（unit / integration / api / e2e 視層級）
- [ ] spec status 已更新（implementing → implemented，或 modifying → implemented）
- [ ] ADR 同步（如有重大決策）
- [ ] CONVENTIONS.md 已知陷阱新增（如踩到坑）

## Break change
- 無 / 列出
```

## 衝突熱區（先講好規則）

| 熱區 | 規則 |
|---|---|
| `ETOmniverse.sln` | 加 csproj 由基礎工程師統一改 |
| `EtOmniverseDbContext` | 拆 partial class 各模組一檔 |
| EF Migration | 加新支前先 pull main，序列化單線 |
| `docker/compose/*.yml` | 基礎工程師維護 |
| 前端 router | 按模組分檔（一檔 routes register） |
| `appsettings.*.json` | 加 config 用 PR，並在 description 列出影響 |

## 文件更新紀律

- 詳細規則見 [`DOCUMENTATION.md`](DOCUMENTATION.md)
- 改 spec 必更新 `status` + `updated`
- 改規範（命名 / 紅線 / pattern）必同步更新 `docs/CONVENTIONS.md`
- 重大決策必加 ADR 並更新 `docs/DECISIONS.md` 摘要
- 上線後事件 / 教訓寫進 `docs/retrospectives/`
- 改 code 後必檢查是否要回寫 KB；若不需要，PR description 要寫理由

## 規劃期 → 開工的轉換

開工前要落地的事：
1. 規劃文件搬進 `docs/`（specs / decisions / interviews 各歸位）
2. 建 `docs/INDEX.md` KB 入口
3. git init + `Directory.Build.props` + `.gitignore`
4. 建 sln 骨架
5. P1.0a-d 先個人作業：依 Day 1 runbook 建 foundation，第一個 vertical slice 用 F-014 NotAvailableYet 端到端跑通後再抽樣板
