export interface AdminUserListItemDto {
  id: string;
  username: string;
  fullName: string;
  email: string | null;
  isActive: boolean;
  lastLoginAt: string | null;
  lastLogoutAt: string | null;
  departmentId: string | null;
  departmentCode: string | null;
  departmentName: string | null;
  positionId: string | null;
  positionCode: string | null;
  positionName: string | null;
  roleId: string | null;
  roleCode: string | null;
  roleName: string | null;
}

export interface AdminUserDetailDto extends AdminUserListItemDto {
  createdAt: string;
  updatedAt: string;
}

export interface AdminRoleDto {
  id: string;
  code: string;
  name: string;
}

export interface AdminDepartmentDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface AdminPositionDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  sortOrder: number;
}

export interface AdminApprovalConfigDto {
  id: string;
  scope: string;
  level: number;
  departmentId: string | null;
  departmentCode: string | null;
  departmentName: string | null;
  roleId: string;
  roleCode: string;
  roleName: string;
  isActive: boolean;
}
