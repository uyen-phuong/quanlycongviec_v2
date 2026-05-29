import { useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { toApiError } from "@/shared/api/client";
import { useAuth } from "@/shared/auth/useAuth";
import { useToast } from "@/shared/hooks/useToast";
import { Toast } from "@/shared/components/Toast";
import { personalEvaluationApi } from "@/features/personal-evaluation/api";
import { PeriodScoresBlock } from "@/features/personal-evaluation/components/PeriodScoresBlock";
import { PersonalEvaluationTable } from "@/features/personal-evaluation/components/PersonalEvaluationTable";
import {
  useCreatePersonalItem,
  useDeletePersonalItem,
  usePersonalEvaluation,
  useScorableUsers,
  useSavePersonalItem,
  useSavePersonalPeriod,
} from "@/features/personal-evaluation/hooks";
import {
  canCreateOrDeleteItem,
  canEditItemText,
  canScoreColumn,
  type CurrentUserCtx,
} from "@/features/personal-evaluation/permissions";
import type { ScorableUser } from "@/features/personal-evaluation/types";
import "@/features/personal-evaluation/PersonalEvaluationPage.css";

function thisMonth(): { year: number; month: number; raw: string } {
  const now = new Date();
  const year = now.getFullYear();
  const month = now.getMonth() + 1;
  return { year, month, raw: `${year}-${String(month).padStart(2, "0")}` };
}

function parseMonthParam(raw: string | null): { year: number; month: number; raw: string } {
  if (raw && /^\d{4}-\d{2}$/.test(raw)) {
    const [y, m] = raw.split("-").map(Number);
    return { year: y, month: m, raw };
  }
  return thisMonth();
}

function canPickEmployee(roles: string[]) {
  return roles.some((role) =>
    ["ADMIN", "TRUONG_KTNB", "PHO_TRUONG_KTNB", "TRUONG_PHONG", "TRUONG_KH", "TRUONG_NHOM"].includes(role),
  );
}

const roleName: Record<string, string> = {
  PHO_TRUONG_KTNB: "Phó Trưởng KTNB",
  TRUONG_PHONG: "Trưởng phòng",
  TRUONG_NHOM: "Trưởng nhóm",
  NHAN_VIEN: "Nhân viên",
};

function groupScorableUsers(users: ScorableUser[]) {
  const groups = new Map<string, ScorableUser[]>();
  for (const user of users) {
    const key = user.departmentName ?? "Chưa có phòng";
    groups.set(key, [...(groups.get(key) ?? []), user]);
  }
  return [...groups.entries()];
}

export function PersonalEvaluationPage() {
  const auth = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const monthParam = parseMonthParam(searchParams.get("month"));
  const { toast, showToast, clearToast } = useToast();
  const userIdParam = searchParams.get("userId");
  const userRoles = auth.user?.roles ?? [];
  const shouldPickEmployee = canPickEmployee(userRoles);

  const viewer: CurrentUserCtx = {
    userId: auth.user?.id ?? null,
    departmentId: auth.user?.departmentId ?? null,
    roles: auth.user?.roles ?? [],
  };

  // userId param is optional; default to me. TRUONG_PHONG có thể switch.
  const targetUserId = userIdParam ?? (shouldPickEmployee ? null : auth.user?.id ?? null);

  const query = usePersonalEvaluation(monthParam.year, monthParam.month, targetUserId);
  const scorableUsersQuery = useScorableUsers(shouldPickEmployee);

  const saveItem = useSavePersonalItem(monthParam.year, monthParam.month, targetUserId);
  const createItem = useCreatePersonalItem(monthParam.year, monthParam.month, targetUserId);
  const deleteItem = useDeletePersonalItem(monthParam.year, monthParam.month, targetUserId);
  const savePeriod = useSavePersonalPeriod(monthParam.year, monthParam.month, targetUserId);

  const period = query.data?.period ?? null;
  const items = query.data?.items ?? [];

  const access = useMemo(() => {
    if (!period) {
      return { canText: false, canSelf: false, canTeam: false, canManager: false, canDeputy: false, canHead: false, canDelete: false, canCreate: false };
    }
    const target = { userId: period.userId, departmentId: period.departmentId };
    return {
      canText: canEditItemText(viewer, target),
      canSelf: canScoreColumn(viewer, target, "self"),
      canTeam: canScoreColumn(viewer, target, "teamLead"),
      canManager: canScoreColumn(viewer, target, "manager"),
      canDeputy: canScoreColumn(viewer, target, "deputy"),
      canHead: canScoreColumn(viewer, target, "head"),
      canDelete: canCreateOrDeleteItem(viewer, target),
      canCreate: canCreateOrDeleteItem(viewer, target),
    };
  }, [period, viewer]);

  function handleMonthChange(value: string) {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      if (value) next.set("month", value);
      else next.delete("month");
      return next;
    });
  }

  function handleSelectUser(userId: string) {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      next.set("userId", userId);
      return next;
    });
  }

  function handleBackToList() {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      next.delete("userId");
      return next;
    });
  }

  function handleSaveAll() {
    if (document.activeElement instanceof HTMLElement) {
      document.activeElement.blur();
    }
    showToast("Đã lưu dữ liệu.", "success");
  }

  function handleSaveItem(id: string, payload: Parameters<typeof saveItem.mutate>[0]["payload"]) {
    saveItem.mutate(
      { id, payload },
      {
        onError: (err) => showToast(toApiError(err).message || "Lưu thất bại", "error"),
      },
    );
  }

  function handleAddRow() {
    if (!period) return;
    createItem.mutate(period.id, {
      onError: (err) => showToast(toApiError(err).message || "Không thể thêm dòng", "error"),
    });
  }

  function handleDelete(id: string) {
    if (!confirm("Xóa dòng đánh giá này?")) return;
    deleteItem.mutate(id, {
      onError: (err) => showToast(toApiError(err).message || "Không thể xóa", "error"),
    });
  }

  async function handleExport(variant: "01" | "01a") {
    if (!period) return;
    try {
      const { blob, fileName } = await personalEvaluationApi.downloadExport(period.id, variant);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    } catch (err) {
      showToast(toApiError(err).message || "Tải file thất bại", "error");
    }
  }

  const periodColAccess = useMemo(() => {
    if (!period) return { self: false, teamLead: false, manager: false, deputy: false, head: false };
    const target = { userId: period.userId, departmentId: period.departmentId };
    return {
      self: canScoreColumn(viewer, target, "self"),
      teamLead: canScoreColumn(viewer, target, "teamLead"),
      manager: canScoreColumn(viewer, target, "manager"),
      deputy: canScoreColumn(viewer, target, "deputy"),
      head: canScoreColumn(viewer, target, "head"),
    };
  }, [period, viewer]);

  const roleLabel = useMemo(() => {
    const r = auth.user?.roles ?? [];
    if (r.includes("ADMIN")) return "Admin";
    if (r.includes("TRUONG_KTNB")) return "Trưởng KTNB";
    if (r.includes("PHO_TRUONG_KTNB")) return "Phó Trưởng KTNB";
    if (r.includes("TRUONG_PHONG") || r.includes("TRUONG_KH")) return "Trưởng phòng";
    if (r.includes("TRUONG_NHOM")) return "Trưởng nhóm";
    if (r.includes("NHAN_VIEN")) return "Nhân viên";
    return "";
  }, [auth.user]);

  const statusClass = period?.status?.toLowerCase() === "approved" ? "approved"
    : period?.status?.toLowerCase() === "controlled" ? "controlled" : "draft";

  if (shouldPickEmployee && !userIdParam) {
    const groupedUsers = groupScorableUsers(scorableUsersQuery.data ?? []);

    return (
      <div className="personal-page">
        <div className="card">
          <div className="personal-list-toolbar">
            <div>
              <div className="personal-list-title">Danh sách nhân viên chấm điểm</div>
              <div className="personal-list-subtitle">Chọn một nhân viên để mở bảng chấm điểm cá nhân.</div>
            </div>
            <input
              className="inp-sm personal-month"
              type="month"
              value={monthParam.raw}
              onChange={(e) => handleMonthChange(e.target.value)}
            />
          </div>

          {scorableUsersQuery.isLoading && <div className="personal-empty">Đang tải danh sách...</div>}
          {scorableUsersQuery.isError && (
            <div className="personal-empty personal-empty--error">
              Lỗi tải danh sách: {toApiError(scorableUsersQuery.error).message || String(scorableUsersQuery.error)}
            </div>
          )}
          {!scorableUsersQuery.isLoading && !scorableUsersQuery.isError && groupedUsers.length === 0 && (
            <div className="personal-empty">Chưa có nhân viên thuộc phạm vi chấm điểm.</div>
          )}
          {groupedUsers.map(([departmentName, users]) => (
            <section className="personal-user-section" key={departmentName}>
              <h3>{departmentName}</h3>
              <div className="personal-user-list">
                {users.map((user) => (
                  <button
                    className="personal-user-row"
                    key={user.id}
                    onClick={() => handleSelectUser(user.id)}
                    type="button"
                  >
                    <span className="personal-user-avatar">{user.fullName.slice(0, 1).toUpperCase()}</span>
                    <span className="personal-user-main">
                      <strong>{user.fullName}</strong>
                      <span>{user.departmentCode ?? "—"} · {roleName[user.roleCode] ?? user.roleName}</span>
                    </span>
                    <span className="personal-user-action">Chấm điểm</span>
                  </button>
                ))}
              </div>
            </section>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="personal-page">
      <div className="card">
        {/* Toolbar */}
        <div className="ctb">
          <div style={{ display: "flex", gap: 7 }}>
            {shouldPickEmployee && (
              <button className="btn-g" type="button" onClick={handleBackToList}>
                Danh sách nhân viên
              </button>
            )}
            <button className="btn-g" type="button" onClick={handleSaveAll} disabled={!period}>
              Lưu dữ liệu
            </button>
            <button className="btn-g" type="button" disabled title="Tính năng upload đang phát triển">
              Upload Excel
            </button>
            <button
              className="btn-g"
              type="button"
              onClick={() => handleExport("01")}
              disabled={!period}
            >
              Xuất Phụ lục 01
            </button>
            <button
              className="btn-o"
              type="button"
              onClick={() => handleExport("01a")}
              disabled={!period}
              style={{ background: "#f59e0b", color: "#fff", border: "none", borderRadius: 7, padding: "8px 14px", fontWeight: 600, fontSize: 12, cursor: "pointer" }}
            >
              Xuất Phụ lục 01A
            </button>
          </div>
        </div>

        {/* Status */}
        <div className="status-row">
          <span className="status-lbl">Trạng thái dữ liệu:</span>
          <span className={`status-pill ${statusClass}`}>
            {period?.status === "Approved" ? "Đã phê duyệt" : period?.status === "Controlled" ? "Đã kiểm soát" : "Chưa phê duyệt"}
          </span>
          <span className="status-role">{roleLabel}</span>
        </div>

        {/* Meta */}
        <div className="personal-meta">
          <div className="personal-grid">
            <span className="lblx">Kỳ báo cáo</span>
            <input
              className="inp-sm"
              type="month"
              value={monthParam.raw}
              onChange={(e) => handleMonthChange(e.target.value)}
            />
            <span className="lblx">Phòng</span>
            <span className="valx">{period?.departmentName ?? "—"}</span>
            <span className="lblx">Cán bộ</span>
            <span className="valx">{period?.userFullName ?? "—"}</span>
          </div>
        </div>

        {/* Section header */}
        <div className="shd">
          <span className="slbl">Bảng Theo Dõi Tiến Độ Công Việc Được Giao</span>
          <div className="sln"></div>
          <span className="sbg">Áp dụng cho nhân viên/phó trưởng phòng thuộc Bộ phận Kiểm toán nội bộ</span>
        </div>

        {/* Main table */}
        {query.isLoading && <div style={{ padding: 20, textAlign: "center" }}>Đang tải...</div>}
        {query.isError && (
          <div style={{ padding: 20, color: "#b5271e" }}>
            Lỗi tải dữ liệu: {toApiError(query.error).message || String(query.error)}
            <pre style={{ fontSize: 11, color: "#6b7280", marginTop: 8, whiteSpace: "pre-wrap" }}>
              {JSON.stringify(toApiError(query.error), null, 2)}
            </pre>
          </div>
        )}
        {period && (
          <>
            <PersonalEvaluationTable
              items={items}
              access={access}
              onSave={handleSaveItem}
              onDelete={handleDelete}
            />
            <div
              className="add-row"
              data-disabled={!access.canCreate ? "true" : undefined}
              onClick={() => access.canCreate && handleAddRow()}
            >
              <svg width="14" height="14" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
                <circle cx="12" cy="12" r="10" />
                <line x1="12" y1="8" x2="12" y2="16" />
                <line x1="8" y1="12" x2="16" y2="12" />
              </svg>
              Thêm mục
            </div>

            <PeriodScoresBlock
              period={period}
              access={periodColAccess}
              onSave={(payload) => savePeriod.mutate(
                { id: period.id, payload },
                { onError: (err) => showToast(toApiError(err).message || "Lưu điểm kỳ thất bại", "error") },
              )}
            />
          </>
        )}
      </div>

      <Toast toast={toast} onDismiss={clearToast} />
    </div>
  );
}
