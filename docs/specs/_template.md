---
id: F-XXX
title: <一句話描述>
module: <module name>
status: draft
owner: <github handle>
created: YYYY-MM-DD
updated: YYYY-MM-DD
supersedes:
superseded-by:
related-adr: []
related-interview: []
phase:
---

# F-XXX：<title>

## 業務背景

<為什麼要做這個 / 解決什麼問題 / 對應 7-step 哪一步>

## 用戶故事

1. As a <role>, I want <action>, so that <outcome>
2. ...

## 範圍

### In scope
- ...

### Out of scope
- ...

## 驗收條件

- [ ] <可驗證行為 1> — 對應測試：<unit | integration | api | e2e>
- [ ] <可驗證行為 2>
- [ ] ...

## 實作連結（完工後填）

- Endpoint：`src/backend/ETOmniverse.Api/Features/<Feature>/...`
- UseCase：`src/backend/ETOmniverse.Domain/<Module>/UseCase/...`
- Port：`src/backend/ETOmniverse.Domain/<Module>/Ports/...`
- Migration：`src/backend/ETOmniverse.Infrastructure/Migrations/...`
- 主要 PR：#XXX

## Open questions

- [ ] Q-XXX-001: ...

## 變更記錄

| 日期 | 變更 | PR |
|---|---|---|
| YYYY-MM-DD | 初版 | #XX |
