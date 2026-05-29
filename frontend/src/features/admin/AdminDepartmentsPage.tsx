import { useState } from "react";
import { toApiError } from "@/shared/api/client";
import { useAdminDepartments, useUpdateDepartment } from "@/features/admin/hooks";
import type { AdminDepartmentDto } from "@/features/admin/types";
import "@/features/admin/AdminPages.css";

function DepartmentRow({
  dept,
}: {
  dept: AdminDepartmentDto;
}) {
  const [editing, setEditing] = useState(false);
  const [name, setName] = useState(dept.name);
  const [isActive, setIsActive] = useState(dept.isActive);
  const [error, setError] = useState<string | null>(null);
  const mutation = useUpdateDepartment();

  async function handleSave() {
    setError(null);
    try {
      await mutation.mutateAsync({ id: dept.id, name, isActive });
      setEditing(false);
    } catch (err) {
      setError(toApiError(err).message);
    }
  }

  function handleCancel() {
    setName(dept.name);
    setIsActive(dept.isActive);
    setEditing(false);
    setError(null);
  }

  if (!editing) {
    return (
      <tr>
        <td className="admin-table__mono">{dept.code}</td>
        <td>{dept.name}</td>
        <td>
          <span className={`admin-badge ${dept.isActive ? "admin-badge--active" : "admin-badge--inactive"}`}>
            {dept.isActive ? "Hoạt động" : "Tạm dừng"}
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
      <td className="admin-table__mono">{dept.code}</td>
      <td>
        <input
          className="admin-input"
          onChange={(e) => setName(e.target.value)}
          value={name}
        />
        {error && <p className="admin-field__error">{error}</p>}
      </td>
      <td>
        <label className="admin-field admin-field--row">
          <input
            checked={isActive}
            onChange={(e) => setIsActive(e.target.checked)}
            type="checkbox"
          />
          <span>Hoạt động</span>
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

export function AdminDepartmentsPage() {
  const deptsQuery = useAdminDepartments();

  return (
    <div className="admin-page">
      <div className="admin-page__header">
        <h1>Quản trị đơn vị</h1>
        <p className="admin-page__subtitle">
          Mã đơn vị (code) là bất biến. Không cho xóa đơn vị.
        </p>
      </div>

      {deptsQuery.isLoading && <p className="admin-state">Đang tải...</p>}
      {deptsQuery.isError && (
        <p className="admin-state admin-state--error">{toApiError(deptsQuery.error).message}</p>
      )}

      {deptsQuery.data && (
        <div className="admin-table-wrapper">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Mã (bất biến)</th>
                <th>Tên đơn vị</th>
                <th>Trạng thái</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {deptsQuery.data.map((dept) => (
                <DepartmentRow dept={dept} key={dept.id} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
