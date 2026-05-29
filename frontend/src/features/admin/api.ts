import { apiClient } from "@/shared/api/client";
import type { PagedMeta } from "@/shared/api/dtos";
import type {
  AdminApprovalConfigDto,
  AdminDepartmentDto,
  AdminRoleDto,
  AdminUserDetailDto,
  AdminUserListItemDto,
} from "@/features/admin/types";
import type {
  ChangeRoleValues,
  CreateUserValues,
  ResetPasswordValues,
  UpdateUserValues,
} from "@/features/admin/schema";

export interface UsersFilter {
  page: number;
  pageSize: number;
  departmentId?: string;
  roleCode?: string;
  isActive?: boolean;
  keyword?: string;
}

export interface PagedUsers {
  items: AdminUserListItemDto[];
  meta: PagedMeta;
}

export const adminApi = {
  // Users
  listUsers: async (filter: UsersFilter): Promise<PagedUsers> => {
    const params: Record<string, string | number | boolean> = {
      page: filter.page,
      pageSize: filter.pageSize,
    };

    if (filter.departmentId) params.departmentId = filter.departmentId;
    if (filter.roleCode) params.roleCode = filter.roleCode;
    if (filter.isActive !== undefined) params.isActive = filter.isActive;
    if (filter.keyword) params.keyword = filter.keyword;

    const res = await apiClient.get<{ data: AdminUserListItemDto[]; meta: PagedMeta }>(
      "/admin/users",
      { params },
    );
    return { items: res.data.data, meta: res.data.meta! };
  },

  getUser: async (id: string): Promise<AdminUserDetailDto> => {
    const res = await apiClient.get<{ data: AdminUserDetailDto }>(`/admin/users/${id}`);
    return res.data.data;
  },

  createUser: async (values: CreateUserValues): Promise<AdminUserDetailDto> => {
    const res = await apiClient.post<{ data: AdminUserDetailDto }>("/admin/users", {
      username: values.username,
      password: values.password,
      fullName: values.fullName,
      email: values.email || null,
      departmentId: values.departmentId || null,
      roleId: values.roleId,
      isActive: values.isActive,
    });
    return res.data.data;
  },

  updateUser: async (id: string, values: UpdateUserValues): Promise<AdminUserDetailDto> => {
    const res = await apiClient.put<{ data: AdminUserDetailDto }>(`/admin/users/${id}`, {
      fullName: values.fullName,
      email: values.email || null,
      departmentId: values.departmentId || null,
      isActive: values.isActive,
    });
    return res.data.data;
  },

  changeRole: async (id: string, values: ChangeRoleValues): Promise<AdminUserDetailDto> => {
    const res = await apiClient.put<{ data: AdminUserDetailDto }>(
      `/admin/users/${id}/role`,
      { roleId: values.roleId },
    );
    return res.data.data;
  },

  resetPassword: async (id: string, values: ResetPasswordValues): Promise<void> => {
    await apiClient.put(`/admin/users/${id}/password`, { password: values.password });
  },

  // Roles
  listRoles: async (): Promise<AdminRoleDto[]> => {
    const res = await apiClient.get<{ data: AdminRoleDto[] }>("/admin/roles");
    return res.data.data;
  },

  // Departments
  listDepartments: async (): Promise<AdminDepartmentDto[]> => {
    const res = await apiClient.get<{ data: AdminDepartmentDto[] }>("/admin/departments");
    return res.data.data;
  },

  updateDepartment: async (id: string, name: string, isActive: boolean): Promise<AdminDepartmentDto> => {
    const res = await apiClient.put<{ data: AdminDepartmentDto }>(
      `/admin/departments/${id}`,
      { name, isActive },
    );
    return res.data.data;
  },

  // Approval configs
  listApprovalConfigs: async (): Promise<AdminApprovalConfigDto[]> => {
    const res = await apiClient.get<{ data: AdminApprovalConfigDto[] }>("/admin/approval-configs");
    return res.data.data;
  },

  updateApprovalConfig: async (
    id: string,
    payload: { departmentId: string | null; roleId: string; isActive: boolean },
  ): Promise<AdminApprovalConfigDto> => {
    const res = await apiClient.put<{ data: AdminApprovalConfigDto }>(
      `/admin/approval-configs/${id}`,
      payload,
    );
    return res.data.data;
  },
};
