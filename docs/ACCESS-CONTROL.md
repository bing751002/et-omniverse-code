# Access Control

> 白話版權限設計。目的不是先把資料表寫死，而是讓人和 agent 對「公司 / 事業群 / 部門 / 外部公司 / 批次權限」有同一套理解。

## 一句話

ET-Omniverse 的權限不要設計成「部門 = 角色」。

Phase 1 先用簡化版：**功能面 RBAC + 事業群 scope**。也就是先管「誰能用哪些功能」和「能看哪個事業群資料」。細到部門、單一 batch 分享、外部公司協作的權限先預留資料結構與 feature flag，不一開始啟用。

```text
Phase 1 default:
user has role permission
AND user belongs to batch.ownerOrgUnitId
```

未來要更細時，再打開 scoped grants。

## Phase 1 簡化版

先不要把權限做到最細。最小可用版：

```text
User
OrgUnit              # 先放事業群：東森購物 / 東森寵物 / 東森新聞
UserOrgMembership   # 使用者屬於哪些事業群
Role
Permission
RolePermission
UserRole            # Phase 1 先不帶 scope，或 scope 固定由 UserOrgMembership 決定
```

角色用功能命名：

```text
Admin
BatchViewer
BatchEditor
ProductEditor
VcrEditor
ScheduleEditor
Auditor
```

Batch 保留：

```text
Batch
- ownerOrgUnitId
```

Phase 1 權限判斷：

```text
CanReadBatch(user, batch)
= user has batch.read
AND user belongs to batch.ownerOrgUnitId
```

```text
CanEditBatch(user, batch)
= user has batch.edit
AND user belongs to batch.ownerOrgUnitId
```

這樣已經可以做到：

```text
東森購物的人只看東森購物 batch
東森寵物的人只看東森寵物 batch
主管如果同時屬於東森購物和東森寵物，就能看兩邊
有 product.edit 的人才可以改商品
有 schedule.edit 的人才可以改排播
```

Phase 1 先不處理：

```text
某個東購 batch 特別開給東寵看
某個 batch 只給東購商品部看
外部公司只看某個 batch
單筆 VCR / 檔案分享
部門級 visibility
```

## 預留升級路徑

即使 Phase 1 簡化，也先保留這些概念，不一定啟用：

```text
ownerOrgUnitId       # 現在就用
visibilityMode       # 可先不出現在 UI，DB 可晚點加
RoleAssignment scope # 可從 UserRole 升級
batchAccessGrants    # 有跨事業群 / 外部公司需求時再加
```

升級順序建議：

```text
Level 0: 功能面 RBAC + 事業群隔離
Level 1: RoleAssignment 加 OrgUnit scope
Level 2: Batch visibilityMode = OwnerOrg / Restricted
Level 3: batchAccessGrants 支援跨事業群 / 外部公司
Level 4: Resource-level grant 支援單筆 VCR / 檔案 / 任務分享
```

## Feature Flag 策略

權限細化要用 feature flag 漸進打開，不要一次切全系統。

建議 flags：

```text
AccessControl:ScopedRoleAssignments=false
AccessControl:RestrictedBatchVisibility=false
AccessControl:BatchAccessGrants=false
AccessControl:ExternalCollaborators=false
AccessControl:ResourceLevelGrants=false
```

預設行為：

```text
ScopedRoleAssignments=false:
  使用 UserRole + UserOrgMembership 判斷，不看 RoleAssignment scope。

RestrictedBatchVisibility=false:
  所有 Batch 視為 OwnerOrg visibility。

BatchAccessGrants=false:
  不讀 batch_access_grants。

ExternalCollaborators=false:
  外部 org / external partner 帳號不能登入或不能取得業務資料。

ResourceLevelGrants=false:
  不支援單筆 VCR / 檔案 / 任務分享。
```

打開順序：

```text
1. ScopedRoleAssignments
2. RestrictedBatchVisibility
3. BatchAccessGrants
4. ExternalCollaborators
5. ResourceLevelGrants
```

每打開一個 flag，都要有 migration / seed / tests / rollback 說明。

正確拆法是：

```text
人在哪個組織節點下
+ 拿到什麼角色
+ 這個角色在哪個範圍有效
+ 目標資料目前狀態是否允許操作
= 最終能不能做某件事
```

也就是：

```text
OrgUnit tree + Role + Permission + Scoped RoleAssignment + Resource Policy
```

## 為什麼不能只用部門當角色

如果把部門直接做成角色，很快會爆炸。

例如：

```text
東森購物商品部編輯者
東森寵物商品部編輯者
東森新聞商品部編輯者
東森購物排播部編輯者
東森寵物排播部編輯者
外部廠商A某批次觀看者
```

事業群、部門、外部公司、流程步驟一多，角色數量會失控，而且很多名字其實只是 scope 不同。

比較穩的設計是：

```text
role = ProductEditor
scope = 東森購物 / 商品部
```

同一個角色可以套在不同 scope 上，不需要發明一堆角色名稱。

## 組織樹：OrgUnit

