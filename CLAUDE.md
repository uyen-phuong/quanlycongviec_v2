# CLAUDE.md
Tai lieu dieu phoi du an KHCT - thay the file Excel theo doi ke hoach cong tac cua khoi KTNB Agribank.
La nguon su that cho nghiep vu, kien truc, roadmap, quyet dinh da chot.
Code va tai lieu mau thuan: cap nhat tai lieu truoc.

## 1. Pham vi v1
Trong: auth, admin, plan main/sub, task tree, approval workflow, inherit main->sub, sync sub->main, import/export Excel, attachment, audit.
Ngoai: SSO, ky so, notification, mobile, realtime, khoi ngoai KTNB.

## 2. Tech stack
Backend: ASP.NET Core 8, EF Core 8 + Pomelo MySQL, MediatR CQRS, FluentValidation, Serilog, ClosedXML, BCrypt.
Frontend: React 19 + TS, Vite 8, TanStack Query, RHF + Zod, Tailwind + CSS rieng theo feature, axios, react-router v7.
DB/Deploy: MySQL 8 utf8mb4 `+07:00`, Docker Compose (`db`, `api`, `web`, `nginx`), upload local volume.

## 3. Kien truc
Backend: `KHCT.Domain` (entity/enum), `KHCT.Application` (CQRS/validator/DTO/interface), `KHCT.Infrastructure` (EF/auth/excel/file), `KHCT.Api` (controller/middleware), `KHCT.Tests`.
Frontend: `shared/api/` (client + envelope + DTO chung), `shared/auth/`, `features/<name>/` (Page + api.ts + hooks.ts + types.ts + schema.ts + components/), `layouts/` (AppShell + Sidebar maroon `#5C1F1F`), `app/` (routes + main).
Naming: DB `snake_case`, C# `PascalCase`, JSON/TS `camelCase`, URL `kebab-case`.

## 4. Trang thai
Backend buoc 18 + Phase 0 FE + Inherit + Sync.
**Inherit** ([InheritService.cs](backend/KHCT.Application/Plans/Workflow/InheritService.cs)): trigger main `approved_1 -> approved_2`, sinh sub plan `draft` + inherited tasks `is_locked=true`, audit `inherit_create` per sub plan.
**Sync** ([SyncService.cs](backend/KHCT.Application/Plans/Workflow/SyncService.cs)): trigger sub `approved_2 -> approved_3`, ghi de `progress_text/work_status/reason_not_completed` len main task qua `inherited_from_task_id`, audit `sync_from_sub` per task. Chan approve neu `sync_duplicate_source` hoac `sync_missing_target`.
Verification BE: `dotnet build` 0 warning, `dotnet test` 47/48 pass (1 fail Excel parser pre-existing, khong lien quan workflow).

FE Phase 1-6 da scaffold gan day du (xem §13.20-25):
- Phase 1 done: `shared/api/client.ts`, `shared/auth/AuthContext`, `app/routes.tsx` (RequireAuth/RequireRole), `layouts/{AppShell,Sidebar,Header}`, `features/auth/LoginPage`.
- Phase 2 done: `features/plan-tracking/` co `PlanTrackingPage` + `Toolbar/StatusBar/TaskTable/TaskRow/TaskHeaderRow/CommentDrawer/TaskDetailDrawer`, CSS rieng (`PlanTrackingPage.css`, `TaskTable.css`).
- Phase 3 done: `features/plans/` co `MainPlansPage`, `MainPlanDetailPage`, `PlanFormDialog`, `ReturnPlanDialog`, `ImportExcelDialog`. **Chua co `SubPlansPage` rieng** (sub plan vao qua `/plan-tracking/dept/:code`).
- Phase 4 done: import/export Excel qua `ImportExcelDialog` + nut export trong toolbar.
- Phase 5 done: `features/attachments/` co `AttachmentList`, `AttachmentUploader`, dung trong plan detail.
- Phase 6 done: `features/admin/` du 4 trang `AdminUsersPage`, `AdminDepartmentsPage`, `AdminRolesPage`, `AdminApprovalConfigsPage`.

