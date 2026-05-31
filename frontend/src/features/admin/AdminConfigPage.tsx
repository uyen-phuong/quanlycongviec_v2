import { useState } from "react";
import { toApiError } from "@/shared/api/client";
import {
  useAdminDepartments,
  useAdminPositions,
  useAdminRoles,
  useCreateDepartment,
  useCreatePosition,
  useDeletePosition,
  useUpdateDepartment,
  useUpdatePosition,
} from "@/features/admin/hooks";
import type { AdminDepartmentDto, AdminPositionDto } from "@/features/admin/types";
import "@/features/admin/AdminPages.css";

type Tab = "departments" | "positions" | "roles";

// ─── DEPARTMENTS TAB ───────────────────────────────────────────────
function DeptRow({ dept }: { dept: AdminDepartmentDto }) {
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

  return (
    <tr>
      <td className="admin-table__mono">{dept.code}</td>
      <td>
        {editing ? (
          <>
            <input className="admin-input" onChange={(e) => setName(e.target.value)} value={name} style={{ width: "100%" }} />
            {error && <p className="admin-field__error">{error}</p>}
          </>
        ) : (
          <span style={{ fontWeight: 600 }}>{dept.name}</span>
        )}
      </td>
      <td>
        {editing ? (
          <label className="admin-field admin-field--row" style={{ margin: 0 }}>
            <input checked={isActive} onChange={(e) => setIsActive(e.target.checked)} type="checkbox" />
            <span>Hoạt động</span>
          </label>
        ) : (
          <span className={`admin-badge ${dept.isActive ? "admin-badge--active" : "admin-badge--inactive"}`}>
            {dept.isActive ? "Hoạt động" : "Tạm dừng"}
          </span>
        )}
      </td>
      <td>
        {editing ? (
          <div className="admin-table__actions">
            <button className="admin-btn admin-btn--xs admin-btn--primary" disabled={mutation.isPending} onClick={() => { void handleSave(); }} type="button">
              {mutation.isPending ? "Lưu..." : "Lưu"}
            </button>
            <button className="admin-btn admin-btn--xs admin-btn--ghost" onClick={handleCancel} type="button">Hủy</button>
          </div>
        ) : (
          <button className="admin-btn admin-btn--xs" onClick={() => setEditing(true)} type="button">Sửa</button>
        )}
      </td>
    </tr>
  );
}