公司、事業群、部門、團隊都用同一種節點表示，形成一棵樹。

```text
OrgUnit
- id
- parentId
- type: Group / Company / BusinessGroup / Department / Team / ExternalCompany
- name
- status
```

範例：

```text
東森集團
├── 東森購物
│   ├── 商品部
│   ├── 電視部
│   └── 排播部
├── 東森寵物
│   ├── 商品部
│   └── 行銷部
├── 東森新聞
│   ├── 編輯部
│   └── 排播部
└── 外部合作公司 A
    └── 專案窗口
```

這裡有一個重點：不同事業群底下可以都有「商品部」或「排播部」，但它們不是同一個部門。

```text
東森購物 / 商品部 != 東森寵物 / 商品部
東森購物 / 排播部 != 東森新聞 / 排播部
```

## 使用者歸屬：Membership

一個人可能有多個歸屬。

```text
UserOrgMembership
- userId
- orgUnitId
- membershipType: Employee / Contractor / ExternalPartner / Vendor
- title
- isPrimary
- startsAt
- expiresAt nullable
- status
```

範例：

```text
王小明
- 主要歸屬：東森購物 / 商品部
- 身分：Employee

陳主管
- 主要歸屬：東森購物
- 身分：Employee

Amy
- 主要歸屬：外部合作公司 A / 專案窗口
- 身分：ExternalPartner
- expiresAt: 2026-12-31
```

Membership 只描述「這個人是哪裡的人」，不直接代表他能做什麼。

## 角色與權限：Role / Permission

Permission 是最小能力，Role 是 Permission 的集合。

```text
Permission examples
- batch.read
- batch.create
- product.edit
- md_picks.submit
- vcr.upload
- vcr.generate_ai
- schedule.edit
- schedule.publish
- sms.preview
- sms.dispatch
- audit.read
- user.manage
- role.assign
```

常見 Role：

```text
SystemAdmin
TenantAdmin
BusinessGroupAdmin
ProductEditor
ProductViewer
TvEditor
ScheduleEditor
VcrEditor
MarketingEditor
Viewer
Auditor
ExternalContributor
```

範例：

```text
ProductEditor
- batch.read
- product.edit
- md_picks.submit

ScheduleEditor
- batch.read
- schedule.edit
- schedule.publish

ExternalContributor
- batch.read
- vcr.upload
```

Role 不要包含「東森購物」或「商品部」這種 scope 字眼。

## 角色指派：RoleAssignment

真正給權限時，要把 role 指派到某個 scope。

```text
RoleAssignment
- userId
- roleId
- scopeType: System / OrgUnit / Batch / Resource
- scopeId
- startsAt
- expiresAt nullable
- grantedBy
```

範例：

```text
王小明
- role: ProductEditor
- scope: 東森購物 / 商品部

李小華
- role: ProductViewer
- scope: 東森寵物 / 商品部

陳主管
- role: BusinessGroupAdmin
- scope: 東森購物

Amy
- role: ExternalContributor
- scope: Batch 2026-W20
- expiresAt: 2026-06-30
```

這樣「同名部門」不會混在一起，外部公司也不會被塞進東森內部部門。

## Scope 涵蓋規則

Scope 是一棵樹，所以要有「涵蓋」概念。

```text
如果 role scope = 東森購物
可以涵蓋：
- 東森購物 / 商品部
- 東森購物 / 電視部
- 東森購物 / 排播部

如果 role scope = 東森購物 / 商品部
只涵蓋：
- 東森購物 / 商品部

不涵蓋：
- 東森寵物 / 商品部
- 東森新聞 / 商品部
- 東森購物 / 排播部
```

白話：

```text
上層 scope 可以看下層。
下層 scope 不會自動看上層或平行部門。
```

## 資料要帶 Scope

權限能不能判斷，取決於資料本身有沒有 scope。

主要 business table 至少要能追到：

```text
organization / business group / department / batch
```

概念欄位：

```text
Batch
- id
- ownerOrgUnitId        # 例如 東森購物
- period
- status

ProductPick
- id
- batchId
- ownerOrgUnitId        # 例如 東森購物 / 商品部

ScheduleItem
- id
- batchId
- ownerOrgUnitId        # 例如 東森購物 / 排播部

VcrAsset
- id
- batchId
- ownerOrgUnitId
- createdByUserId
```

外部合作資料不要混進內部部門。外部人可透過 Batch / Resource scope 被授權。

## 權限判斷公式

每個操作都用同一種思路：

```text
CanDo(user, action, resource)
= user has permission for action
AND role assignment scope covers resource scope
AND resource status allows action
AND optional resource ACL allows action
```

例如商品編輯：

```text
CanEditProduct(user, product)
= user has product.edit
AND assignment.scope contains product.ownerOrgUnitId
AND product.batch.status allows product editing
```

例如排播發布：

```text
CanPublishSchedule(user, schedule)
= user has schedule.publish
AND assignment.scope contains schedule.ownerOrgUnitId
AND schedule.batch.status is ready for publish
```

例如外部廠商上傳 VCR：

