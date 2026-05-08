# Git hooks

Repo-shared git hooks。clone 後請跑：

```
git config core.hooksPath .githooks
```

（一次性設定，每次 clone 都要做一次）

## Pre-commit

- ADR 檔（`docs/decisions/D-*.md`）或 `docs/DECISIONS.md` 進 staged → 自動跑 `build-adr-index.py` 重寫並 stage
- spec 檔（`docs/specs/`）進 staged → 跑 `check-spec-links.py` 驗證 src/ 反向連結
- code / infra / auth / ADR 變更 → 跑 `check-doc-governance.py --staged`，強制同步 spec / KB / ADR / pattern，或新增 `docs/no-doc-update-<topic>.md` 說明不更新理由

## 前置條件

- Python 3.10+ 在 PATH（Windows 用 Git Bash 跑 hook）

## 跳過 hook（不建議）

```
git commit --no-verify
```

跳過會在 CI 被擋（CI 跑 `build-adr-index.py --check` 驗證沒漂移）。

## GitLab MR

內部 GitLab 建 MR 優先使用：

```
python scripts/create-gitlab-mr.py --push --title "<title>"
```

本機沒有 GitLab token / remote 時可先 dry-run：

```
python scripts/create-gitlab-mr.py --dry-run --source-branch feat/p1/foundation --title "<title>"
```
