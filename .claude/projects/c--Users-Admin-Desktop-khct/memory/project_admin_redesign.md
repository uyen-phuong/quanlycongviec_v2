---
name: project-admin-redesign
description: Admin section redesign - new Position entity, user fields, config tabs, permission matrix
metadata:
  type: project
---

Đã hoàn thành redesign module Quản trị hệ thống (2026-06-01).

**Why:** User yêu cầu chuyên nghiệp hóa admin section theo PRD mới, thêm Chức vụ (Position), bỏ chấm điểm cá nhân.

**Thay đổi Backend:**
- Entity mới: `Position` (code, name, isActive, sortOrder) với table `position`
- User entity: thêm `LastLogoutAt`, `PositionId` FK
- Migration: `20260531165533_AdminPositionAndUserFields` - đã apply vào DB
- Logout handler: ghi `LastLogoutAt` khi revoke refresh token
- Seed data: 8 positions + 3 departments mới (LDKTNB, BKS, VL) + role names theo PRD
- API: `/api/admin/positions` (CRUD), `/api/admin/departments` (thêm POST)

**Thay đổi Frontend:**
- Xóa: `personal-evaluation` khỏi routes, sidebar
- Xóa: `PERSONAL_EVAL_ROLES` từ roles.ts
- Sidebar redesign: collapsible admin section với icons, user avatar ở footer
- 3 trang admin mới: `AdminConfigPage` (tab Phòng Ban/Chức vụ/Vai trò), `AdminPermissionMatrixPage`
- `AdminUsersPage`: bảng mới với cột Position, LastLogout, Online/Offline badge
- Routes: `/admin/config`, `/admin/permissions` thay cho `/admin/departments`, `/admin/roles`

**How to apply:** Xem files trong `frontend/src/features/admin/` và `backend/KHCT.Application/Admin/`
