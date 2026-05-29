import { useState } from "react";
import { toApiError } from "@/shared/api/client";
import { getDepartmentLabel } from "@/shared/departmentLabels";
import {
  useAdminApprovalConfigs,
  useAdminRoles,
  useUpdateApprovalConfig,
} from "@/features/admin/hooks";
import type { AdminApprovalConfigDto } from "@/features/admin/types";
import "@/features/admin/AdminPages.css";

function scopeLabel(scope: string) {
  return scope === "main" ? "Kế hoạch tổng hợp" : "Kế hoạch đơn vị";
}

function ConfigRow({ config }: { config: AdminApprovalConfigDto }) {
  const [editing, setEditing] = useState(false);
  const [roleId, setRoleId] = useState(config.roleId);
  const [isActive, setIsActive] = useState(config.isActive);
  const [error, setError] = useState<string | null>(null);
  const rolesQuery = useAdminRoles();
  const mutation = useUpdateApprovalConfig();

  async function handleSave() {
    setError(null);
    try {
      await mutation.mutateAsync({
        id: config.id,
        departmentId: config.departmentId,
        roleId,
        isActive,
      });
      setEditing(false);
    } catch (err) {
      setError(toApiError(err).message);
    }
  }

  function handleCancel() {
    setRoleId(config.roleId);
    setIsActive(config.isActive);
    setEditing(false);
    setError(null);
  }

  if (!editing) {
    return (
      <tr>
        <td>{scopeLabel(config.scope)}</td>
        <td>Bước {config.level}</td>
        <td>{getDepartmentLabel(config.departmentCode, config.departmentName) || "Tất cả"}</td>
        <td className="admin-table__mono">{config.roleCode}</td>
        <td>{config.roleName}</td>
        <td>
          <span className={`admin-badge ${config.isActive ? "admin-badge--active" : "admin-badge--inactive"}`}>
            {config.isActive ? "Hoạt động" : "Tắt"}
          </span>
        </td>
        <td>
          <button className="admin-btn admin-btn--xs" onClick={() => setEditing(true)} type="button">
            Sửa
          </button>
        </td>
      </tr>
    );
  }

  return (
    <tr>
      <td>{scopeLabel(config.scope)}</td>
      <td>Bước {config.level}</td>
      <td>{getDepartmentLabel(config.departmentCode, config.departmentName) || "Tất cả"}</td>
      <td colSpan={2}>
        <select
          className="admin-input"
          onChange={(e) => setRoleId(e.target.value)}
          value={roleId}
        >
          {(rolesQuery.data ?? []).map((r) => (
            <option key={r.id} value={r.id}>
              {r.name} ({r.code})
            </option>
          ))}
        </select>
        {error && <p className="admin-field__error">{error}</p>}
      </td>
      <td>
        <label className="admin-field admin-field--row">
          <input
            checked={isActive}
            onChange={(e) => setIsActive(e.target.checked)}
            type="checkbox"
          />
          <span>Bật</span>
        </label>
      </td>
      <td>
        <div className="admin-table__actions">
          <button
            className="admin-btn admin-btn--xs admin-btn--primary"
            disabled={mutation.isPending}
            onClick={() => { void handleSave(); }}
            type="button"
          >
            {mutation.isPending ? "Lưu..." : "Lưu"}
          </button>
          <button className="admin-btn admin-btn--xs admin-btn--ghost" onClick={handleCancel} type="button">
            Hủy
          </button>
        </div>
      </td>
    </tr>
  );
}

export function AdminApprovalConfigsPage() {
  const configsQuery = useAdminApprovalConfigs();

  return (
    <div className="admin-page">
      <div className="admin-page__header">
        <h1>Cấu hình phê duyệt</h1>
        <p className="admin-page__subtitle">
          Cấu hình phê duyệt là nguồn sự thật cho phân công người duyệt theo từng bước.
          Thay đổi có hiệu lực ngay với workflow mới; workflow đang chờ vẫn dùng config cũ.
        </p>
      </div>

      {configsQuery.isLoading && <p className="admin-state">Đang tải...</p>}
      {configsQuery.isError && (
        <p className="admin-state admin-state--error">{toApiError(configsQuery.error).message}</p>
      )}

      {configsQuery.data && (
        <div className="admin-table-wrapper">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Loại kế hoạch</th>
                <th>Bước</th>
                <th>Đơn vị</th>
                <th>Mã vai trò</th>
                <th>Tên vai trò</th>
                <th>Trạng thái</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {configsQuery.data.map((config) => (
                <ConfigRow config={config} key={config.id} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
