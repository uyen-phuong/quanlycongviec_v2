import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { adminApi, type UsersFilter } from "@/features/admin/api";
import type { AdminUserListItemDto } from "@/features/admin/types";
import type { ChangeRoleValues, CreateUserValues, ResetPasswordValues, UpdateUserValues } from "@/features/admin/schema";

export const adminKeys = {
  users: (filter: UsersFilter) => ["admin", "users", filter] as const,
  roles: ["admin", "roles"] as const,
  departments: ["admin", "departments"] as const,
  approvalConfigs: ["admin", "approval-configs"] as const,
};

export function useAdminUsers(filter: UsersFilter) {
  return useQuery({
    queryKey: adminKeys.users(filter),
    queryFn: () => adminApi.listUsers(filter),
    staleTime: 30 * 1000,
  });
}

export function useAdminRoles() {
  return useQuery({
    queryKey: adminKeys.roles,
    queryFn: adminApi.listRoles,
    staleTime: 5 * 60 * 1000,
  });
}

export function useAdminDepartments() {
  return useQuery({
    queryKey: adminKeys.departments,
    queryFn: adminApi.listDepartments,
    staleTime: 5 * 60 * 1000,
  });
}

export function useAdminApprovalConfigs() {
  return useQuery({
    queryKey: adminKeys.approvalConfigs,
    queryFn: adminApi.listApprovalConfigs,
    staleTime: 30 * 1000,
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: CreateUserValues) => adminApi.createUser(values),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["admin", "users"] });
    },
  });
}

export function useUpdateUser(userId: string, filter: UsersFilter) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: UpdateUserValues) => adminApi.updateUser(userId, values),
    onSuccess: (updated) => {
      queryClient.setQueryData<{ items: AdminUserListItemDto[]; meta: unknown }>(
        adminKeys.users(filter),
        (prev) =>
          prev
            ? { ...prev, items: prev.items.map((u) => (u.id === updated.id ? updated : u)) }
            : prev,
      );
    },
  });
}

export function useChangeRole(userId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: ChangeRoleValues) => adminApi.changeRole(userId, values),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["admin", "users"] });
    },
  });
}

export function useResetPassword(userId: string) {
  return useMutation({
    mutationFn: (values: ResetPasswordValues) => adminApi.resetPassword(userId, values),
  });
}

export function useUpdateDepartment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, name, isActive }: { id: string; name: string; isActive: boolean }) =>
      adminApi.updateDepartment(id, name, isActive),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminKeys.departments });
    },
  });
}

export function useUpdateApprovalConfig() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      departmentId,
      roleId,
      isActive,
    }: {
      id: string;
      departmentId: string | null;
      roleId: string;
      isActive: boolean;
    }) => adminApi.updateApprovalConfig(id, { departmentId, roleId, isActive }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: adminKeys.approvalConfigs });
    },
  });
}
