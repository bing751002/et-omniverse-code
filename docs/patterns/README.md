# Patterns

可複用的實作模式。

## 何時建 pattern

- **第二次用同個寫法** → 抽 `<name>.md`（沿用 [`../WORKFLOW.md`](../WORKFLOW.md) 規則）
- 第一次寫不要急著抽（避免過度設計）

## 紀律

- pattern 範例不寫太長（>20 行請改連結到 repo 內檔）
- 每個 pattern 必含「何時用 / 何時不用」
- 新發明 pattern 前先 `ls docs/patterns/` 看是否已有
- pattern 對應 ADR 的話標出 `D-XX`（例如 outbox pattern ↔ 某個事件處理決策）

## 模板

[`_template.md`](_template.md)