function DepartmentsTab() {
  const deptsQuery = useAdminDepartments();
  const createMutation = useCreateDepartment();
  const [adding, setAdding] = useState(false);
  const [newCode, setNewCode] = useState("");
  const [newName, setNewName] = useState("");
  const [addError, setAddError] = useState<string | null>(null);

  async function handleAdd() {
    setAddError(null);
    if (!newCode.trim() || !newName.trim()) {
      setAddError("Mã và Tên đơn vị là bắt buộc.");
      return;
    }
    try {
      await createMutation.mutateAsync({ code: newCode.trim(), name: newName.trim() });
      setNewCode("");
      setNewName("");
      setAdding(false);
    } catch (err) {
      setAddError(toApiError(err).message);
    }
  }

  return (
    <div>
      <div className="admin-section-header">
        <div>
          <h2 className="admin-section-title">Danh sách Phòng Ban / Đơn vị</h2>
          <p className="admin-section-desc">Mã đơn vị (code) là bất biến sau khi tạo. Có thể tắt nhưng không xóa.</p>
        </div>
        <button className="admin-btn admin-btn--primary" onClick={() => setAdding(true)} type="button">+ Thêm đơn vị</button>
      </div>

      {adding && (
        <div className="admin-add-row">
          <input className="admin-input" onChange={(e) => setNewCode(e.target.value)} placeholder="Mã (vd: KTNB4)" style={{ width: 130 }} value={newCode} />
          <input className="admin-input" onChange={(e) => setNewName(e.target.value)} placeholder="Tên đơn vị đầy đủ" style={{ flex: 1 }} value={newName} />
          <button className="admin-btn admin-btn--primary" disabled={createMutation.isPending} onClick={() => { void handleAdd(); }} type="button">
            {createMutation.isPending ? "Đang tạo..." : "Tạo"}
          </button>
          <button className="admin-btn admin-btn--ghost" onClick={() => { setAdding(false); setAddError(null); }} type="button">Hủy</button>
          {addError && <p className="admin-field__error">{addError}</p>}
        </div>
      )}

      {deptsQuery.isLoading && <p className="admin-state">Đang tải...</p>}
      {deptsQuery.isError && <p className="admin-state admin-state--error">{toApiError(deptsQuery.error).message}</p>}

      {deptsQuery.data && (
        <div className="admin-table-wrapper">
          <table className="admin-table">
            <thead>
              <tr>
                <th style={{ width: 100 }}>Mã</th>
                <th>Tên đơn vị</th>
                <th style={{ width: 130 }}>Trạng thái</th>
                <th style={{ width: 80 }}></th>
              </tr>
            </thead>
            <tbody>
              {deptsQuery.data.map((d) => <DeptRow dept={d} key={d.id} />)}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// ─── POSITIONS TAB ─────────────────────────────────────────────────
function PositionRow({ pos }: { pos: AdminPositionDto }) {
  const [editing, setEditing] = useState(false);
  const [name, setName] = useState(pos.name);
  const [isActive, setIsActive] = useState(pos.isActive);
  const [sortOrder, setSortOrder] = useState(pos.sortOrder);
  const [error, setError] = useState<string | null>(null);
  const updateMutation = useUpdatePosition();
  const deleteMutation = useDeletePosition();

  async function handleSave() {
    setError(null);
    try {
      await updateMutation.mutateAsync({ id: pos.id, name, isActive, sortOrder });
      setEditing(false);
    } catch (err) {
      setError(toApiError(err).message);
    }
  }

  async function handleDelete() {
    if (!confirm(`Xóa chức vụ "${pos.name}"?`)) return;
    try {
      await deleteMutation.mutateAsync(pos.id);
    } catch (err) {
      alert(toApiError(err).message);
    }
  }

  return (
    <tr>
      <td className="admin-table__mono">{pos.code}</td>
      <td>
        {editing ? (
          <>
            <input className="admin-input" onChange={(e) => setName(e.target.value)} value={name} style={{ width: "100%" }} />
            {error && <p className="admin-field__error">{error}</p>}
          </>
        ) : (
          <span style={{ fontWeight: 600 }}>{pos.name}</span>
        )}
      </td>
      <td>
        {editing ? (
          <input
            className="admin-input"
            max={99}
            min={1}
            onChange={(e) => setSortOrder(Number(e.target.value))}
            style={{ width: 70 }}
            type="number"
            value={sortOrder}
          />
        ) : (
          <span className="admin-table__dim">{pos.sortOrder}</span>
        )}
      </td>
      <td>
        {editing ? (
          <label className="admin-field admin-field--row" style={{ margin: 0 }}>
            <input checked={isActive} onChange={(e) => setIsActive(e.target.checked)} type="checkbox" />
            <span>Hoạt động</span>
          </label>
        ) : (
          <span className={`admin-badge ${pos.isActive ? "admin-badge--active" : "admin-badge--inactive"}`}>
            {pos.isActive ? "Hoạt động" : "Tắt"}
          </span>
        )}
      </td>
      <td>
        {editing ? (
          <div className="admin-table__actions">
            <button className="admin-btn admin-btn--xs admin-btn--primary" disabled={updateMutation.isPending} onClick={() => { void handleSave(); }} type="button">
              {updateMutation.isPending ? "Lưu..." : "Lưu"}
            </button>
            <button className="admin-btn admin-btn--xs admin-btn--ghost" onClick={() => setEditing(false)} type="button">Hủy</button>
          </div>
        ) : (
          <div className="admin-table__actions">
            <button className="admin-btn admin-btn--xs" onClick={() => setEditing(true)} type="button">Sửa</button>
            <button className="admin-btn admin-btn--xs admin-btn--danger" disabled={deleteMutation.isPending} onClick={() => { void handleDelete(); }} type="button">Xóa</button>
          </div>
        )}
      </td>
    </tr>
  );
}

function PositionsTab() {
  const posQuery = useAdminPositions();
  const createMutation = useCreatePosition();
  const [adding, setAdding] = useState(false);
  const [newCode, setNewCode] = useState("");
  const [newName, setNewName] = useState("");
  const [newSort, setNewSort] = useState(99);
  const [addError, setAddError] = useState<string | null>(null);

  async function handleAdd() {
    setAddError(null);
    if (!newCode.trim() || !newName.trim()) {
      setAddError("Mã và Tên chức vụ là bắt buộc.");
      return;
    }
    try {
      await createMutation.mutateAsync({ code: newCode.trim(), name: newName.trim(), sortOrder: newSort });
      setNewCode(""); setNewName(""); setNewSort(99);
      setAdding(false);
    } catch (err) {
      setAddError(toApiError(err).message);
    }
  }

  return (
    <div>
      <div className="admin-section-header">
        <div>
          <h2 className="admin-section-title">Danh sách Chức vụ</h2>
          <p className="admin-section-desc">Chức vụ được gắn vào từng người dùng để hiển thị thông tin. Không liên quan đến phân quyền (dùng Role).</p>
        </div>
        <button className="admin-btn admin-btn--primary" onClick={() => setAdding(true)} type="button">+ Thêm chức vụ</button>
      </div>

      {adding && (
        <div className="admin-add-row">
          <input className="admin-input" onChange={(e) => setNewCode(e.target.value)} placeholder="Mã (vd: PHO_PHONG)" style={{ width: 160 }} value={newCode} />
          <input className="admin-input" onChange={(e) => setNewName(e.target.value)} placeholder="Tên chức vụ" style={{ flex: 1 }} value={newName} />
          <input className="admin-input" max={99} min={1} onChange={(e) => setNewSort(Number(e.target.value))} placeholder="Thứ tự" style={{ width: 90 }} type="number" value={newSort} />
          <button className="admin-btn admin-btn--primary" disabled={createMutation.isPending} onClick={() => { void handleAdd(); }} type="button">
            {createMutation.isPending ? "Đang tạo..." : "Tạo"}
          </button>
          <button className="admin-btn admin-btn--ghost" onClick={() => { setAdding(false); setAddError(null); }} type="button">Hủy</button>
          {addError && <p className="admin-field__error">{addError}</p>}
        </div>
      )}

      {posQuery.isLoading && <p className="admin-state">Đang tải...</p>}
      {posQuery.isError && <p className="admin-state admin-state--error">{toApiError(posQuery.error).message}</p>}

      {posQuery.data && (
        <div className="admin-table-wrapper">
          <table className="admin-table">
            <thead>
              <tr>
                <th style={{ width: 160 }}>Mã</th>
                <th>Tên chức vụ</th>
                <th style={{ width: 90 }}>Thứ tự</th>
                <th style={{ width: 130 }}>Trạng thái</th>
                <th style={{ width: 120 }}></th>
              </tr>
            </thead>
            <tbody>
              {posQuery.data.map((p) => <PositionRow key={p.id} pos={p} />)}
              {posQuery.data.length === 0 && (
                <tr><td colSpan={5} className="admin-state">Chưa có chức vụ nào.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// ─── ROLES TAB ─────────────────────────────────────────────────────
function RolesTab() {
  const rolesQuery = useAdminRoles();

  const roleDescriptions: Record<string, string> = {
    "ADMIN": "Toàn quyền quản trị hệ thống. Không thêm/sửa công việc.",
    "TRUONG_KTNB": "Phê duyệt cuối cùng KH KTNB và Dự án. Xem toàn bộ hệ thống.",
    "PHO_TRUONG_KTNB": "Phê duyệt cấp 2. Phê duyệt KH đơn vị cấp cao nhất.",
    "TRUONG_PHONG": "Kiểm soát và phê duyệt trong phạm vi phòng mình.",
    "PHO_PHONG": "Kiểm soát khi được TP ủy quyền cụ thể trên từng Task.",
    "NHAN_VIEN": "Thực hiện và cập nhật tiến độ công việc được giao.",
    "VAN_THU": "Tạo KH KTNB, Dự án. Quyền như Nhân viên trong phòng.",
    "GUEST": "Chỉ xem KH KTNB và Dự án. Không thêm/sửa/xóa.",
  };

  return (
    <div>
      <div className="admin-section-header">
        <div>
          <h2 className="admin-section-title">Danh sách Vai trò (Role)</h2>
          <p className="admin-section-desc">
            Vai trò là nguồn sự thật cho phân quyền nghiệp vụ. Mã role không được thay đổi. Tên hiển thị có thể cập nhật qua seed.
          </p>
        </div>
      </div>

      {rolesQuery.isLoading && <p className="admin-state">Đang tải...</p>}
      {rolesQuery.isError && <p className="admin-state admin-state--error">{toApiError(rolesQuery.error).message}</p>}

      {rolesQuery.data && (
        <div className="admin-table-wrapper">
          <table className="admin-table">
            <thead>
              <tr>
                <th style={{ width: 200 }}>Mã Role</th>
                <th style={{ width: 280 }}>Tên hiển thị</th>
                <th>Mô tả quyền hạn</th>
              </tr>
            </thead>
            <tbody>
              {rolesQuery.data.map((r) => (
                <tr key={r.id}>
                  <td className="admin-table__mono">{r.code}</td>
                  <td>
                    <span className="admin-role-badge">{r.name}</span>
                  </td>
                  <td className="admin-table__dim">{roleDescriptions[r.code] ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// ─── MAIN PAGE ─────────────────────────────────────────────────────
export function AdminConfigPage() {
  const [tab, setTab] = useState<Tab>("departments");

  return (
    <div className="admin-page">
      <div className="admin-page__header">
        <div>
          <h1>Cấu hình thông số hệ thống</h1>
          <p className="admin-page__subtitle">
            Quản lý danh mục Phòng Ban, Chức vụ và xem danh sách Vai trò trong hệ thống.
          </p>
        </div>
      </div>

      <div className="admin-tabs">
        <button
          className={`admin-tab-btn ${tab === "departments" ? "admin-tab-btn--active" : ""}`}
          onClick={() => setTab("departments")}
          type="button"
        >
          🏢 Phòng Ban
        </button>
        <button
          className={`admin-tab-btn ${tab === "positions" ? "admin-tab-btn--active" : ""}`}
          onClick={() => setTab("positions")}
          type="button"
        >
          👤 Chức vụ
        </button>
        <button
          className={`admin-tab-btn ${tab === "roles" ? "admin-tab-btn--active" : ""}`}
          onClick={() => setTab("roles")}
          type="button"
        >
          🔑 Vai trò (Role)
        </button>
      </div>

      {tab === "departments" && <DepartmentsTab />}
      {tab === "positions" && <PositionsTab />}
      {tab === "roles" && <RolesTab />}
    </div>
  );
}
