import "@/features/admin/AdminPages.css";

type Perm = "view" | "add" | "edit" | "delete" | "approve" | "none";

interface MenuEntry {
  menu: string;
  submenu?: string;
  permissions: Record<string, Perm[]>;
}

const ROLES = [
  { code: "ADMIN", label: "Admin" },
  { code: "TRUONG_KTNB", label: "PD1 – T.KTNB" },
  { code: "PHO_TRUONG_KTNB", label: "PD2 – P.KTNB" },
  { code: "TRUONG_PHONG", label: "KS1 – T.Phòng" },
  { code: "PHO_PHONG", label: "KS2 – P.Phòng" },
  { code: "NHAN_VIEN", label: "Nhân viên" },
  { code: "VAN_THU", label: "Văn thư" },
  { code: "GUEST", label: "Guest" },
];

const V: Perm[] = ["view"];
const VA: Perm[] = ["view", "add"];
const VAE: Perm[] = ["view", "add", "edit"];
const VAED: Perm[] = ["view", "add", "edit", "delete"];
const FULL: Perm[] = ["view", "add", "edit", "delete", "approve"];
const N: Perm[] = [];
const VApp: Perm[] = ["view", "approve"];
const VE: Perm[] = ["view", "edit"];

const MATRIX: MenuEntry[] = [
  // KH KTNB
  {
    menu: "KH Công tác KTNB",
    permissions: {
      ADMIN: V, TRUONG_KTNB: VApp, PHO_TRUONG_KTNB: VApp,
      TRUONG_PHONG: V, PHO_PHONG: V, NHAN_VIEN: V,
      VAN_THU: VAED, GUEST: V,
    },
  },
  {
    menu: "KH Công tác KTNB", submenu: "Tạo / Sửa / Xóa KH",
    permissions: {
      ADMIN: N, TRUONG_KTNB: N, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: N, PHO_PHONG: N, NHAN_VIEN: N,
      VAN_THU: VAED, GUEST: N,
    },
  },
  {
    menu: "KH Công tác KTNB", submenu: "Phê duyệt KH (Trưởng KTNB)",
    permissions: {
      ADMIN: N, TRUONG_KTNB: VApp, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: N, PHO_PHONG: N, NHAN_VIEN: N,
      VAN_THU: N, GUEST: N,
    },
  },
  {
    menu: "KH Công tác KTNB", submenu: "Giao việc trong phòng",
    permissions: {
      ADMIN: N, TRUONG_KTNB: N, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: FULL, PHO_PHONG: V, NHAN_VIEN: V,
      VAN_THU: N, GUEST: N,
    },
  },
  {
    menu: "KH Công tác KTNB", submenu: "Cập nhật tiến độ",
    permissions: {
      ADMIN: N, TRUONG_KTNB: V, PHO_TRUONG_KTNB: V,
      TRUONG_PHONG: VE, PHO_PHONG: VE, NHAN_VIEN: VE,
      VAN_THU: N, GUEST: N,
    },
  },
  // Dự án
  {
    menu: "Dự án Triển khai",
    permissions: {
      ADMIN: V, TRUONG_KTNB: VApp, PHO_TRUONG_KTNB: VApp,
      TRUONG_PHONG: V, PHO_PHONG: V, NHAN_VIEN: V,
      VAN_THU: VAED, GUEST: V,
    },
  },
  {
    menu: "Dự án Triển khai", submenu: "Tạo / Sửa / Xóa Dự án",
    permissions: {
      ADMIN: N, TRUONG_KTNB: N, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: N, PHO_PHONG: N, NHAN_VIEN: N,
      VAN_THU: VAED, GUEST: N,
    },
  },
  {
    menu: "Dự án Triển khai", submenu: "Task trong Dự án",
    permissions: {
      ADMIN: N, TRUONG_KTNB: N, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: V, PHO_PHONG: V, NHAN_VIEN: VE,
      VAN_THU: N, GUEST: N,
    },
  },
  // Công việc riêng phòng
  {
    menu: "Công việc riêng Phòng",
    permissions: {
      ADMIN: V, TRUONG_KTNB: V, PHO_TRUONG_KTNB: V,
      TRUONG_PHONG: FULL, PHO_PHONG: VAE, NHAN_VIEN: VA,
      VAN_THU: VA, GUEST: N,
    },
  },
  {
    menu: "Công việc riêng Phòng", submenu: "Giao việc / Kiểm soát / Phê duyệt",
    permissions: {
      ADMIN: N, TRUONG_KTNB: N, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: FULL, PHO_PHONG: VApp, NHAN_VIEN: N,
      VAN_THU: N, GUEST: N,
    },
  },
  // Công việc cá nhân
  {
    menu: "Công việc cá nhân",
    permissions: {
      ADMIN: N, TRUONG_KTNB: VAED, PHO_TRUONG_KTNB: VAED,
      TRUONG_PHONG: VAED, PHO_PHONG: VAED, NHAN_VIEN: VAED,
      VAN_THU: VAED, GUEST: N,
    },
  },
  // Quản trị hệ thống
  {
    menu: "Quản trị hệ thống",
    permissions: {
      ADMIN: FULL, TRUONG_KTNB: N, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: N, PHO_PHONG: N, NHAN_VIEN: N,
      VAN_THU: N, GUEST: N,
    },
  },
  {
    menu: "Quản trị hệ thống", submenu: "Quản trị người dùng",
    permissions: {
      ADMIN: FULL, TRUONG_KTNB: N, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: N, PHO_PHONG: N, NHAN_VIEN: N,
      VAN_THU: N, GUEST: N,
    },
  },
  {
    menu: "Quản trị hệ thống", submenu: "Cấu hình thông số",
    permissions: {
      ADMIN: FULL, TRUONG_KTNB: N, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: N, PHO_PHONG: N, NHAN_VIEN: N,
      VAN_THU: N, GUEST: N,
    },
  },
  {
    menu: "Quản trị hệ thống", submenu: "Ma trận phân quyền",
    permissions: {
      ADMIN: V, TRUONG_KTNB: N, PHO_TRUONG_KTNB: N,
      TRUONG_PHONG: N, PHO_PHONG: N, NHAN_VIEN: N,
      VAN_THU: N, GUEST: N,
    },
  },
];