Khac biet FE hien tai vs HTML mock `TheoDoiTienDoThucHienCV.html` (tab "Cong viec phong" = `#departmentPage`):
- ✅ Toolbar 6 nut thang hang khong tach `.ctb-left`; mau: Upload/Xuat/PheDuyet `btn-g` xanh la, Luu `btn-b` xanh duong, ChuyenTra `btn-o` cam, KiemSoat `btn-r` tim. Month picker dat o `.shd-month` trong section header (canh badge `.sbg`), goi `handleMonthChange` push len URL `?month=YYYY-MM`.
- ❌ Bang giu 14 cot `min-width:2520px` (TT, Noi dung, BKS, KTNB lead, Loai CV, Don vi, Han, Tien do, Trang thai, Nguoi nhap, Kiem soat, Phe duyet, Nguyen nhan, Ghi chu, Xoa). KHONG thu gon ve 12 cot nhu mock vi can giu day du field nghiep vu (`ktnb_leader_text`, `work_type`, `work_status`). Cot "Loai cong viec" editable o sub plan (select 3 option), readonly o main plan (force "Cong viec chung").
- ✅ Cot "Tien do thuc hien" da dung pattern compact: `.xcell` voi `.xpreview-text` (3-line clamp) + nut `.xbtn` "Xem chi tiet" mo `DetailEditorModal` (textarea full + nut `btn-g` "Xac nhan"). Readonly khi khong co quyen edit. Cot "Nguyen nhan" van dung textarea inline (chua chuyen sang xcell - lam khi can).
- ✅ Unit picker da dong bo voi mock: dung modal `UnitPickerModal` voi `.units-grid` + `.upill` pill. Phong dau moi = `mode="single"` (click pill = chon + dong); Phong phoi hop = `mode="multi"` (toggle pill, dong manual). Da xoa `.picker-wrap`/`.picker-popover`/`.picker-select`.
- ✅ Cac phan da giong mock: `.shd` (chu do uppercase + gradient line + badge), `.c-content.l2/.l3` indent, `.inp-sm` (focus do), hover row `#fdf9f9`, row open-comment `#fff6bf`, header row `#f5f5f5 + uppercase + border-top 2px`, sticky thead.

## 5. Migration & data model
Migration: `20260513015845_Initial`, `20260513032909_AdminStep7`, `20260513073707_TaskOwnerDepartmentAndInheritedTasks`, `20260513091852_TaskNoteTextAndMainPlanImport`, `20260515034854_TaskKtnbLeaderText`. `db/schema_mysql.sql` phai sync.

Bang nghiep vu: `department`, `role`, `app_user` (DB ten la `app_user`), `user_role`, `approval_config`, `bks_member`, `plan`, `task`, `task_supporting_dept`, `approval_history`, `line_comment`, `attachment`, `audit_log`. Bang auth: `refresh_token`. `audit_log` khong co FK cung sang entity nghiep vu.

Phong: `KTNB1/2/3`, `KH`, `GS`, `VPMN`, `VPMT`, `VPTNB`, `TKTH`.
Role: `ADMIN`, `VAN_THU`, `TRUONG_KH`, `TRUONG_KTNB`, `PHO_TRUONG_KTNB`, `TRUONG_PHONG`, `TRUONG_NHOM`, `NHAN_VIEN`. 1 user = 1 role hoat dong. `approval_config` la nguon su that cho assignment nguoi duyet.

## 6. Auth & admin
Auth: access token 15p, refresh 7 ngay luu SHA-256 hash, cookie refresh HttpOnly, user inactive bi chan login/refresh. Doi role/deactivate user: access token cu con hieu luc toi 15p.
Admin (`/api/admin/*` role `ADMIN` only): username `trim + lowercase`, email optional khong unique, role static catalog, department khong hard delete + `code` immutable, khong cho deactivate/demote admin cuoi cung.

