import { NavLink } from "react-router-dom";
import { useDepartments } from "@/features/plan-tracking/hooks";
import { getDepartmentLabel } from "@/shared/departmentLabels";
import { useAuth } from "@/shared/auth/useAuth";
import "@/layouts/Sidebar.css";

const GLOBAL_TRACKING_ROLES = ["ADMIN", "VAN_THU", "TRUONG_KH", "TRUONG_KTNB", "PHO_TRUONG_KTNB"] as const;
const PERSONAL_EVAL_ROLES = ["ADMIN", "TRUONG_KTNB", "PHO_TRUONG_KTNB", "TRUONG_PHONG", "TRUONG_KH", "TRUONG_NHOM", "NHAN_VIEN"] as const;

const giamSatItems = [
  { to: "/plan-tracking", label: "Theo dõi tiến độ thực hiện kế hoạch công tác" },
  { to: "/plans/main", label: "Kế hoạch" },
];

const heThongItems = [
  { to: "/admin/users", label: "Quản lý hệ thống" },
  { to: "/admin/departments", label: "Quản lý danh mục" },
  { to: "/admin/roles", label: "Vai trò" },
  { to: "/admin/approval-configs", label: "Cấu hình phê duyệt" },
];

export function Sidebar() {
  const auth = useAuth();
  const departmentsQuery = useDepartments();
  const isAdmin = auth.user?.roles.includes("ADMIN") ?? false;
  const canBrowseDepts = auth.user?.roles.some((r) =>
    GLOBAL_TRACKING_ROLES.includes(r as (typeof GLOBAL_TRACKING_ROLES)[number]),
  ) ?? false;
  const hasOwnDepartment = Boolean(auth.user?.departmentCode);

  const ktnbDepts = (departmentsQuery.data ?? []).filter((d) =>
    d.code.startsWith("KTNB"),
  );
  const otherDepts = (departmentsQuery.data ?? []).filter((d) => !d.code.startsWith("KTNB"));
  const ownDepartment = (departmentsQuery.data ?? []).find(
    (department) => department.code === auth.user?.departmentCode,
  );
  const isDeptScopedUser = hasOwnDepartment && !canBrowseDepts;

  const initials = (auth.user?.fullName ?? "?")
    .split(" ")
    .slice(-2)
    .map((w) => w[0])
    .join("")
    .toUpperCase();

  return (
    <aside className="sb">
      <div className="sb-top">
        <div className="sb-ico">B</div>
        <div>
          <div className="sb-brand">BankCore</div>
          <div className="sb-sub">
            {isDeptScopedUser
              ? getDepartmentLabel(auth.user?.departmentCode, ownDepartment?.name ?? null)
              : "Giám sát"}
          </div>
        </div>
      </div>

      <div className="sb-scroll">
        {isAdmin && (
          <>
            <div className="sb-sec">Hệ thống</div>
            {heThongItems.map((item) => (
              <NavLink
                key={item.to}
                className={({ isActive }) => `sba${isActive ? " on" : ""}`}
                to={item.to}
              >
                {item.label}
              </NavLink>
            ))}
          </>
        )}

        {canBrowseDepts ? (
          <>
            <div className="sb-sec">Giám sát</div>
            {giamSatItems.map((item) => (
              <NavLink
                key={item.to}
                className={({ isActive }) => `sba${isActive ? " on" : ""}`}
                to={item.to}
              >
                {item.label}
              </NavLink>
            ))}
          </>
        ) : null}

        {canBrowseDepts && ktnbDepts.length > 0 && (
          <>
            <div className="sb-suba" style={{ cursor: "default", opacity: 0.7 }}>
              Bộ phận Kiểm toán nội bộ
            </div>
            {ktnbDepts.map((d) => (
              <NavLink
                key={d.code}
                className={({ isActive }) => `sb-suba lv2${isActive ? " on" : ""}`}
                to={`/plan-tracking/dept/${d.code}`}
              >
                {getDepartmentLabel(d.code, d.name)}
              </NavLink>
            ))}
          </>
        )}

        {canBrowseDepts && otherDepts.length > 0 &&
          otherDepts.map((d) => (
            <NavLink
              key={d.code}
              className={({ isActive }) => `sb-suba lv2${isActive ? " on" : ""}`}
              to={`/plan-tracking/dept/${d.code}`}
            >
              {getDepartmentLabel(d.code, d.name)}
            </NavLink>
          ))}

        {hasOwnDepartment && auth.user?.departmentCode && (
          <>
            <div className="sb-sec">Đơn vị của tôi</div>
            <div className="sb-panel">
              <div className="sb-panel__title">
                {getDepartmentLabel(auth.user.departmentCode, ownDepartment?.name ?? null)}
              </div>
              <div className="sb-panel__meta">
                Chỉ truy cập được dữ liệu của phòng này
              </div>
            </div>
            <NavLink
              className={({ isActive }) => `sba${isActive ? " on" : ""}`}
              to={`/plan-tracking/dept/${auth.user.departmentCode}`}
            >
              Theo dõi công việc phòng
            </NavLink>
          </>
        )}

        {auth.user?.roles.some((r) =>
          PERSONAL_EVAL_ROLES.includes(r as (typeof PERSONAL_EVAL_ROLES)[number]),
        ) && (
          <NavLink
            className={({ isActive }) => `sba${isActive ? " on" : ""}`}
            to="/plan-tracking/personal"
          >
            Công việc cá nhân
          </NavLink>
        )}
      </div>

      <div className="sb-bot">
        <div
          className="sba"
          onClick={() => void auth.logout()}
          style={{ cursor: "pointer" }}
        >
          <svg fill="none" height="14" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="14">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
            <polyline points="16 17 21 12 16 7" />
            <line x1="21" x2="9" y1="12" y2="12" />
          </svg>
          Đăng xuất
        </div>
      </div>
    </aside>
  );
}