```text
CanUploadVcr(user, batch)
= user has vcr.upload
AND assignment.scope is this batch or contains this batch owner scope
AND assignment is not expired
AND batch.status allows VCR upload
```

## 具體例子

### 例 1：東森購物商品部可以改自己事業群商品

```text
User: 王小明
Membership: 東森購物 / 商品部
RoleAssignment:
- ProductEditor @ 東森購物 / 商品部

Resource:
- ProductPick A
- ownerOrgUnitId = 東森購物 / 商品部
```

結果：

```text
王小明可以 edit ProductPick A。
```

但如果資料是：

```text
ProductPick B
- ownerOrgUnitId = 東森寵物 / 商品部
```

結果：

```text
王小明不能 edit ProductPick B。
```

因為「東森購物 / 商品部」不涵蓋「東森寵物 / 商品部」。

### 例 2：同樣叫商品部，權限不互通

```text
東森購物 / 商品部
東森寵物 / 商品部
```

它們只是名字一樣，OrgUnit id 不一樣。

```text
李小華 = ProductViewer @ 東森寵物 / 商品部
```

李小華可以看東森寵物商品資料，但不能看東森購物商品資料，除非另有授權。

### 例 3：事業群主管可以看下層部門

```text
User: 陳主管
RoleAssignment:
- BusinessGroupAdmin @ 東森購物
```

如果 `BusinessGroupAdmin` 包含：

```text
batch.read
audit.read
role.assign_limited
```

那陳主管可以看：

```text
東森購物 / 商品部
東森購物 / 電視部
東森購物 / 排播部
```

但不會自動看：

```text
東森寵物
東森新聞
外部合作公司 A
```

### 例 4：排播部只改排播，不改商品

```text
User: 張排播
Membership: 東森購物 / 排播部
RoleAssignment:
- ScheduleEditor @ 東森購物 / 排播部
```

可以：

```text
schedule.edit
schedule.publish
```

不能：

```text
product.edit
md_picks.submit
vcr.generate_ai
```

原因是 role 沒有那些 permission，不是因為他不是商品部。

### 例 5：外部公司只能進指定批次

```text
User: Amy
Membership: 外部合作公司 A / 專案窗口
RoleAssignment:
- ExternalContributor @ Batch 2026-W20
- expiresAt = 2026-06-30
```

Amy 可以：

```text
看 Batch 2026-W20
上傳指定 VCR asset
```

Amy 不能：

```text
看其他 batch
看東森購物所有資料
看東森寵物資料
管理使用者
```

外部人預設應該什麼都看不到，只能靠明確授權打開。

### 例 6：臨時支援跨事業群

```text
User: 王小明
原本:
- ProductEditor @ 東森購物 / 商品部

臨時支援:
- ProductEditor @ 東森寵物 / 商品部
- expiresAt = 2026-07-31
```

這時不用新增 role，只要新增一筆帶期限的 RoleAssignment。

到期後自動失效。

## 完整版建議做到哪裡

完整權限模型要先支援未來擴充，但不要一開始就全部啟用。

完整版表：

```text
User
OrgUnit
UserOrgMembership
Role
Permission
RolePermission
RoleAssignment
```

Phase 1 可以先用 `UserRole` 取代 `RoleAssignment`，但程式的 authorization service 要留介面，未來能切到 scoped RoleAssignment。

先不做：

```text
自助申請權限
複雜審批流程
外部公司自行管理員工
動態 permission builder UI
AD / LDAP / SSO
多 tenant 獨立 DB
```

Phase 1 可以 local user store，但欄位要預留：

```text
User
- authProvider: Local / Ldap / Oidc
- externalSubjectId nullable
```

未來接 AD / SSO 時，不需要重做權限模型。

## Admin UI 應該長怎樣

管理權限時，不要讓管理者選一堆技術 permission。

比較好的操作語言：

```text
把「王小明」設定成「ProductEditor」
範圍是「東森購物 / 商品部」
有效期限「無」
```

外部合作：

```text
邀請「Amy」
公司「外部合作公司 A」
角色「ExternalContributor」
範圍「Batch 2026-W20」
有效期限「2026-06-30」
```

系統背後再轉成 RoleAssignment。

## 安全紅線

- 外部使用者預設沒有任何資料權限。
- Role 不包含公司、事業群、部門名稱；那些是 scope。
- 權限判斷不能只看前端顯示，後端每個 command/query 都要檢查。
- 查 list API 時也要套 scope filter，不能只在 detail API 檢查。
- 已到期的 RoleAssignment 一律無效。
- 未來若啟用 vector DB，payload 若放 business metadata，也必須遵守 MSSQL 的權限 scope；不可直接讓前端查 vector DB。

## 開放問題

- 「東森購物 / 東森寵物 / 東森新聞」在正式組織架構上是 Company、BusinessGroup，還是 OrgUnit type 需要另命名？
- 外部公司是只協作單一批次，還是未來可能成為獨立租戶？
- 事業群主管是否能管理下層部門使用者，或只能看資料？
- 外部合作是否需要審批流程，還是由內部管理者直接指派？
