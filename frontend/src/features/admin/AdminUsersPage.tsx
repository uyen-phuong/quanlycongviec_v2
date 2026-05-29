import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toApiError } from "@/shared/api/client";
import {
  useAdminDepartments,
  useAdminRoles,
  useAdminUsers,
  useChangeRole,
  useCreateUser,
  useResetPassword,
  useUpdateUser,
} from "@/features/admin/hooks";

import {
  changeRoleSchema,
  createUserSchema,
  resetPasswordSchema,
  updateUserSchema,
  type ChangeRoleValues,
  type CreateUserValues,
  type ResetPasswordValues,
  type UpdateUserValues,
} from "@/features/admin/schema";
import type { AdminUserListItemDto } from "@/features/admin/types";
import type { UsersFilter } from "@/features/admin/api";
import "@/features/admin/AdminPages.css";

const PAGE_SIZE = 20;

type DialogMode =
  | { type: "create" }
  | { type: "edit"; user: AdminUserListItemDto }
  | { type: "change-role"; user: AdminUserListItemDto }
  | { type: "reset-password"; user: AdminUserListItemDto }
  | null;

function formatDate(iso: string | null) {
  if (!iso) return "Chưa đăng nhập";
  return new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short" }).format(new Date(iso));
}

function CreateEditDialog({
  mode,
  filter,
  onClose,
}: {
  mode: { type: "create" } | { type: "edit"; user: AdminUserListItemDto };
  filter: UsersFilter;
  onClose: () => void;
}) {
  const rolesQuery = useAdminRoles();
  const departmentsQuery = useAdminDepartments();
  const createMutation = useCreateUser();
  const updateMutation = useUpdateUser(mode.type === "edit" ? mode.user.id : "", filter);
  const [serverError, setServerError] = useState<string | null>(null);

  const isEdit = mode.type === "edit";
  const defaultUser = isEdit ? mode.user : null;

  const createForm = useForm<CreateUserValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: {
      username: "",
      password: "",
      fullName: defaultUser?.fullName ?? "",
      email: defaultUser?.email ?? "",
      departmentId: defaultUser?.departmentId ?? "",
      roleId: defaultUser?.roleId ?? "",
      isActive: defaultUser?.isActive ?? true,
    },
  });

  const updateForm = useForm<UpdateUserValues>({
    resolver: zodResolver(updateUserSchema),
    defaultValues: {
      fullName: defaultUser?.fullName ?? "",
      email: defaultUser?.email ?? "",
      departmentId: defaultUser?.departmentId ?? "",
      isActive: defaultUser?.isActive ?? true,
    },
  });

  async function onSubmitCreate(values: CreateUserValues) {
    setServerError(null);
    try {
      await createMutation.mutateAsync(values);
      onClose();
    } catch (error) {
      setServerError(toApiError(error).message);
    }
  }

  async function onSubmitUpdate(values: UpdateUserValues) {
    setServerError(null);
    try {
      await updateMutation.mutateAsync(values);
      onClose();
    } catch (error) {
      setServerError(toApiError(error).message);
    }
  }

  const isPending = createMutation.isPending || updateMutation.isPending;

  return (
    <div className="admin-dialog-backdrop">
      <div className="admin-dialog">
        <div className="admin-dialog__header">
          <h2>{isEdit ? "Cập nhật người dùng" : "Tạo người dùng mới"}</h2>
          <button className="admin-dialog__close" onClick={onClose} type="button">×</button>
        </div>

        {isEdit ? (
          <form className="admin-dialog__form" onSubmit={updateForm.handleSubmit(onSubmitUpdate)}>
            <label className="admin-field">
              <span>Họ tên</span>
              <input {...updateForm.register("fullName")} className="admin-input" />
              {updateForm.formState.errors.fullName && (
                <span className="admin-field__error">{updateForm.formState.errors.fullName.message}</span>
              )}
            </label>
            <label className="admin-field">
              <span>Email</span>
              <input {...updateForm.register("email")} className="admin-input" type="email" />
            </label>
            <label className="admin-field">
              <span>Đơn vị</span>
              <select {...updateForm.register("departmentId")} className="admin-input">
                <option value="">-- Không có --</option>
                {(departmentsQuery.data ?? []).filter((d) => d.isActive).map((d) => (
                  <option key={d.id} value={d.id}>{d.name} ({d.code})</option>
                ))}
              </select>
            </label>
            <label className="admin-field admin-field--row">
              <input {...updateForm.register("isActive")} type="checkbox" />
              <span>Hoạt động</span>
            </label>
            {serverError && <p className="admin-field__error">{serverError}</p>}
            <div className="admin-dialog__footer">
              <button className="admin-btn admin-btn--ghost" onClick={onClose} type="button">Hủy</button>
              <button className="admin-btn admin-btn--primary" disabled={isPending} type="submit">
                {isPending ? "Đang lưu..." : "Lưu"}
              </button>
            </div>
          </form>
        ) : (
          <form className="admin-dialog__form" onSubmit={createForm.handleSubmit(onSubmitCreate)}>
            <label className="admin-field">
              <span>Tên đăng nhập</span>
              <input {...createForm.register("username")} className="admin-input" autoComplete="off" />
              {createForm.formState.errors.username && (
                <span className="admin-field__error">{createForm.formState.errors.username.message}</span>
              )}
            </label>
            <label className="admin-field">
              <span>Mật khẩu</span>
              <input {...createForm.register("password")} className="admin-input" type="password" autoComplete="new-password" />
              {createForm.formState.errors.password && (
                <span className="admin-field__error">{createForm.formState.errors.password.message}</span>
              )}
            </label>
            <label className="admin-field">
              <span>Họ tên</span>
              <input {...createForm.register("fullName")} className="admin-input" />
              {createForm.formState.errors.fullName && (
                <span className="admin-field__error">{createForm.formState.errors.fullName.message}</span>
              )}
            </label>
            <label className="admin-field">
              <span>Email</span>
              <input {...createForm.register("email")} className="admin-input" type="email" />
            </label>
            <label className="admin-field">
              <span>Đơn vị</span>
              <select {...createForm.register("departmentId")} className="admin-input">
                <option value="">-- Không có --</option>
                {(departmentsQuery.data ?? []).filter((d) => d.isActive).map((d) => (
                  <option key={d.id} value={d.id}>{d.name} ({d.code})</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Vai trò</span>
              <select {...createForm.register("roleId")} className="admin-input">
                <option value="">-- Chọn vai trò --</option>
                {(rolesQuery.data ?? []).map((r) => (
                  <option key={r.id} value={r.id}>{r.name} ({r.code})</option>
                ))}
              </select>
              {createForm.formState.errors.roleId && (
                <span className="admin-field__error">{createForm.formState.errors.roleId.message}</span>
              )}
            </label>
            <label className="admin-field admin-field--row">
              <input {...createForm.register("isActive")} type="checkbox" defaultChecked />
              <span>Hoạt động</span>
            </label>
            {serverError && <p className="admin-field__error">{serverError}</p>}
            <div className="admin-dialog__footer">
              <button className="admin-btn admin-btn--ghost" onClick={onClose} type="button">Hủy</button>
              <button className="admin-btn admin-btn--primary" disabled={isPending} type="submit">
                {isPending ? "Đang tạo..." : "Tạo mới"}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}

function ChangeRoleDialog({
  user,
  onClose,
}: {
  user: AdminUserListItemDto;
  onClose: () => void;
}) {
  const rolesQuery = useAdminRoles();
  const mutation = useChangeRole(user.id);
  const [serverError, setServerError] = useState<string | null>(null);

  const form = useForm<ChangeRoleValues>({
    resolver: zodResolver(changeRoleSchema),
    defaultValues: { roleId: user.roleId ?? "" },
  });

  async function onSubmit(values: ChangeRoleValues) {
    setServerError(null);
    try {
      await mutation.mutateAsync(values);
      onClose();
    } catch (error) {
      setServerError(toApiError(error).message);
    }
  }

  return (
    <div className="admin-dialog-backdrop">
      <div className="admin-dialog admin-dialog--sm">
        <div className="admin-dialog__header">
          <h2>Đổi vai trò — {user.fullName}</h2>
          <button className="admin-dialog__close" onClick={onClose} type="button">×</button>
        </div>
        <form className="admin-dialog__form" onSubmit={form.handleSubmit(onSubmit)}>
          <label className="admin-field">
            <span>Vai trò mới</span>
            <select {...form.register("roleId")} className="admin-input">
              <option value="">-- Chọn vai trò --</option>
              {(rolesQuery.data ?? []).map((r) => (
                <option key={r.id} value={r.id}>{r.name} ({r.code})</option>
              ))}
            </select>
            {form.formState.errors.roleId && (
              <span className="admin-field__error">{form.formState.errors.roleId.message}</span>
            )}
          </label>
          <p className="admin-field__hint">
            Access token cũ vẫn hiệu lực tối đa 15 phút sau khi đổi vai trò.
          </p>
          {serverError && <p className="admin-field__error">{serverError}</p>}
          <div className="admin-dialog__footer">
            <button className="admin-btn admin-btn--ghost" onClick={onClose} type="button">Hủy</button>
            <button className="admin-btn admin-btn--primary" disabled={mutation.isPending} type="submit">
              {mutation.isPending ? "Đang lưu..." : "Xác nhận"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

function ResetPasswordDialog({
  user,
  onClose,
}: {
  user: AdminUserListItemDto;
  onClose: () => void;
}) {
  const mutation = useResetPassword(user.id);
  const [serverError, setServerError] = useState<string | null>(null);

  const form = useForm<ResetPasswordValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { password: "", confirmPassword: "" },
  });

  async function onSubmit(values: ResetPasswordValues) {
    setServerError(null);
    try {
      await mutation.mutateAsync(values);
      onClose();
    } catch (error) {
      setServerError(toApiError(error).message);
    }
  }

  return (
    <div className="admin-dialog-backdrop">
      <div className="admin-dialog admin-dialog--sm">
        <div className="admin-dialog__header">
          <h2>Đặt lại mật khẩu — {user.fullName}</h2>
          <button className="admin-dialog__close" onClick={onClose} type="button">×</button>
        </div>
        <form className="admin-dialog__form" onSubmit={form.handleSubmit(onSubmit)}>
          <label className="admin-field">
            <span>Mật khẩu mới</span>
            <input {...form.register("password")} className="admin-input" type="password" autoComplete="new-password" />
            {form.formState.errors.password && (
              <span className="admin-field__error">{form.formState.errors.password.message}</span>
            )}
          </label>
          <label className="admin-field">
            <span>Xác nhận mật khẩu</span>
            <input {...form.register("confirmPassword")} className="admin-input" type="password" autoComplete="new-password" />
            {form.formState.errors.confirmPassword && (
              <span className="admin-field__error">{form.formState.errors.confirmPassword.message}</span>
            )}
          </label>
          {serverError && <p className="admin-field__error">{serverError}</p>}
          <div className="admin-dialog__footer">
            <button className="admin-btn admin-btn--ghost" onClick={onClose} type="button">Hủy</button>
            <button className="admin-btn admin-btn--primary" disabled={mutation.isPending} type="submit">
              {mutation.isPending ? "Đang lưu..." : "Đặt lại mật khẩu"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export function AdminUsersPage() {
  const [filter, setFilter] = useState<UsersFilter>({ page: 1, pageSize: PAGE_SIZE });
  const [keyword, setKeyword] = useState("");
  const [dialog, setDialog] = useState<DialogMode>(null);

  const usersQuery = useAdminUsers(filter);
  const rolesQuery = useAdminRoles();
  const departmentsQuery = useAdminDepartments();

  function applyKeyword() {
    setFilter((prev) => ({ ...prev, page: 1, keyword: keyword || undefined }));
  }

  function applyFilter(patch: Partial<UsersFilter>) {
    setFilter((prev) => ({ ...prev, page: 1, ...patch }));
  }

  const totalPages = usersQuery.data
    ? Math.ceil(usersQuery.data.meta.total / PAGE_SIZE)
    : 1;

  return (
    <div className="admin-page">
      <div className="admin-page__header">
        <h1>Quản trị người dùng</h1>
        <button className="admin-btn admin-btn--primary" onClick={() => setDialog({ type: "create" })} type="button">
          + Tạo người dùng
        </button>
      </div>

      <div className="admin-filters">
        <input
          className="admin-input admin-filters__search"
          onChange={(e) => setKeyword(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && applyKeyword()}
          placeholder="Tìm theo tên, username..."
          type="search"
          value={keyword}
        />
        <button className="admin-btn admin-btn--ghost" onClick={applyKeyword} type="button">Tìm</button>

        <select
          className="admin-input"
          onChange={(e) => applyFilter({ roleCode: e.target.value || undefined })}
        >
          <option value="">Tất cả vai trò</option>
          {(rolesQuery.data ?? []).map((r) => (
            <option key={r.id} value={r.code}>{r.name}</option>
          ))}
        </select>

        <select
          className="admin-input"
          onChange={(e) => applyFilter({ departmentId: e.target.value || undefined })}
        >
          <option value="">Tất cả đơn vị</option>
          {(departmentsQuery.data ?? []).map((d) => (
            <option key={d.id} value={d.id}>{d.name}</option>
          ))}
        </select>

        <select
          className="admin-input"
          onChange={(e) => applyFilter({
            isActive: e.target.value === "" ? undefined : e.target.value === "true",
          })}
        >
          <option value="">Tất cả trạng thái</option>
          <option value="true">Hoạt động</option>
          <option value="false">Bị khóa</option>
        </select>
      </div>

      {usersQuery.isLoading && <p className="admin-state">Đang tải...</p>}
      {usersQuery.isError && (
        <p className="admin-state admin-state--error">
          {toApiError(usersQuery.error).message}
        </p>
      )}

      {usersQuery.data && (
        <>
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Username</th>
                  <th>Họ tên</th>
                  <th>Đơn vị</th>
                  <th>Vai trò</th>
                  <th>Trạng thái</th>
                  <th>Đăng nhập cuối</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {usersQuery.data.items.map((user) => (
                  <tr key={user.id}>
                    <td className="admin-table__mono">{user.username}</td>
                    <td>{user.fullName}</td>
                    <td>{user.departmentCode ?? "—"}</td>
                    <td>{user.roleCode ?? "—"}</td>
                    <td>
                      <span className={`admin-badge ${user.isActive ? "admin-badge--active" : "admin-badge--inactive"}`}>
                        {user.isActive ? "Hoạt động" : "Bị khóa"}
                      </span>
                    </td>
                    <td>{formatDate(user.lastLoginAt)}</td>
                    <td>
                      <div className="admin-table__actions">
                        <button
                          className="admin-btn admin-btn--xs"
                          onClick={() => setDialog({ type: "edit", user })}
                          type="button"
                        >
                          Sửa
                        </button>
                        <button
                          className="admin-btn admin-btn--xs"
                          onClick={() => setDialog({ type: "change-role", user })}
                          type="button"
                        >
                          Đổi vai trò
                        </button>
                        <button
                          className="admin-btn admin-btn--xs admin-btn--danger"
                          onClick={() => setDialog({ type: "reset-password", user })}
                          type="button"
                        >
                          Đặt lại MK
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="admin-pagination">
            <span className="admin-pagination__info">
              Tổng {usersQuery.data.meta.total} người dùng
            </span>
            <div className="admin-pagination__controls">
              <button
                className="admin-btn admin-btn--ghost admin-btn--xs"
                disabled={filter.page <= 1}
                onClick={() => setFilter((p) => ({ ...p, page: p.page - 1 }))}
                type="button"
              >
                ‹ Trang trước
              </button>
              <span>{filter.page} / {totalPages}</span>
              <button
                className="admin-btn admin-btn--ghost admin-btn--xs"
                disabled={filter.page >= totalPages}
                onClick={() => setFilter((p) => ({ ...p, page: p.page + 1 }))}
                type="button"
              >
                Trang sau ›
              </button>
            </div>
          </div>
        </>
      )}

      {dialog?.type === "create" && (
        <CreateEditDialog filter={filter} mode={{ type: "create" }} onClose={() => setDialog(null)} />
      )}
      {dialog?.type === "edit" && (
        <CreateEditDialog filter={filter} mode={{ type: "edit", user: dialog.user }} onClose={() => setDialog(null)} />
      )}
      {dialog?.type === "change-role" && (
        <ChangeRoleDialog user={dialog.user} onClose={() => setDialog(null)} />
      )}
      {dialog?.type === "reset-password" && (
        <ResetPasswordDialog user={dialog.user} onClose={() => setDialog(null)} />
      )}
    </div>
  );
}
