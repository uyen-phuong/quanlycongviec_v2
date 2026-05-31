import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toApiError } from "@/shared/api/client";
import {
  useAdminDepartments,
  useAdminPositions,
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

function fmtDate(iso: string | null) {
  if (!iso) return "—";
  return new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short" }).format(new Date(iso));
}

function isOnline(lastLoginAt: string | null, lastLogoutAt: string | null): boolean {
  if (!lastLoginAt) return false;
  if (!lastLogoutAt) return true;
  return new Date(lastLoginAt) > new Date(lastLogoutAt);
}

function OnlineBadge({ online }: { online: boolean }) {
  return (
    <span className={`usr-online-badge ${online ? "usr-online-badge--on" : "usr-online-badge--off"}`}>
      <span className="usr-online-badge__dot" />
      {online ? "Online" : "Offline"}
    </span>
  );
}

function ActiveBadge({ active }: { active: boolean }) {
  return (
    <span className={`admin-badge ${active ? "admin-badge--active" : "admin-badge--inactive"}`}>
      {active ? "Hoạt động" : "Bị khóa"}
    </span>
  );
}

function UserFormDialog({
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
  const positionsQuery = useAdminPositions();
  const createMutation = useCreateUser();
  const updateMutation = useUpdateUser(mode.type === "edit" ? mode.user.id : "", filter);
  const [serverError, setServerError] = useState<string | null>(null);

  const isEdit = mode.type === "edit";
  const user = isEdit ? mode.user : null;

  const createForm = useForm<CreateUserValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: {
      username: "",
      password: "",
      fullName: "",
      email: "",
      departmentId: "",
      positionId: "",
      roleId: "",
      isActive: true,
    },
  });

  const editForm = useForm<UpdateUserValues>({
    resolver: zodResolver(updateUserSchema),
    defaultValues: {
      fullName: user?.fullName ?? "",
      email: user?.email ?? "",
      departmentId: user?.departmentId ?? "",
      positionId: user?.positionId ?? "",
      isActive: user?.isActive ?? true,
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

  async function onSubmitEdit(values: UpdateUserValues) {
    setServerError(null);
    try {
      await updateMutation.mutateAsync(values);
      onClose();
    } catch (error) {
      setServerError(toApiError(error).message);
    }
  }

  const isPending = createMutation.isPending || updateMutation.isPending;

  if (isEdit) {
    return (
      <div className="admin-dialog-backdrop">
        <div className="admin-dialog">
          <div className="admin-dialog__header">
            <h2>Chỉnh sửa: {user?.fullName}</h2>
            <button className="admin-dialog__close" onClick={onClose} type="button">×</button>
          </div>
          <form className="admin-dialog__form" onSubmit={editForm.handleSubmit(onSubmitEdit)}>
            <div className="admin-form-row">
              <label className="admin-field">
                <span>Họ và tên <span className="req">*</span></span>
                <input {...editForm.register("fullName")} className="admin-input" />
                {editForm.formState.errors.fullName && (
                  <span className="admin-field__error">{editForm.formState.errors.fullName.message}</span>
                )}
              </label>
              <label className="admin-field">
                <span>Email</span>
                <input {...editForm.register("email")} className="admin-input" type="email" />
              </label>
            </div>
            <div className="admin-form-row">
              <label className="admin-field">
                <span>Phòng ban</span>
                <select {...editForm.register("departmentId")} className="admin-input">
                  <option value="">— Không có —</option>
                  {(departmentsQuery.data ?? []).filter((d) => d.isActive).map((d) => (
                    <option key={d.id} value={d.id}>{d.name} ({d.code})</option>
                  ))}
                </select>
              </label>
              <label className="admin-field">
                <span>Chức vụ</span>
                <select {...editForm.register("positionId")} className="admin-input">
                  <option value="">— Không có —</option>
                  {(positionsQuery.data ?? []).filter((p) => p.isActive).map((p) => (
                    <option key={p.id} value={p.id}>{p.name}</option>
                  ))}
                </select>
              </label>
            </div>
            <label className="admin-field admin-field--row">
              <input {...editForm.register("isActive")} type="checkbox" />
              <span>Tài khoản hoạt động (Active)</span>
            </label>
            {serverError && <p className="admin-field__error">{serverError}</p>}
            <div className="admin-dialog__footer">
              <button className="admin-btn admin-btn--ghost" onClick={onClose} type="button">Hủy</button>
              <button className="admin-btn admin-btn--primary" disabled={isPending} type="submit">
                {isPending ? "Đang lưu..." : "Lưu thay đổi"}
              </button>
            </div>
          </form>
        </div>
      </div>
    );
  }

  return (
    <div className="admin-dialog-backdrop">
      <div className="admin-dialog">
        <div className="admin-dialog__header">
          <h2>Tạo người dùng mới</h2>
          <button className="admin-dialog__close" onClick={onClose} type="button">×</button>
        </div>
        <form className="admin-dialog__form" onSubmit={createForm.handleSubmit(onSubmitCreate)}>
          <div className="admin-form-row">
            <label className="admin-field">
              <span>Tên đăng nhập <span className="req">*</span></span>
              <input {...createForm.register("username")} className="admin-input" autoComplete="off" />
              {createForm.formState.errors.username && (
                <span className="admin-field__error">{createForm.formState.errors.username.message}</span>
              )}
            </label>
            <label className="admin-field">
              <span>Mật khẩu <span className="req">*</span></span>
              <input {...createForm.register("password")} className="admin-input" type="password" autoComplete="new-password" />
              {createForm.formState.errors.password && (
                <span className="admin-field__error">{createForm.formState.errors.password.message}</span>
              )}
            </label>
          </div>
          <div className="admin-form-row">
            <label className="admin-field">
              <span>Họ và tên <span className="req">*</span></span>
              <input {...createForm.register("fullName")} className="admin-input" />
              {createForm.formState.errors.fullName && (
                <span className="admin-field__error">{createForm.formState.errors.fullName.message}</span>
              )}
            </label>
            <label className="admin-field">
              <span>Email</span>
              <input {...createForm.register("email")} className="admin-input" type="email" />
            </label>
          </div>
          <div className="admin-form-row">
            <label className="admin-field">
              <span>Phòng ban</span>
              <select {...createForm.register("departmentId")} className="admin-input">
                <option value="">— Không có —</option>
                {(departmentsQuery.data ?? []).filter((d) => d.isActive).map((d) => (
                  <option key={d.id} value={d.id}>{d.name} ({d.code})</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Chức vụ</span>
              <select {...createForm.register("positionId")} className="admin-input">
                <option value="">— Không có —</option>
                {(positionsQuery.data ?? []).filter((p) => p.isActive).map((p) => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
            </label>
          </div>
          <label className="admin-field">
            <span>Vai trò <span className="req">*</span></span>
            <select {...createForm.register("roleId")} className="admin-input">
              <option value="">— Chọn vai trò —</option>
              {(rolesQuery.data ?? []).map((r) => (
                <option key={r.id} value={r.id}>{r.name}</option>
              ))}
            </select>
            {createForm.formState.errors.roleId && (
              <span className="admin-field__error">{createForm.formState.errors.roleId.message}</span>
            )}
          </label>
          <label className="admin-field admin-field--row">
            <input {...createForm.register("isActive")} type="checkbox" defaultChecked />
            <span>Tài khoản hoạt động (Active)</span>
          </label>
          {serverError && <p className="admin-field__error">{serverError}</p>}
          <div className="admin-dialog__footer">
            <button className="admin-btn admin-btn--ghost" onClick={onClose} type="button">Hủy</button>
            <button className="admin-btn admin-btn--primary" disabled={isPending} type="submit">
              {isPending ? "Đang tạo..." : "Tạo người dùng"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

function ChangeRoleDialog({ user, onClose }: { user: AdminUserListItemDto; onClose: () => void }) {
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
              <option value="">— Chọn vai trò —</option>
              {(rolesQuery.data ?? []).map((r) => (
                <option key={r.id} value={r.id}>{r.name}</option>
              ))}
            </select>
            {form.formState.errors.roleId && (
              <span className="admin-field__error">{form.formState.errors.roleId.message}</span>
            )}
          </label>
          <p className="admin-field__hint">Access token cũ vẫn hiệu lực tối đa 15 phút sau khi đổi vai trò.</p>
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

function ResetPasswordDialog({ user, onClose }: { user: AdminUserListItemDto; onClose: () => void }) {
  const mutation = useResetPassword(user.id);
  const [serverError, setServerError] = useState<string | null>(null);
  const [done, setDone] = useState(false);

  const form = useForm<ResetPasswordValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { password: "", confirmPassword: "" },
  });

  async function onSubmit(values: ResetPasswordValues) {
    setServerError(null);
    try {
      await mutation.mutateAsync(values);
      setDone(true);
    } catch (error) {
      setServerError(toApiError(error).message);
    }
  }

  if (done) {
    return (
      <div className="admin-dialog-backdrop">
        <div className="admin-dialog admin-dialog--sm">
          <div className="admin-dialog__header">
            <h2>Đặt lại mật khẩu thành công</h2>
            <button className="admin-dialog__close" onClick={onClose} type="button">×</button>
          </div>
          <div className="admin-dialog__form">
            <p style={{ color: "#2e7d32", fontSize: "13px" }}>✓ Mật khẩu của <strong>{user.fullName}</strong> đã được đặt lại.</p>
          </div>
          <div className="admin-dialog__footer">
            <button className="admin-btn admin-btn--primary" onClick={onClose} type="button">Đóng</button>
          </div>
        </div>
      </div>
    );
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

  const items = usersQuery.data?.items ?? [];
  const meta = usersQuery.data?.meta;
  const totalPages = meta ? Math.ceil(meta.total / PAGE_SIZE) : 1;

  return (
    <div className="admin-page">
      <div className="admin-page__header">
        <div>
          <h1>Quản trị người dùng</h1>
          <p className="admin-page__subtitle">
            Quản lý tài khoản, vai trò và trạng thái hoạt động của toàn bộ người dùng trong hệ thống.
          </p>
        </div>
        <button className="admin-btn admin-btn--primary" onClick={() => setDialog({ type: "create" })} type="button">
          + Tạo người dùng
        </button>
      </div>

      {/* Filter bar */}
      <div className="admin-filter-bar">
        <div className="admin-filter-bar__search">
          <input
            className="admin-input"
            onChange={(e) => setKeyword(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && applyKeyword()}
            placeholder="Tìm theo tên, username, email..."
            type="search"
            value={keyword}
          />
          <button className="admin-btn" onClick={applyKeyword} type="button">Tìm</button>
        </div>
        <div className="admin-filter-bar__selects">
          <select
            className="admin-input"
            value={filter.roleCode ?? ""}
            onChange={(e) => applyFilter({ roleCode: e.target.value || undefined })}
          >
            <option value="">Tất cả vai trò</option>
            {(rolesQuery.data ?? []).map((r) => (
              <option key={r.id} value={r.code}>{r.name}</option>
            ))}
          </select>
          <select
            className="admin-input"
            value={filter.departmentId ?? ""}
            onChange={(e) => applyFilter({ departmentId: e.target.value || undefined })}
          >
            <option value="">Tất cả phòng ban</option>
            {(departmentsQuery.data ?? []).map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </select>
          <select
            className="admin-input"
            value={filter.isActive === undefined ? "" : filter.isActive ? "true" : "false"}
            onChange={(e) => applyFilter({
              isActive: e.target.value === "" ? undefined : e.target.value === "true",
            })}
          >
            <option value="">Tất cả trạng thái</option>
            <option value="true">Đang hoạt động</option>
            <option value="false">Đã khóa</option>
          </select>
          <button
            className="admin-btn admin-btn--ghost"
            onClick={() => { setKeyword(""); setFilter({ page: 1, pageSize: PAGE_SIZE }); }}
            type="button"
          >
            Xóa bộ lọc
          </button>
        </div>
      </div>

      {/* Stats row */}
      {meta && (
        <div className="admin-stats-row">
          <div className="admin-stat">
            <span className="admin-stat__num">{meta.total}</span>
            <span className="admin-stat__label">Tổng người dùng</span>
          </div>
          <div className="admin-stat">
            <span className="admin-stat__num" style={{ color: "#2e7d32" }}>
              {items.filter((u) => u.isActive).length}
            </span>
            <span className="admin-stat__label">Đang hoạt động</span>
          </div>
          <div className="admin-stat">
            <span className="admin-stat__num" style={{ color: "#10b981" }}>
              {items.filter((u) => isOnline(u.lastLoginAt, u.lastLogoutAt)).length}
            </span>
            <span className="admin-stat__label">Đang online</span>
          </div>
        </div>
      )}

      {/* Table */}
      {usersQuery.isError ? (
        <p className="admin-state admin-state--error">{toApiError(usersQuery.error).message}</p>
      ) : (
        <div className="admin-table-wrapper">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Username</th>
                <th>Họ và tên</th>
                <th>Email</th>
                <th>Phòng ban</th>
                <th>Chức vụ</th>
                <th>Vai trò</th>
                <th>Kết nối</th>
                <th>Đăng nhập cuối</th>
                <th>Đăng xuất cuối</th>
                <th>Tình trạng</th>
                <th style={{ width: 160 }}>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {usersQuery.isLoading ? (
                <tr>
                  <td colSpan={11} className="admin-state">Đang tải...</td>
                </tr>
              ) : items.length === 0 ? (
                <tr>
                  <td colSpan={11} className="admin-state">Không có dữ liệu phù hợp.</td>
                </tr>
              ) : (
                items.map((u) => (
                  <tr key={u.id}>
                    <td className="admin-table__mono">{u.username}</td>
                    <td style={{ fontWeight: 600 }}>{u.fullName}</td>
                    <td className="admin-table__dim">{u.email ?? "—"}</td>
                    <td>{u.departmentName ? `${u.departmentName}` : "—"}</td>
                    <td>{u.positionName ?? "—"}</td>
                    <td>
                      {u.roleName ? (
                        <span className="admin-role-badge">{u.roleName}</span>
                      ) : "—"}
                    </td>
                    <td><OnlineBadge online={isOnline(u.lastLoginAt, u.lastLogoutAt)} /></td>
                    <td className="admin-table__dim">{fmtDate(u.lastLoginAt)}</td>
                    <td className="admin-table__dim">{fmtDate(u.lastLogoutAt)}</td>
                    <td><ActiveBadge active={u.isActive} /></td>
                    <td>
                      <div className="admin-table__actions">
                        <button
                          className="admin-btn admin-btn--xs"
                          onClick={() => setDialog({ type: "edit", user: u })}
                          type="button"
                        >Sửa</button>
                        <button
                          className="admin-btn admin-btn--xs"
                          onClick={() => setDialog({ type: "change-role", user: u })}
                          type="button"
                        >Vai trò</button>
                        <button
                          className="admin-btn admin-btn--xs admin-btn--danger"
                          onClick={() => setDialog({ type: "reset-password", user: u })}
                          type="button"
                        >MK</button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="admin-pagination">
              <span className="admin-pagination__info">
                Trang {filter.page}/{totalPages} · {meta?.total ?? 0} người dùng
              </span>
              <div className="admin-pagination__controls">
                <button
                  className="admin-btn admin-btn--xs"
                  disabled={filter.page <= 1}
                  onClick={() => setFilter((p) => ({ ...p, page: p.page - 1 }))}
                  type="button"
                >← Trước</button>
                <button
                  className="admin-btn admin-btn--xs"
                  disabled={filter.page >= totalPages}
                  onClick={() => setFilter((p) => ({ ...p, page: p.page + 1 }))}
                  type="button"
                >Sau →</button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Dialogs */}
      {dialog?.type === "create" && (
        <UserFormDialog filter={filter} mode={{ type: "create" }} onClose={() => setDialog(null)} />
      )}
      {dialog?.type === "edit" && (
        <UserFormDialog filter={filter} mode={{ type: "edit", user: dialog.user }} onClose={() => setDialog(null)} />
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
