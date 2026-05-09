# Documentation Governance

> 給人和 LLM agent 的文件寫作規範。目的：讓 SDD spec、Knowledge Base、ADR、pattern 不混在一起，並確保改完 code 後能把長期知識回寫到 KB。

## 一句話

```text
Spec = 這次要做什麼。
Knowledge Base = 以後大家要怎麼理解這個系統。
```

開發時用 spec 驅動；完成後把穩定、會重複使用的知識抽回 KB。

## 文件分類

| 類型 | 位置 | 用途 | 生命週期 |
|---|---|---|---|
| KB 入口 | `docs/INDEX.md` | 文件導覽 | 長期維護 |
| 架構 KB | `docs/ARCHITECTURE.md` | 技術棧、模組、repo 結構 | 長期維護 |
| Infra KB | `docs/INFRA.md` | Docker、環境、CI/CD | 長期維護 |
| 權限 KB | `docs/ACCESS-CONTROL.md` | 權限模型 | 長期維護 |
| 決策摘要 | `docs/DECISIONS.md` | ADR 摘要表 | 長期維護 |
| 詳細 ADR | `docs/decisions/D-XX-*.md` | 單一重大決策 | accepted 後 append/supersede |
| 規範 | `docs/CONVENTIONS.md` | code style、測試、已知陷阱 | 長期維護 |
| 詞彙 | `docs/GLOSSARY.md` | 業務/技術詞彙 | 長期維護 |
| Workflow | `docs/WORKFLOW.md` | SDD / PR / 文件更新流程 | 長期維護 |
| AI Guide | `docs/AI-GUIDE.md` | agent 紅線與交接規則 | 長期維護 |
| Feature spec | `docs/specs/F-XXX-*.md` | 單一功能/change 的開發契約 | draft → implemented/deprecated |
| Pattern | `docs/patterns/*.md` | 重複出現的實作寫法 | 第二次使用後抽出 |
| Interview | `docs/interviews/*.md` | 訪談原始紀錄與結論 | append-only |
| Retrospective | `docs/retrospectives/*.md` | 事故、踩坑、AI 失敗模式 | append-only |

## Spec vs KB

放 spec：

```text
單一 feature / change 的目標
本次範圍 / 不範圍
API request / response
UI 行為
驗收條件
測試計畫
本次 open questions
```

放 KB：

```text
跨多個 feature 都會用到的規則
穩定的架構概念
模組責任
權限模型
部署方式
命名與 code style
常見陷阱
已採納 pattern
```

判斷規則：

```text
如果內容只為了完成這一次 feature → spec。
如果內容未來很多 feature 都要懂或遵守 → KB。
```

## Code 改完後的 KB 回寫流程

每次完成 code change 後，必跑這個檢查：

```text
1. spec 狀態是否更新？
2. 是否產生新決策？
3. 是否改變架構 / 模組責任？
4. 是否改變 infra / config / env？
5. 是否改變權限 / 資料可見性？
6. 是否新增術語？
7. 是否形成重複 pattern？
8. 是否踩到坑或修掉 AI 失敗模式？
```

對應更新：

| 變更 | 要更新 |
|---|---|
| feature 完成 | `docs/specs/F-XXX-*.md` status → `implemented` |
| feature 行為改變 | 對應 spec updated，必要時開新 F-YYY supersedes |
| 重大技術/產品決策 | `docs/decisions/D-XX-*.md` + `docs/DECISIONS.md` |
| 模組責任或架構改變 | `docs/ARCHITECTURE.md` |
| docker/env/CI/config 改變 | `docs/INFRA.md` |
| 權限模型/可見性改變 | `docs/ACCESS-CONTROL.md` |
| code style/測試規則/紅線改變 | `docs/CONVENTIONS.md` |
| 新詞彙或名詞定義 | `docs/GLOSSARY.md` |
| 第二次出現同一寫法 | `docs/patterns/<name>.md` |
| 事故/踩坑/AI 連續寫錯 | `docs/retrospectives/YYYY-MM-DD-*.md` |
| agent 紅線或交接規則改變 | `docs/AI-GUIDE.md` |

## 更新順序

### 新 feature

```text
1. 建 / 更新 spec
2. 實作 code + tests
3. 回頭更新 spec status / acceptance result
4. 抽出需要長期保存的內容到 KB
5. 如有重大決策，補 ADR + DECISIONS 摘要
6. 如有 pattern，補 patterns/
7. PR 前跑 documentation checklist
```

### Bug fix

```text
1. 找到對應 spec 或建立 bug note
2. 寫 failing test
3. 修 code
4. 如果 spec 漏掉此行為 → 補 spec
5. 如果是通用陷阱 → 補 CONVENTIONS 或 retrospectives
```

