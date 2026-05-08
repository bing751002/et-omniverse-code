# Retrospectives

事件後 / 上線後 / 階段性檢討紀錄。

## 何時寫

- **上線後出事**（user-visible 故障 / data corruption / 安全事件）
- **AI 寫錯第二次以上**（依 [`../AI-GUIDE.md`](../AI-GUIDE.md) 失敗模式紀律）
- **Phase 完工後**若有重大教訓
- **訪談 / spec / ADR 反覆改超過 3 次** — 訊號代表流程有問題

## 檔名格式

`YYYY-MM-DD-<title>.md`

## 紀律

- **What happened 區 append-only**（事實層）
- **Lessons 區可改寫**（理解可深化）
- 從 retrospective 抽出的「下次別這樣」要回流：
  - 通用陷阱 → `docs/CONVENTIONS.md` 已知陷阱表
  - 工具 / 流程問題 → `docs/WORKFLOW.md` 或 `docs/AI-GUIDE.md`
  - 模式化解法 → `docs/patterns/`

## 模板

[`_template.md`](_template.md)