const PERM_ICONS: Record<Perm, string> = {
  view: "👁",
  add: "➕",
  edit: "✏️",
  delete: "🗑",
  approve: "✅",
  none: "",
};

function PermCell({ perms }: { perms: Perm[] }) {
  if (perms.length === 0) {
    return (
      <td style={{ textAlign: "center" }}>
        <span className="perm-check perm-check--no">✕</span>
      </td>
    );
  }
  return (
    <td>
      <div style={{ display: "flex", flexWrap: "wrap", gap: 3, justifyContent: "center" }}>
        {perms.map((p) => (
          <span
            key={p}
            className="perm-check perm-check--yes"
            title={p}
            style={{ fontSize: "11px", padding: "2px 5px", borderRadius: 4 }}
          >
            {PERM_ICONS[p]}
          </span>
        ))}
      </div>
    </td>
  );
}

export function AdminPermissionMatrixPage() {
  return (
    <div className="admin-page">
      <div className="admin-page__header">
        <div>
          <h1>Ma trận phân quyền</h1>
          <p className="admin-page__subtitle">
            Tổng quan quyền truy cập của từng vai trò theo menu và submenu trong hệ thống.
          </p>
        </div>
      </div>

      {/* Legend */}
      <div style={{ display: "flex", gap: 16, flexWrap: "wrap", marginBottom: 16 }}>
        {[
          { icon: "👁", label: "Xem (View)" },
          { icon: "➕", label: "Thêm (Add)" },
          { icon: "✏️", label: "Sửa (Edit)" },
          { icon: "🗑", label: "Xóa (Delete)" },
          { icon: "✅", label: "Phê duyệt" },
          { icon: "✕", label: "Không có quyền" },
        ].map((l) => (
          <div key={l.label} style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 12, color: "var(--ink2)" }}>
            <span style={{ fontSize: 14 }}>{l.icon}</span>
            <span>{l.label}</span>
          </div>
        ))}
      </div>

      <div className="perm-matrix">
        <table>
          <thead>
            <tr>
              <th style={{ minWidth: 220 }}>Menu / Chức năng</th>
              {ROLES.map((r) => (
                <th key={r.code} style={{ minWidth: 80, textAlign: "center" }} title={r.code}>
                  {r.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {MATRIX.map((entry, i) => (
              <tr key={i} style={entry.submenu ? {} : { background: "#f9f9fb" }}>
                <td className={entry.submenu ? "perm-matrix__submenu" : "perm-matrix__menu"}>
                  {entry.submenu ? `↳ ${entry.submenu}` : entry.menu}
                </td>
                {ROLES.map((r) => (
                  <PermCell key={r.code} perms={entry.permissions[r.code] ?? []} />
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <p style={{ marginTop: 16, fontSize: 11.5, color: "var(--ink3)" }}>
        * Ma trận này phản ánh quy tắc nghiệp vụ theo PRD. Phân quyền chi tiết cấp Task (người kiểm soát, đầu mối) được thiết lập động tại từng Task/KH.
      </p>
    </div>
  );
}
