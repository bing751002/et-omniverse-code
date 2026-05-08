---
date: YYYY-MM-DD
trigger: <incident | phase-complete | recurring-mistake>
owner: <github handle>
related-spec: [F-XXX]
related-adr: [D-XX]
---

# Retrospective: <title>

## What happened（append-only，按時間軸）

- HH:MM ...
- HH:MM ...

## Impact

- 使用者影響：...
- 系統影響：...
- 時間損耗：...

## Root cause

<根因分析 — 5 whys 或同等深度>

## Lessons（可改寫）

- ...

## Action items

- [ ] 更新 `docs/CONVENTIONS.md` 已知陷阱表加一條：...
- [ ] 補測試覆蓋：...
- [ ] ADR 紀錄決策變更（如有）：D-YY
- [ ] 補 pattern：`docs/patterns/<name>.md`
- [ ] 更新 `docs/AI-GUIDE.md` 紅線（如踩到 AI 邊界）

## References

- PR: #XXX
- 相關 commit: ...
- 通訊 thread: ...
