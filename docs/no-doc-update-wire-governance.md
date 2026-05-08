# No Documentation Update Rationale — Wire Governance Hooks

## Change

- Files changed:
  - `.githooks/pre-commit`
  - `ci/jenkins/Jenkinsfile`
  - `scripts/check-doc-governance.py`
- Summary: 把 `check-doc-governance.py` 接進 pre-commit hook 與 Jenkins Docs Lint stage；同時擴充 Rule 1 的 required docs 範圍、為 access-control 與 adr-summary 兩條 rule 關閉 rationale bypass。

## Reason No KB Update Is Needed

`docs/DOCUMENTATION.md` §強制機制（line 162-173）原本就宣告 pre-commit hook 與 Jenkinsfile 會跑 `check-doc-governance.py`。這次變更只是讓**實裝對齊既有宣告**，並未引入新行為、新規則或新檔案治理範圍：

- DOCUMENTATION.md 對 governance script 的描述（line 172）未改變。
- pre-commit / Jenkinsfile 的角色（line 170-171）未改變。
- 新增的 rule 收緊（access-control / adr-summary 禁用 rationale bypass）是 DOCUMENTATION.md「LLM 寫文件規則」與「不該做」精神的具體化，原則層無新增。
- 沒有改變 spec、ADR、架構、infra 拓撲、權限模型、術語、pattern 或 retrospective。

## Verification

- Command: `python scripts/check-doc-governance.py`
- Result: 修補後執行；預期不再有「宣告但未接線」的漂移。
