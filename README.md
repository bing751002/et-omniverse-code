# ET-Omniverse

東森媒體 7-step 內容排播平台 — 取代 SharePoint Excel 共編、加上 AI VCR 製作能力。內部 ~50 人使用、on-prem 單機部署、modular monolith。

## 階段

**P1.0 foundation 已開工**。目前已建立初始 solution / backend projects / frontend shell / docker / ci skeleton；業務功能尚未實作。

## 入口（依序讀）

1. **`docs/ARCHITECTURE.md`** — 技術棧、repo 結構、模組
2. **`docs/INFRA.md`** — Docker Compose、環境設定、CI/CD 分階段規則
3. **`docs/ACCESS-CONTROL.md`** — 公司 / 事業群 / 部門 / 外部公司權限模型
4. **`docs/DECISIONS.md`** — 重大決策摘要（D10-D17）
5. **`docs/CONVENTIONS.md`** — 命名、code style、git、已知陷阱
6. **`docs/WORKFLOW.md`** — SDD / ADR / 訪談 / PR 紀律
7. **`docs/DOCUMENTATION.md`** — spec vs KB、code 後文件回寫規範
8. **`docs/GLOSSARY.md`** — 業務 / 技術詞彙表
9. **`docs/AI-GUIDE.md`** — 用 AI 工具開發時的紅線與紀律（可選讀）

## 工具入口

不論用什麼 AI 工具，**規範都在 `docs/`**：

| 工具 | 入口 | 內容 |
|---|---|---|
| Claude Code | `CLAUDE.md` | thin pointer → docs/ |
| Codex / Cursor | `AGENTS.md` | thin pointer → docs/ |
| 不用 AI | 直接讀 `docs/` | 同上 |

## 規劃文件

規劃期文件在使用者個人空間，尚未進 repo。寫 spec / code 前若需要範圍或功能清單，向使用者要對應檔。

## License

Internal — 公司專案。