### Infra / config change

```text
1. 先更新 INFRA.md 設計
2. 再改 docker / appsettings / CI
3. 補驗證命令
4. PR description 說明 config key、secret 來源、rollback
```

## LLM 寫文件規則

LLM 寫文件時必守：

- 不要把 speculation 寫成 accepted fact。
- 不確定就標 `OPEN-QUESTION` 或放到 spec 的 open questions。
- 已 accepted 的 ADR 不改寫；要變更就新增 ADR 並 `supersedes`。
- 訪談結論 append-only；不要重寫歷史。
- KB 要寫白話、可被下一個 agent 理解，不只寫給當下對話。
- 文件要連到 source：spec / ADR / code path / decision id。
- 不要把一次性 task 細節塞進 KB。
- 不要把長期規則只留在 spec。

## PR 文件檢查清單

每個 PR description 必須回答：

```text
- [ ] 對應 spec 是哪一份？
- [ ] spec status 是否更新？
- [ ] 是否需要 ADR？若需要，是否已補？
- [ ] 是否需要更新 KB？更新了哪些檔？
- [ ] 是否新增/改變 config、env、docker、CI？
- [ ] 是否新增/改變權限規則？
- [ ] 是否新增術語？
- [ ] 是否形成可複用 pattern？
- [ ] 是否有踩坑需要 retrospective？
```

若答案是「不需要更新 KB」，PR description 要寫一句理由。

## 強制機制

本專案從 P1.0 起就啟用文件治理檢查。

| 機制 | 檔案 | 行為 |
|---|---|---|
| MR template | `.gitlab/merge_request_templates/Default.md` | GitLab merge request 預設模板 |
| PR fallback template | `.github/pull_request_template.md` | 非 GitLab 平台備用 |
| pre-commit hook | `.githooks/pre-commit` | commit 前跑文件治理檢查 |
| CI stage | `ci/jenkins/Jenkinsfile` | Jenkins 跑 `check-doc-governance.py` + `check-spec-links.py` |
| 檢查腳本 | `scripts/check-doc-governance.py` | code / infra / auth / ADR 變更必須同步文件或提出 no-doc rationale |
| MR 建立腳本 | `scripts/create-gitlab-mr.py` | 透過 GitLab API 建立 MR 並注入模板；欄位仍需依變更內容補齊 |

例外處理：

```text
如果真的不需要更新 KB/spec/ADR/pattern，
新增 docs/no-doc-update-<topic>.md，
說明為什麼這次變更沒有改變行為、架構、infra、權限、術語或 pattern。
```

不允許只在 chat 裡說「不用更新文件」。

## GitLab MR 自動建立

內部 GitLab 使用 merge request，不靠人手動複製模板；腳本會建立 MR 並注入預設 description，欄位仍需依變更內容補齊。

必要環境變數：

```text
GITLAB_URL=https://gitlab.internal
GITLAB_TOKEN=<token with api scope>
GITLAB_PROJECT_ID=<project id>   # 可選；若 origin remote 可推得 project path，可省略
```

建立 MR：

```powershell
python scripts/create-gitlab-mr.py --push --title "F-XXX: <title>"
```

Dry run：

```powershell
python scripts/create-gitlab-mr.py --dry-run --source-branch feat/p1/foundation --title "F-XXX: <title>"
```

規則：

- source branch 不可等於 target branch。
- 預設 target branch 是 `main`。
- 預設使用 `.gitlab/merge_request_templates/Default.md` 作 description。
- 預設 `squash=true`、`remove_source_branch=true`。
- 沒有 GitLab token / remote 時只能 dry-run。

## 範例：Create Batch

Spec 寫：

```text
F-021 Create Batch
- 建立 batch API
- batch.owner_org_unit_id 必填
- 權限檢查 batch.create
- 不做 batch_access_grants
- 驗收：東森購物使用者只能建立東森購物 batch
```

KB 寫：

```text
ACCESS-CONTROL.md
- Batch.ownerOrgUnitId 表示 batch 屬於哪個事業群
- Phase 1 權限使用功能面 RBAC + 事業群 scope
- batch_access_grants 是未來 feature flag
```

完成後：

```text
F-021 status: implemented
ACCESS-CONTROL.md 若有新規則則更新
CONVENTIONS.md 若形成固定 endpoint/usecase 寫法則更新
```

## 不該做

```text
不該：改完 code，spec 還是 draft。
不該：新增權限判斷，只改 code，不更新 ACCESS-CONTROL.md。
不該：新增 docker env，只改 compose，不更新 INFRA.md。
不該：同一 pattern 用第二次，仍散在 code 裡不寫 patterns。
不該：把討論中的想法直接寫進 DECISIONS.md 當 accepted。
```
