import { useCallback, useMemo, useState } from "react";
import {
  Navigate,
  useLocation,
  useNavigate,
  useParams,
  useSearchParams,
} from "react-router-dom";
import { toApiError } from "@/shared/api/client";
import { useAuth } from "@/shared/auth/useAuth";
import { ReturnPlanDialog } from "@/features/plans/components/ReturnPlanDialog";
import { StatusBar } from "@/features/plan-tracking/components/StatusBar";
import { TaskTable } from "@/features/plan-tracking/components/TaskTable";
import { Toolbar } from "@/features/plan-tracking/components/Toolbar";
import {
  useApprovalHistory,
  useApprovePlan,
  useCreateLineComment,
  useCreateTask,
  useDeleteTask,
  useDepartments,
  useLineComments,
  useResolveLineComment,
  useResolvePlan,
  useReturnPlan,
  useSaveTask,
  useSubmitPlan,
  useTasks,
} from "@/features/plan-tracking/hooks";
import type {
  CreateTaskPayload,
  SaveTaskPayload,
  TaskListItem,
  TaskRowViewModel,
  TrackingContext,
} from "@/features/plan-tracking/types";
import "@/features/plan-tracking/PlanTrackingPage.css";

const GLOBAL_TRACKING_ROLES = ["ADMIN", "VAN_THU", "TRUONG_KH", "TRUONG_KTNB", "PHO_TRUONG_KTNB"] as const;
const OWN_SUBPLAN_ROLES = ["TRUONG_PHONG", "TRUONG_NHOM", "NHAN_VIEN"] as const;

