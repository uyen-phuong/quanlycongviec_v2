import { useState } from "react";
import { NavLink } from "react-router-dom";
import { useDepartments } from "@/features/plan-tracking/hooks";
import { getDepartmentLabel } from "@/shared/departmentLabels";
import { useAuth } from "@/shared/auth/useAuth";
import "@/layouts/Sidebar.css";

const ICONS = {
  plans: (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
      <polyline points="14 2 14 8 20 8"/>
      <line x1="16" y1="13" x2="8" y2="13"/>
      <line x1="16" y1="17" x2="8" y2="17"/>
      <polyline points="10 9 9 9 8 9"/>
    </svg>
  ),
  project: (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <rect x="2" y="3" width="6" height="6" rx="1"/>
      <rect x="16" y="3" width="6" height="6" rx="1"/>
      <rect x="9" y="14" width="6" height="7" rx="1"/>
      <path d="M5 9v3h14V9"/>
      <line x1="12" y1="12" x2="12" y2="14"/>
    </svg>
  ),
  dept: (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
      <circle cx="9" cy="7" r="4"/>
      <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
      <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
    </svg>
  ),
  task: (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <polyline points="9 11 12 14 22 4"/>
      <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"/>
    </svg>
  ),
  personal: (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="12" cy="8" r="4"/>
      <path d="M20 21a8 8 0 1 0-16 0"/>
    </svg>
  ),
  admin: (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="12" cy="12" r="3"/>
      <path d="M19.07 4.93a10 10 0 0 1 0 14.14"/>
      <path d="M4.93 4.93a10 10 0 0 0 0 14.14"/>
      <path d="M12 2v2M12 20v2M2 12h2M20 12h2"/>
    </svg>
  ),
  users: (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
      <circle cx="9" cy="7" r="4"/>
      <path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/>
    </svg>
  ),
  config: (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <line x1="4" y1="6" x2="20" y2="6"/>
      <line x1="4" y1="12" x2="20" y2="12"/>
      <line x1="4" y1="18" x2="20" y2="18"/>
    </svg>
  ),
  lock: (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
      <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
    </svg>
  ),
  chevron: (open: boolean) => (
    <svg
      width="12"
      height="12"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      style={{ transition: "transform 0.2s", transform: open ? "rotate(90deg)" : "rotate(0deg)" }}
    >
      <polyline points="9 18 15 12 9 6"/>
    </svg>
  ),
  logout: (
    <svg fill="none" height="14" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="14">
      <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
      <polyline points="16 17 21 12 16 7"/>
      <line x1="21" x2="9" y1="12" y2="12"/>
    </svg>
  ),
};

