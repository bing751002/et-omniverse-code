# Glossary

> 專案術語對照。AI / 新人容易誤翻或搞混的詞集中這裡。

## 業務術語

| 中文 | 英文 / Code | 說明 | 別誤翻 / 別誤用 |
|---|---|---|---|
| 批次 | Batch | 一個檔期的編排容器（含 7 step） | ❌ batch job ✅ batch / campaign |
| 檔期 | Campaign Period | 業務週期單位（某週某主題） | ❌ schedule slot ✅ campaign period |
| Step | Step | 7-step 中的某一步（Step 0~6） | 直接用 Step，不譯 |
| 分眾 | Audience Segment | 受眾分組（由大數據算） | 不要用 Group / Cohort |
| 受眾 | Audience | 目標客群 | — |
| VCR | VCR | 影片內容（短影音） | ❌ 錄影機（VCR 縮寫源自影片產業，不是電器） |
| AI VCR | AI VCR | AI 產製的 VCR | — |
| 排播 | Schedule（編排）+ Dispatch（派報） | 編排 + 派報合稱 | 視情境拆 |
| 派報 | Dispatch | 把排好的內容送出 | — |
| 廣宣 | Marketing | 行銷推廣（含簡訊發送） | — |
| 商品池 | Product Pool | 候選商品集合 | — |
| 挑品 | Pick | MD 從商品池選 | — |
| 主檔 | Master Record | VCR 多版本中選定的版本 | — |
| 口白 | Narration | AI 產製的影片旁白 | — |

## 角色

| 角色 | 說明 | 階段 |
|---|---|---|
| MD | 商品部，負責挑品、節目編排 | 全程 |
| 節目部 | 編排節目（D11 後不填分眾） | 全程 |
| 行銷 | 設定行銷連結、廣宣 | 全程 |
| 排播 | 排程、派報 | 後段 |
| 廣宣 | 簡訊設定 + 發送 | Step 6 |
| Admin / IT | 平台管理 | 全程 |
| 大數據 | 受眾分析（外部，非平台用戶） | 外部 |

## 技術術語

| 縮寫 | 全名 | 場景 |
|---|---|---|
| RBAC | Role-Based Access Control | Auth / 權限 |
| ADR | Architecture Decision Record | 決策紀錄 |
| SDD | Spec-Driven Development | 開發流程 |
| Onion / Hexagonal | DDD onion architecture / Ports & Adapters | 後端架構 |
| Modular Monolith | 模組化單體 | 部署形態 |
| EFK | Elasticsearch + Fluent Bit + Kibana | log stack |
| APM | Application Performance Monitoring | 觀測 |
| OTel | OpenTelemetry | 觀測標準 |
| OpenAPI | API 規格標準 | API 自動產 client |

## 外部服務

| 服務 | 用途 | 階段 |
|---|---|---|
| kie.ai | AI VCR 產製（4 個 engine：sora2 / kling3 / seedance2 / wan27）| Phase 1 |
| Gemini | 口白文字生成 | Phase 1 |
| 大數據受眾 | 受眾 segment 計算（內部服務） | Phase 1 |
| 派報自動化 | 派報觸發（內部服務） | Phase 1 |
| SMTP | Email 通知 | Phase 1 |
| Fugo | 復購服務（簡訊發送） | **Phase 2** |
| AD / LDAP | 公司目錄服務 | **Phase 2** |

## 模組命名（跟 Step 對齊）

| 模組 | 對應 Step | 用途 |
|---|---|---|
| Identity | — | Auth + RBAC + scoped org permission |
| BatchWorkspace | — | 批次容器 |
| ProductSchedule | Step 0 | 商品排播 |
| MdPicks | Step 1 | MD 挑品 |
| Audience | Step 2 | 受眾（read-only） |
| AiVcr | Step 3 | AI VCR ★ |
| MarketingLink | Step 4 | 行銷連結 |
| Schedule | Step 5 | 排播派報 |
| Sms | Step 6 | 簡訊（read-only Phase 1） |
| Collaboration | — | SignalR 共編 |
| Notification | — | 站內 + Email |
| Audit | — | Audit log |

## 命名規則速查

- **C# class / method / property**：PascalCase（`LoginUseCase`、`GetById`、`UserId`）
- **C# local / param**：camelCase（`userId`）
- **TS var / function**：camelCase（`useUser`）
- **TS component / type**：PascalCase（`BatchCard.vue`、`type User`）
- **DB table**：snake_case 複數（`users`、`batches`）
- **DB column**：snake_case（`user_id`、`created_at`）
- **API endpoint**：kebab-case 複數（`/api/batches`、`/api/ai-vcr-tasks`）