function resolveContext(pathname: string, departmentCode: string | null, userRoles: string[]): TrackingContext {
  if (pathname.includes("/plan-tracking/personal")) {
    return {
      scope: "sub",
      departmentCode,
      workType: 2,
      title: "Công việc cá nhân",
    };
  }

  const deptRouteMatch = pathname.match(/\/plan-tracking\/dept\/([^/?#]+)/);
  if (deptRouteMatch?.[1]) {
    return {
      scope: "sub",
      departmentCode: decodeURIComponent(deptRouteMatch[1]).toUpperCase(),
      workType: null,
      title: "Theo dõi theo đơn vị",
    };
  }

  const canBrowseAllDepartments = userRoles.some((role) =>
    GLOBAL_TRACKING_ROLES.includes(role as (typeof GLOBAL_TRACKING_ROLES)[number]),
  );
  const shouldDefaultToOwnDepartment =
    Boolean(departmentCode) &&
    userRoles.some((role) => OWN_SUBPLAN_ROLES.includes(role as (typeof OWN_SUBPLAN_ROLES)[number]));

  if (shouldDefaultToOwnDepartment || (departmentCode && !canBrowseAllDepartments)) {
    return {
      scope: "sub",
      departmentCode,
      workType: null,
      title: "Theo dõi kế hoạch công tác",
    };
  }

  return {
    scope: "main",
    departmentCode: null,
    workType: null,
    title: "Theo dõi kế hoạch công tác",
  };
}

function deriveMonth(searchParams: URLSearchParams) {
  const value = searchParams.get("month");
  if (value && /^\d{4}-\d{2}$/.test(value)) {
    const [year, month] = value.split("-").map(Number);
    return {
      year,
      month,
      raw: value,
    };
  }

  const now = new Date();
  return {
    year: now.getFullYear(),
    month: now.getMonth() + 1,
    raw: `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}`,
  };
}

function buildRows(tasks: TaskListItem[]): TaskRowViewModel[] {
  const byParent = new Map<string | null, TaskListItem[]>();
  for (const task of tasks) {
    const group = byParent.get(task.parentTaskId) ?? [];
    group.push(task);
    byParent.set(task.parentTaskId, group);
  }

  for (const group of byParent.values()) {
    group.sort((left, right) => {
      if (left.displayOrder !== right.displayOrder) {
        return left.displayOrder - right.displayOrder;
      }

      return left.createdAt.localeCompare(right.createdAt);
    });
  }

  const rows: TaskRowViewModel[] = [];

  function walk(parentTaskId: string | null, depth: number) {
    const group = byParent.get(parentTaskId) ?? [];
    for (const task of group) {
      rows.push({ task, depth });
      walk(task.id, depth + 1);
    }
  }

  walk(null, 0);
  return rows;
}

function toRoleLabel(userRoles: string[]) {
  return userRoles.join(" / ");
}

function toMinDeadline(createdAt: string | null) {
  if (!createdAt) {
    return "";
  }

  return new Date(createdAt).toISOString().slice(0, 10);
}

function canResolveComments(scope: "main" | "sub", userRoles: string[]) {
  if (scope === "main") {
    return userRoles.some((role) => ["VAN_THU", "ADMIN"].includes(role));
  }

  return userRoles.some((role) =>
    ["PHO_TRUONG_KTNB", "TRUONG_PHONG", "NHAN_VIEN"].includes(role),
  );
}

export function PlanTrackingPage() {
  const auth = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const params = useParams();
  const [searchParams] = useSearchParams();
  const [selectedTask, setSelectedTask] = useState<TaskListItem | null>(null);
  const [isReturnOpen, setIsReturnOpen] = useState(false);
  const canBrowseAllDepartments = auth.user?.roles.some((role) =>
    ["ADMIN", "VAN_THU", "TRUONG_KH", "TRUONG_KTNB", "PHO_TRUONG_KTNB"].includes(role),
  ) ?? false;
  const requestedDepartmentCode = params.code?.toUpperCase() ?? null;
  const ownDepartmentCode = auth.user?.departmentCode?.toUpperCase() ?? null;

  if (
    requestedDepartmentCode &&
    ownDepartmentCode &&
    !canBrowseAllDepartments &&
    requestedDepartmentCode !== ownDepartmentCode
  ) {
    return <Navigate replace to={`/plan-tracking/dept/${ownDepartmentCode}`} />;
  }

  const context = resolveContext(
    location.pathname,
    auth.user?.departmentCode ?? null,
    auth.user?.roles ?? [],
  );
  const monthInfo = deriveMonth(searchParams);

  const resolvedPlanQuery = useResolvePlan({
    scope: context.scope,
    departmentCode: params.code ?? context.departmentCode,
    year: monthInfo.year,
    month: monthInfo.month,
  });

  const departmentsQuery = useDepartments();
  const tasksQuery = useTasks(resolvedPlanQuery.data?.planId ?? null, context.workType, params.code ?? context.departmentCode);
  const lineCommentsQuery = useLineComments(resolvedPlanQuery.data?.planId ?? null);
  const approvalHistoryQuery = useApprovalHistory(resolvedPlanQuery.data?.planId ?? null);
  const saveTaskMutation = useSaveTask(resolvedPlanQuery.data?.planId ?? null, context.workType, params.code ?? context.departmentCode);
  const createTaskMutation = useCreateTask(resolvedPlanQuery.data?.planId ?? null, params.code ?? context.departmentCode);
  const deleteTaskMutation = useDeleteTask(resolvedPlanQuery.data?.planId ?? null, context.workType, params.code ?? context.departmentCode);
  const resolveLineCommentMutation = useResolveLineComment(resolvedPlanQuery.data?.planId ?? null);
  const createLineCommentMutation = useCreateLineComment(resolvedPlanQuery.data?.planId ?? null);
  const submitPlanMutation = useSubmitPlan(resolvedPlanQuery.data?.planId ?? null, params.code ?? context.departmentCode);
  const approvePlanMutation = useApprovePlan(resolvedPlanQuery.data?.planId ?? null, params.code ?? context.departmentCode);
  const returnPlanMutation = useReturnPlan(resolvedPlanQuery.data?.planId ?? null);

  const [searchText, setSearchText] = useState("");

  const rows = useMemo(
    () => buildRows(tasksQuery.data ?? []),
    [tasksQuery.data],
  );

  const stats = useMemo(() => {
    const list = tasksQuery.data ?? [];
    const nonHeaders = list.filter((t) => !t.isHeader);
    const total = nonHeaders.length;
    const done = nonHeaders.filter((t) => t.workStatus === "done").length;
    const overdue = nonHeaders.filter((t) => t.workStatus === "overdue").length;
    const withComments = nonHeaders.filter((t) => t.hasOpenComment).length;
    const attention = nonHeaders.filter((t) => t.workStatus === "overdue" || t.hasOpenComment).length;
    const percent = total > 0 ? Math.round((done / total) * 100) : 0;
    return { total, done, overdue, withComments, attention, percent };
  }, [tasksQuery.data]);

  const filteredRows = useMemo(() => {
    const q = searchText.trim().toLowerCase();
    if (!q) return rows;

    const matchingIds = new Set<string>();
    for (const { task } of rows) {
      const haystack = [
        task.title,
        task.bksMemberText,
        task.ktnbLeaderText,
        task.progressText,
        task.assigneeName,
        task.outlineIndex,
        task.noteText,
      ].filter(Boolean).join(" ").toLowerCase();
      if (haystack.includes(q)) matchingIds.add(task.id);
    }

    const taskById = new Map(rows.map(({ task }) => [task.id, task]));
    const includedIds = new Set<string>(matchingIds);
    for (const id of matchingIds) {
      let cur = taskById.get(id);
      while (cur?.parentTaskId) {
        includedIds.add(cur.parentTaskId);
        cur = taskById.get(cur.parentTaskId);
      }
    }

    return rows.filter(({ task }) => includedIds.has(task.id));
  }, [rows, searchText]);

  const selectedComments = useMemo(() => {
    if (!selectedTask) {
      return [];
    }

    return (lineCommentsQuery.data ?? []).filter(
      (comment) => comment.taskId === selectedTask.id,
    );
  }, [lineCommentsQuery.data, selectedTask]);

  const handleSave = useCallback(async (taskId: string, payload: SaveTaskPayload) => {
    try {
      await saveTaskMutation.mutateAsync({ taskId, payload });
    } catch (error) {
      const apiError = toApiError(error);
      throw new Error(apiError.message);
    }
  }, [saveTaskMutation]);

  const handleCreateTask = useCallback(async (payload: CreateTaskPayload) => {
    try {
      await createTaskMutation.mutateAsync(payload);
    } catch (error) {
      const apiError = toApiError(error);
      throw new Error(apiError.message);
    }
  }, [createTaskMutation]);

  const handleDeleteTask = useCallback(async (taskId: string) => {
    try {
      await deleteTaskMutation.mutateAsync(taskId);
    } catch (error) {
      const apiError = toApiError(error);
      throw new Error(apiError.message);
    }
  }, [deleteTaskMutation]);

  const planStatus = resolvedPlanQuery.data?.status ?? null;
  const planDeptCode = (resolvedPlanQuery.data?.departmentCode ?? params.code ?? context.departmentCode ?? "").toUpperCase();
  const userRoles = auth.user?.roles ?? [];
  const userDeptCode = auth.user?.departmentCode?.toUpperCase() ?? null;
  const planEditable = planStatus === "draft" || planStatus === "returned";
  const canAddTask = planEditable && (
    context.scope === "main"
      ? userRoles.some((r) => ["VAN_THU", "ADMIN"].includes(r))
      : userRoles.includes("PHO_TRUONG_KTNB") ||
        (userRoles.some((r) => ["TRUONG_PHONG", "TRUONG_NHOM"].includes(r)) && userDeptCode === planDeptCode)
  );

  function handleMonthChange(value: string) {
    const next = new URLSearchParams(searchParams);
    if (value) {
      next.set("month", value);
    } else {
      next.delete("month");
    }
    navigate({ pathname: location.pathname, search: next.toString() });
  }

  function handleJumpToDetail(action: "import" | "export" | "return") {
    const planId = resolvedPlanQuery.data?.planId;
    if (!planId) {
      return;
    }

    if (action === "return" && context.scope === "sub") {
      setIsReturnOpen(true);
      return;
    }

    navigate(`/plans/main/${planId}?action=${action}`);
  }

  async function handleReturn(values: { comment: string | null; lineComments: Array<{ taskId: string; content: string }> }) {
    try {
      await returnPlanMutation.mutateAsync(values);
      setIsReturnOpen(false);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  async function handleSubmit() {
    const openTasks = (tasksQuery.data ?? []).filter((t) => t.hasOpenComment);
    if (openTasks.length > 0) {
      const titles = openTasks
        .map((t) => `• ${t.outlineIndex ? t.outlineIndex + ". " : ""}${t.title}`)
        .join("\n");
      window.alert(
        `Còn ${openTasks.length} công việc chưa giải quyết nhận xét:\n\n${titles}\n\nVui lòng mở "Chi tiết" từng dòng để giải quyết trước khi kiểm soát.`
      );
      return;
    }

    const comment = window.prompt("Nhập ghi chú kiểm soát (có thể để trống):");
    if (comment === null) return;
    try {
      await submitPlanMutation.mutateAsync(comment);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  async function handleApprove() {
    const comment = window.prompt("Nhập ghi chú phê duyệt (có thể để trống):");
    if (comment === null) return;
    try {
      await approvePlanMutation.mutateAsync(comment);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  async function handleResolveComment(commentId: string) {
    try {
      await resolveLineCommentMutation.mutateAsync(commentId);
    } catch (error) {
      window.alert(`Giải quyết nhận xét thất bại: ${toApiError(error).message}`);
    }
  }

  async function handleAddComment(taskId: string, content: string) {
    await createLineCommentMutation.mutateAsync({ taskId, content });
  }

  const planYear = resolvedPlanQuery.data?.year ?? monthInfo.year;
  const planMonth = resolvedPlanQuery.data?.month ?? monthInfo.month;

  return (
    <div className="tracking-page">
      <div className="srow">
        <div className="sbox">
          <svg fill="none" height="14" stroke="#b0bec5" strokeWidth="2" viewBox="0 0 24 24" width="14">
            <circle cx="11" cy="11" r="8" /><line x1="21" x2="16.65" y1="21" y2="16.65" />
          </svg>
          <input
            onChange={(e) => setSearchText(e.target.value)}
            placeholder="Tìm theo nội dung chỉ đạo, thành viên BKS, tiến độ, người nhập..."
            value={searchText}
          />
        </div>
        <button className="btn-search" onClick={() => setSearchText("")} type="button">
          {searchText ? "Xóa" : (
            <>
              <svg fill="none" height="12" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="12">
                <circle cx="11" cy="11" r="8" /><line x1="21" x2="16.65" y1="21" y2="16.65" />
              </svg>
              Tìm kiếm
            </>
          )}
        </button>
      </div>

      {resolvedPlanQuery.data?.found && tasksQuery.data && tasksQuery.data.length > 0 && (
        <div className="hero-stats">
          <div className="stat-card">
            <div className="stat-icon-wrapper blue">
              <svg fill="none" height="20" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="20">
                <path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2" />
                <rect x="9" y="3" width="6" height="4" rx="2" />
              </svg>
            </div>
            <div className="stat-info">
              <span className="stat-label">Tổng Số Công Việc</span>
              <span className="stat-val">{stats.total}</span>
              <span className="stat-desc">Đầu mục công tác trong kỳ</span>
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-icon-wrapper green">
              <svg fill="none" height="20" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="20">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
                <polyline points="22 4 12 14.01 9 11.01" />
              </svg>
            </div>
            <div className="stat-info">
              <span className="stat-label">Tiến Độ Hoàn Thành</span>
              <div style={{ display: "flex", alignItems: "baseline", gap: "6px" }}>
                <span className="stat-val">{stats.percent}%</span>
                <span className="stat-subval">({stats.done}/{stats.total})</span>
              </div>
              <div className="stat-progress-bg">
                <div className="stat-progress-bar" style={{ width: `${stats.percent}%` }} />
              </div>
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-icon-wrapper red">
              <svg fill="none" height="20" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="20">
                <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                <line x1="12" x2="12" y1="9" y2="13" />
                <line x1="12" x2="12.01" y1="17" y2="17" />
              </svg>
            </div>
            <div className="stat-info">
              <span className="stat-label">Đầu Mục Cần Lưu Ý</span>
              <span className="stat-val warning">{stats.attention}</span>
              <span className="stat-desc">
                {stats.overdue} trễ hạn · {stats.withComments} phản hồi chưa giải quyết
              </span>
            </div>
          </div>
        </div>
      )}

      <div className="card">
        <Toolbar
          hasPlan={Boolean(resolvedPlanQuery.data?.planId)}
          onApprove={handleApprove}
          onJumpToDetail={handleJumpToDetail}
          onSubmit={handleSubmit}
          scope={context.scope}
          status={resolvedPlanQuery.data?.status ?? null}
          userRoles={auth.user?.roles ?? []}
        />

        <div className="shd">
          <span className="slbl">
            📋 Theo dõi tiến độ thực hiện kế hoạch công tác năm {planYear}
          </span>
          <div className="sln" />
          <label className="shd-month">
            <span>Kỳ báo cáo</span>
            <input
              onChange={(e) => handleMonthChange(e.target.value)}
              type="month"
              value={monthInfo.raw}
            />
          </label>
          <span className="sbg">
            Đến {String(planMonth).padStart(2, "0")}/{planYear} · Phụ lục 03/BKS-TKTH
          </span>
        </div>

        {resolvedPlanQuery.isLoading || departmentsQuery.isLoading || tasksQuery.isLoading ? (
          <div className="tracking-state">Đang tải dữ liệu...</div>
        ) : resolvedPlanQuery.isError ? (
          <div className="tracking-state">{toApiError(resolvedPlanQuery.error).message}</div>
        ) : resolvedPlanQuery.data && !resolvedPlanQuery.data.found ? (
          <div className="tracking-state">
            Chưa có kế hoạch cho kỳ {String(monthInfo.month).padStart(2, "0")}/{monthInfo.year}.
          </div>
        ) : resolvedPlanQuery.data?.found ? (
          <div className="tracking-table-shell">
            <TaskTable
              allComments={lineCommentsQuery.data ?? []}
              approvalHistory={approvalHistoryQuery.data ?? []}
              canAddTask={canAddTask}
              canResolveComment={canResolveComments(context.scope, auth.user?.roles ?? [])}
              departments={departmentsQuery.data ?? []}
              isResolvingComment={resolveLineCommentMutation.isPending}
              minDeadline={toMinDeadline(resolvedPlanQuery.data.createdAt)}
              onAddComment={handleAddComment}
              onCloseDetails={() => setSelectedTask(null)}
              onCreateTask={handleCreateTask}
              onDeleteTask={handleDeleteTask}
              onOpenDetails={setSelectedTask}
              onResolveComment={handleResolveComment}
              onSave={handleSave}
              planDepartmentId={resolvedPlanQuery.data.departmentId}
              planId={resolvedPlanQuery.data.planId ?? undefined}
              planStatus={resolvedPlanQuery.data.status ?? null}
              rows={filteredRows}
              scope={context.scope}
              selectedComments={selectedComments}
              selectedTask={selectedTask}
              userRoles={auth.user?.roles ?? []}
            />
          </div>
        ) : null}
      </div>

      {context.scope === "sub" && (
        <ReturnPlanDialog
          isPending={returnPlanMutation.isPending}
          onClose={() => setIsReturnOpen(false)}
          onSubmit={(values) => { void handleReturn(values); }}
          open={isReturnOpen}
          tasks={tasksQuery.data ?? []}
        />
      )}
    </div>
  );
}