## 7. Plan
Main: `scope=main`, `department_id=null`, mutate `VAN_THU,ADMIN`, unique `(scope, department_id, year, month)`.
Sub: `scope=sub`, bat buoc `department_id`, mutate `TRUONG_PHONG` (phong minh) hoac `PHO_TRUONG_KTNB` (cross-dept).
Read sub: see-all `ADMIN/TRUONG_KTNB/PHO_TRUONG_KTNB/TRUONG_KH/VAN_THU`, dept-scoped `TRUONG_PHONG/NHAN_VIEN`. Ngoai whitelist: 403 `forbidden_role`. Ngoai scope by id: 404.
CRUD: update khi `draft|returned`, delete hard chi khi `draft` va chua co task (co task: `plan_has_tasks`). DELETE tra `{ "data": true }`.

## 8. Task
API: `GET /api/plans/{planId}/tasks` (flat list), `GET/POST/PUT/DELETE /api/tasks/{id}`.
Cau truc: `parentTaskId` + `displayOrder` cho move/reorder, bat ky task lam parent, FE build tree, backend trust `displayOrder` khong auto reindex.

Field nghiep vu:
- `work_type`: 0=chung (general), 1=rieng (private/specific), 2=ca nhan (personal). Backend enum: `WorkType.General=0, WorkType.Group=1, WorkType.Personal=2` (C# ten noi bo, KHONG doi); FE label hien thi: "Cong viec chung / Cong viec rieng / Cong viec ca nhan"
- `is_header=true`: dong nhom
- `owner_department_id`: phong dau moi cua task main
- `bks_member_text` (max 255): thanh vien BKS chi dao
- `ktnb_leader_text` (max 255): lanh dao KTNB chi dao
- `note_text`: text mo ta, import map cot Han hoan thanh
- `inherited_from_task_id`: map task inherited, `is_locked=true` -> read-only

Rule:
- Main list force `work_type=0`
- Header normalize clear: `deadline, assigneeUserId, ownerDepartmentId, bksMemberText, ktnbLeaderText, noteText, progressText, reasonNotCompleted, supportingDepartmentIds`
- `work_type=1/2` force `bksMemberText=null`
- Main task `work_type=0 + is_header=false` bat buoc co `owner_department_id`
- Sub task `owner_department_id=null`
- Task locked khong mutate; delete task co children: `task_has_children`
- `deadline >= plan.created_at.Date`; overdue bat buoc `reason_not_completed`
- Parent cung plan; move khong tao cycle (`task_cycle`)
- Supporting dept phai active; sub plan khong cho supporting trung `plan.department_id`

Quyen:
- Main mutate: `VAN_THU,ADMIN`
- Sub full update/create/delete: `TRUONG_PHONG` cung phong hoac `PHO_TRUONG_KTNB`
- `NHAN_VIEN`: khong create/delete; chi update 3 field trong sub plan phong minh: `workStatus, progressText, reasonNotCompleted`. Doi field khac: 403 `forbidden_field_change`.

## 9. Approval, inherit, sync
Approval o cap plan (khong cap task).
Main flow: `draft -> pending -> approved_1 -> approved_2 -> returned`. Submit `VAN_THU`; approve: `TRUONG_KH` (->1), `TRUONG_KTNB` (->2).
Sub flow: `draft -> pending -> approved_1 -> approved_2 -> approved_3 -> returned`. Submit `TRUONG_PHONG` (cung phong) hoac `PHO_TRUONG_KTNB`; approve: `TRUONG_NHOM` (->1), `TRUONG_PHONG` (->2), `PHO_TRUONG_KTNB` (->3).
Return: bat buoc line comment theo task -> `has_open_comment=true`. Resolve het -> clear. Resolve comment: main `VAN_THU|ADMIN`; sub `PHO_TRUONG_KTNB` hoac role cung phong `TRUONG_PHONG|NHAN_VIEN`.

Inherit (trigger main `approved_1 -> approved_2`): chon leaf `General` (`is_header=false, work_type=0, owner_department_id!=null`), gom header ancestor theo phong, tim/tao sub plan `draft` `(department, year, month)`, tao inherited task `is_locked=true, inherited_from_task_id`. Task `Group(1)`/`Personal(2)` khong sinh sub plan. Khong copy `supportingDepartmentIds`. Main task general thieu owner: chan `approved_2`.

Sync (trigger sub `approved_2 -> approved_3`): dua vao `sub_task.inherited_from_task_id -> main_task.id`, moi main task 1 nguon. Ghi de hoan toan: `progress_text, work_status, reason_not_completed`. Audit `sync_from_sub` co `subPlanId, subTaskId, inheritedFromTaskId`. Missing target/duplicate source: chan approval.

## 10. Excel, attachment, audit
Excel import (`POST /api/plans/main/{id}/import-excel`): chi `.xlsx` <=10MB, vao main plan `draft|returned`, sheet 0, header row 3 data row 4. Parse `outline_index` cot A, fallback token dau cot B. Replace-all trong transaction sau validate. Khong map `owner_department_id`/`deadline`. Co downstream inherited: `plan_has_downstream`. Audit `import_excel`.
Map import: C->`bksMemberText`, D->`ktnbLeaderText` (vi tri TBD theo phu luc thuc), E->`noteText`, F->`progressText`, K->`reasonNotCompleted`.

Excel export (`GET /api/plans/main/{id}/export-excel`): workbook 2 sheet `Bao cao` + `Chi tiet`, chi task `work_type=0` main plan, tinh than Phu luc 03 (khong clone 100%).
Map export: `deadline->Han hoan thanh, bks_member_text->Thanh vien BKS chi dao, ktnb_leader_text->Lanh dao KTNB chi dao, progress_text->Tien do thuc hien, reason_not_completed->Nguyen nhan chua hoan thanh, note_text->Ghi chu`.

Attachment: owner type `plan|task`. Storage local `storage/attachments/yyyy/MM/{guid}.{ext}`, DB luu `stored_path` relative. Delete: xoa DB row truoc, file best-effort. Download khong audit. Create/delete co audit `attachment`. Limit 50MB enforce 2 lop (Kestrel + endpoint). Whitelist: `.pdf .doc .docx .xls .xlsx .ppt .pptx .txt .csv .png .jpg .jpeg .gif .zip .rar .7z`. Magic bytes check moi extension tru `.txt/.csv`.
Quyen attachment: bam theo mutate owner. Sub task `is_locked=true` van duoc upload neu user co quyen mutate task. Sub `NHAN_VIEN` cung phong duoc upload theo nhanh progress-only.

Audit auto (DbContext.SaveChanges): CRUD cho `plan, task, user, department, approval_config, attachment`. Khong auto: `refresh_token, audit_log, approval_history, line_comment, user_role, task_supporting_dept`. Exclude `password_hash, created_at, updated_at`. Special audit thu cong: `submit, approve, return, sync_from_sub, import_excel, inherit_create, change_role, reset_password`. Neu request da them `AuditLog` thu cong -> auto audit bo qua toan bo `SaveChanges` do. Task snapshot kem `supportingDepartmentIds`.

## 11. API chinh
Auth: `POST /api/auth/{login,refresh,logout}`, `GET /api/auth/me`.
Admin: `GET/POST/PUT /api/admin/users[/{id}/{role,password}]`, `GET/PUT /api/admin/departments`, `GET /api/admin/roles`, `GET/PUT /api/admin/approval-configs`.
Plan: `GET/POST/PUT/DELETE /api/plans/{main,sub}`, `GET /api/plans/resolve?scope&departmentCode&year&month` (resolver), `POST /api/plans/{id}/{submit,approve,return}`, `GET /api/plans/{id}/{approval-history,line-comments}`, `POST /api/line-comments/{id}/resolve`, `POST /api/plans/main/{id}/import-excel`, `GET /api/plans/main/{id}/export-excel`.
Task: `GET /api/plans/{planId}/tasks`, `GET/POST/PUT/DELETE /api/tasks/{id}`.
Attachment: `POST/GET /api/{plans,tasks}/{id}/attachments`, `GET /api/attachments/{id}/download`, `DELETE /api/attachments/{id}`.
Swagger dev-only: Bearer scheme, tags theo controller, summary cho workflow endpoints.

## 12. Frontend conventions
Quy tac tach:
- Component (.tsx) chi JSX + state cuc bo; KHONG goi axios truc tiep.
- api.ts: 1 function = 1 endpoint, tra Promise<T>.
- hooks.ts: wrap api.ts bang `useQuery`/`useMutation`; component chi import hooks.
- types.ts mirror C# DTO 1-1 (camelCase), sync thu cong.
- schema.ts Zod cho form, suy type `z.infer<>`.
- Tailwind 90% style; file `.css` rieng khi can animation/scrollbar/grid phuc tap.

Routing:
- `/login` public; route guard `<RequireAuth>` + `<RequireRole>`.
- `/plan-tracking/dept/:deptCode` dung resolver suy ra planId (default thang hien tai).
- `/plans/main/:id`, `/plans/sub/:id` explicit theo scope.
- `/admin/*` role `ADMIN`.

Auth flow FE:
1. Login -> accessToken trong memory (Context) + axios header.
2. Refresh token o cookie HttpOnly `khct_refresh`.
3. App load: POST `/api/auth/refresh` (cookie tu gui) -> nhan accessToken + user. KHONG goi `/me` o boot vi `/me` la [Authorize].
4. Axios 401 -> auto refresh + retry 1 lan; fail -> redirect /login.

Autosave task (critical):
- Backend `EnsureProgressOnlyPayload` so toan bo field voi DB.
- FE phai gui FULL SNAPSHOT cua row (merge edit vao snapshot), KHONG patch chi 3 field, neu khong se 403 `forbidden_field_change`.

Validation FE bam backend:
- `deadline >= plan.createdAt.Date` (fetch `plan.createdAt` cung task list -> input min).
- Attachment <=50MB, excel import <=10MB, whitelist extension match backend.

Khong lam o v1: realtime, mobile, i18n, SSR, codegen TS, Storybook, E2E test, trang `/attachments`+`/history` toan cuc (an khoi sidebar -> v2), virtualize bang task.

## 13. Roadmap
1-18. Backend done (clean arch, auth, admin, plan, task, approval, inherit, sync, excel, attachment, audit hook, tests baseline, swagger cleanup).

19. Phase 0 backend (prerequisite cho FE, da xong trong code):
    - Them field `KtnbLeaderText` (max 255) vao entity `Task` (`backend/KHCT.Domain/Entities/Task.cs`).
    - Cau hinh cot `ktnb_leader_text` (`TaskConfiguration`).
    - Them vao `TaskListItemDto`, `TaskDetailDto`, `CreateTaskRequest`, `SaveTaskRequest`.
    - Validator: max 255; header normalize clear; `TaskSupport.EnsureProgressOnlyPayload` them vao danh sach bat bien.
    - Map import (`MainPlanExcelImportService`) + map export (`MainPlanExcelExportService`).
    - `dotnet ef migrations add TaskKtnbLeaderText` + cap nhat `db/schema_mysql.sql`.
    - Tao `ResolvePlanQuery` + endpoint `GET /api/plans/resolve?scope&departmentCode&year&month`, tai dung `PlanSupport.ApplySubReadScope`.
    - Verify: build 0 warning, test 59/59 pass, swagger ra field + endpoint moi.

20. Phase 1 FE foundation:
    - `shared/api/client.ts` axios instance + Bearer header + response interceptor 401 -> auto refresh + retry 1 lan.
    - `shared/api/envelope.ts` `ApiEnvelope<T>`, `ApiError`, `PagedMeta`.
    - `shared/api/dtos.ts` DTO dung chung: `UserDto, DepartmentDto, RoleDto`, enum `ApprovalStatus, WorkType, WorkStatus`.
    - `shared/auth/AuthContext` luu accessToken in-memory + user; boot goi `/api/auth/refresh`.
    - `layouts/AppShell + Sidebar + Header` mau `#5C1F1F`, menu theo wireframe (HE THONG, GIAM SAT, phong, ca nhan, dang xuat).
    - `app/routes.tsx` voi `<RequireAuth>` + `<RequireRole>`.
    - `features/auth/LoginPage` form RHF + Zod, goi `POST /api/auth/login`.
    - Verify: `npm run dev`, login `admin/Admin@123`, vao shell, navigate qua route placeholder khong loi console.

21. Phase 2 FE plan-tracking (man chinh):
    - `features/plan-tracking/PlanTrackingPage` voi `MonthPicker` -> goi `GET /api/plans/resolve` -> nhan `planId`.
    - `Toolbar` 6 nut (Upload Excel, Xuat Excel, Luu du lieu, Chuyen tra, Kiem soat, Phe duyet), enable theo role + plan status.
    - `StatusBar` hien pill status + vai tro.
    - `TaskTable` du 15 cot phu luc 03 (TT, noi dung, BKS, KTNB lead, loai cong viec, don vi thuc hien + supporting, han hoan thanh, tien do, trang thai, nguoi nhap, kiem soat, phe duyet, nguyen nhan, ghi chu, xoa). Scroll ngang. KHONG virtualize.
    - Inline edit cac field theo role; `useSaveTask` mutation gui FULL SNAPSHOT (merge edit vao row hien tai), debounce 800ms, optimistic update.
    - Popover "+ Them" cho supporting depts.
    - `CommentDrawer` hien line comment + lich su; resolve comment cho VAN_THU/TRUONG_PHONG.
    - `useTasks(planId)` TanStack Query `staleTime: 30s`.
    - Verify: nhap row -> refresh trang -> data persist. NHAN_VIEN chi sua duoc 3 field; sua field khac -> backend tra 403 -> FE rollback + toast.

22. Phase 3 FE plan CRUD + workflow:
    - `MainPlansPage`, `SubPlansPage` list + filter `year/month/status[/departmentId]`, pagination.
    - `PlanFormDialog` create/edit (year + month [+ dept cho sub]).
    - `MainPlanDetailPage` `/plans/main/:id`, `SubPlanDetailPage` `/plans/sub/:id` tai su dung `TaskTable` lam man hinh trung tam; metadata/workflow/history la lop thong tin bao quanh.
    - Submit/Approve action: dialog optional comment.
    - Return action: modal chon NHIEU task trong 1 lan, moi task co comment rieng (>=1 theo validator backend).
    - `useApprovalHistory(planId)` timeline.
    - Verify: chain `draft -> submit -> approved_1 -> approved_2` main; sub `draft -> ... -> approved_3` -> sync ve main task.

23. Phase 4 FE import/export Excel:
    - `ImportExcelDialog`: drop zone `.xlsx`, validate FE <=10MB, hien progress, hien ket qua import (so task tao).
    - `ExportExcelButton`: tai file workbook (axios `responseType: 'blob'` -> tao link tai).
    - Verify: import file phu luc 03 -> task hien dung; export -> file co 2 sheet + data khop.

24. Phase 5 FE attachment:
    - `AttachmentList` + `AttachmentUploader` dung trong drawer plan/task.
    - Validate FE: <=50MB, whitelist extension match backend.
    - Download qua `GET /api/attachments/{id}/download` -> blob -> save.
    - Verify: upload + list + download bit-perfect + delete tren ca plan va task.

25. Phase 6 FE admin:
    - `AdminUsersPage`: list + filter (dept/role/active/keyword) + pagination + create/edit/change-role/reset-password.
    - `AdminDepartmentsPage`: list + edit name/active.
    - `AdminRolesPage`: read-only list.
    - `AdminApprovalConfigsPage`: edit theo scope/level.
    - Verify: tao user moi, doi role, deactivate -> user moi login duoc/bi chan dung quy tac.

## 14. Nguyen tac
- Khong hardcode nguoi duyet; `approval_config` la nguon su that.
- Khong dua task loai 1/2 len tab to.
- Khong dat approval status tren task.
- Khong tron excel/file storage vao controller.
- Khong bo audit cho thay doi quan trong.
- Uu tien dung nghiep vu truoc toi uu UI/performance.

## 15. Integration test baseline
Stack: `WebApplicationFactory` + SQLite in-memory + HTTP call that.
Cover: auth login/me/inactive, plan/task CRUD + permission `NHAN_VIEN`, workflow main+sub day du tu draft den approved_3 + sync, attachment upload/list/download/delete/reject ext sai.
Khong thay the smoke MySQL that. Chua cover het import/export layout, CORS preflight, matrix phan quyen day du.
