# Architecture Decision Records

每個重大決策一個 `D-XX-*.md`。

## 編號規則

- `D-XX` 全域連號（D-01, D-02, ...）
- D-01 ~ D-16 已散在規劃文件，未來搬入時保留原編號
- **新決策從 D-17 起**
- 寫新 ADR 前：先 `grep -r "^id: D-" .` + 看 [`../DECISIONS.md`](../DECISIONS.md) 摘要表，取最大值 +1

## ADR 紀律

- **Accepted 不改寫**。要變更 → 開新 ADR：
  - 新 ADR frontmatter 加 `supersedes: D-XX`
  - 舊 ADR frontmatter 加 `superseded-by: D-YY`，status 改 `superseded`
- ADR 完成後同步更新 [`../DECISIONS.md`](../DECISIONS.md) 摘要表（一行）
- 寫 ADR 前先看摘要表是否衝突

## Status

| Status | 意義 |
|---|---|
| `proposed` | 提案中，討論未定 |
| `accepted` | 通過，生效中 |
| `superseded` | 被新 ADR 取代 |
| `deprecated` | 廢棄但無取代者 |

## 模板

[`_template.md`](_template.md)