export function Sidebar() {
  const auth = useAuth();
  const departmentsQuery = useDepartments();
  const isAdmin = auth.isAdmin;
  const canBrowseDepts = auth.canBrowseGlobal;
  const hasOwnDepartment = Boolean(auth.user?.departmentCode);

  const [adminOpen, setAdminOpen] = useState(true);

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
      {/* Brand header */}
      <div className="sb-top">
        <div className="sb-ico">
          <svg viewBox="0 0 32 32" width="26" height="26" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect width="32" height="32" rx="6" fill="#053e2b" />
            <rect x="1.5" y="1.5" width="29" height="29" rx="4.5" stroke="#ffdd00" strokeWidth="1.5" />
            <path d="M16 6V26" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
            <path d="M16 8C14 10 11 13 11 17" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
            <path d="M16 8C18 10 21 13 21 17" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
            <path d="M16 14C14 16 12 18 12 21" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
            <path d="M16 14C18 16 20 18 20 21" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
            <circle cx="16" cy="7" r="1.2" fill="#fff" />
            <circle cx="12" cy="12" r="1" fill="#fff" />
            <circle cx="20" cy="12" r="1" fill="#fff" />
            <circle cx="13" cy="17" r="1" fill="#fff" />
            <circle cx="19" cy="17" r="1" fill="#fff" />
          </svg>
        </div>
        <div>
          <div className="sb-brand">AGRIBANK</div>
          <div className="sb-sub">
            {isDeptScopedUser
              ? getDepartmentLabel(auth.user?.departmentCode, ownDepartment?.name ?? null)
              : "Khối KTNB"}
          </div>
        </div>
      </div>

      <div className="sb-scroll">

        {/* ADMIN section — collapsible */}
        {isAdmin && (
          <div className="sb-group">
            <button
              className="sb-group__toggle"
              onClick={() => setAdminOpen((v) => !v)}
              type="button"
            >
              <span className="sb-group__icon">{ICONS.admin}</span>
              <span className="sb-group__label">Quản trị hệ thống</span>
              <span className="sb-group__chevron">{ICONS.chevron(adminOpen)}</span>
            </button>
            {adminOpen && (
              <div className="sb-group__children">
                <NavLink className={({ isActive }) => `sba sba--sub${isActive ? " on" : ""}`} to="/admin/users">
                  <span className="sba__icon">{ICONS.users}</span>
                  Quản trị người dùng
                </NavLink>
                <NavLink className={({ isActive }) => `sba sba--sub${isActive ? " on" : ""}`} to="/admin/config">
                  <span className="sba__icon">{ICONS.config}</span>
                  Cấu hình thông số
                </NavLink>
                <NavLink className={({ isActive }) => `sba sba--sub${isActive ? " on" : ""}`} to="/admin/permissions">
                  <span className="sba__icon">{ICONS.lock}</span>
                  Ma trận phân quyền
                </NavLink>
              </div>
            )}
          </div>
        )}

        {/* CÔNG VIỆC TRỌNG ĐIỂM */}
        {canBrowseDepts && (
          <>
            <div className="sb-sec">Công việc trọng điểm</div>
            <NavLink className={({ isActive }) => `sba${isActive ? " on" : ""}`} to="/plans/main">
              <span className="sba__icon">{ICONS.plans}</span>
              KH Công tác KTNB
            </NavLink>
            <NavLink className={({ isActive }) => `sba${isActive ? " on" : ""}`} to="/projects">
              <span className="sba__icon">{ICONS.project}</span>
              Dự án triển khai
            </NavLink>
          </>
        )}

        {/* PHÒNG BAN — global browse */}
        {canBrowseDepts && ktnbDepts.length > 0 && (
          <>
            <div className="sb-sec">Bộ phận KTNB</div>
            {ktnbDepts.map((d) => (
              <NavLink
                key={d.code}
                className={({ isActive }) => `sba sba--dept${isActive ? " on" : ""}`}
                to={`/plan-tracking/dept/${d.code}`}
              >
                <span className="sba__dept-dot" />
                {getDepartmentLabel(d.code, d.name)}
              </NavLink>
            ))}
          </>
        )}

        {canBrowseDepts && otherDepts.length > 0 && (
          <>
            <div className="sb-sec">Đơn vị khác</div>
            {otherDepts.map((d) => (
              <NavLink
                key={d.code}
                className={({ isActive }) => `sba sba--dept${isActive ? " on" : ""}`}
                to={`/plan-tracking/dept/${d.code}`}
              >
                <span className="sba__dept-dot" />
                {getDepartmentLabel(d.code, d.name)}
              </NavLink>
            ))}
          </>
        )}

        {/* ĐƠN VỊ CỦA TÔI */}
        {hasOwnDepartment && auth.user?.departmentCode && (
          <>
            <div className="sb-sec">Đơn vị của tôi</div>
            <div className="sb-panel">
              <div className="sb-panel__dept">
                {getDepartmentLabel(auth.user.departmentCode, ownDepartment?.name ?? null)}
              </div>
            </div>
            <NavLink className={({ isActive }) => `sba${isActive ? " on" : ""}`} to={`/plan-tracking/dept/${auth.user.departmentCode}`}>
              <span className="sba__icon">{ICONS.plans}</span>
              Theo dõi KH công tác
            </NavLink>
            <NavLink className={({ isActive }) => `sba${isActive ? " on" : ""}`} to="/department-tasks">
              <span className="sba__icon">{ICONS.dept}</span>
              Công việc riêng phòng
            </NavLink>
          </>
        )}

        {/* CÔNG VIỆC CÁ NHÂN */}
        <div className="sb-sec">Cá nhân</div>
        <NavLink className={({ isActive }) => `sba${isActive ? " on" : ""}`} to="/personal-tasks">
          <span className="sba__icon">{ICONS.personal}</span>
          Công việc cá nhân
        </NavLink>

      </div>

      {/* User info + logout */}
      <div className="sb-bot">
        <div className="sb-user">
          <div className="sb-user__avatar">{initials}</div>
          <div className="sb-user__info">
            <div className="sb-user__name">{auth.user?.fullName ?? "—"}</div>
            <div className="sb-user__role">{auth.user?.roles?.[0] ?? ""}</div>
          </div>
        </div>
        <button
          className="sb-logout"
          onClick={() => void auth.logout()}
          type="button"
        >
          {ICONS.logout}
          Đăng xuất
        </button>
      </div>
    </aside>
  );
}
