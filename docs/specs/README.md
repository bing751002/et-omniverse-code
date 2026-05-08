# Specs

Feature specifications。一個 feature 一個 `F-XXX-*.md`。

> **模組子目錄暫未劃分** — 先全部平鋪在 `specs/` 下。等 spec 累積到一定數量、模組邊界清楚後再決定怎麼分（按業務模組 / 按 7-step / 按其他維度）。

## 編號規則

- `F-XXX` 全域連號，三位數（F-001, F-002, ...）
- 新 spec 編號：先 `grep -r "^id: F-" .` 取最大值 +1

## Status 流轉

| Status | 意義 | 何時改 |
|---|---|---|
| `draft` | 草稿，未審 | 剛起草、訪談中 |
| `blocked` | 等訪談 / 等決策 | 有 open question 卡關 |
| `approved` | 通過 review，可開工 | plan 完、ADR 對齊 |
| `implementing` | 實作中 | execute-phase 進行 |
| `implemented` | 已上線 | verify-work 通過、ship |
| `modifying` | 既有 spec 在改 | insert-phase 開始時 |
| `deprecated` | 已棄用 | 被新 F-YYY supersedes |

詳細流轉規則見 [`../WORKFLOW.md`](../WORKFLOW.md)。

## Frontmatter 欄位

```yaml
---
id: F-XXX
title: <一句話描述>
module:               # 暫留空 — 模組劃分未定
status: draft
owner: <github handle>
created: YYYY-MM-DD
updated: YYYY-MM-DD
supersedes:           # 可選：取代了哪個舊 F-XXX
superseded-by:        # 可選：被哪個新 F-YYY 取代
related-adr: []       # 可選：相關 D-XX
related-interview: [] # 可選：相關訪談檔名（不含副檔名）
phase:                # 可選：對應 GSD phase 編號
---
```

## 模板

[`_template.md`](_template.md)
