import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { toApiError } from "@/shared/api/client";
import { getDepartmentLabel } from "@/shared/departmentLabels";
import { useAuth } from "@/shared/auth/useAuth";
import { AttachmentList } from "@/features/attachments/AttachmentList";
import { AttachmentUploader } from "@/features/attachments/AttachmentUploader";
import {
  useDeleteAttachment,
  usePlanAttachments,
  useUploadPlanAttachment,
} from "@/features/attachments/hooks";
import {
  useCreateLineComment,
  useCreateTask,
  useDepartments,
  useLineComments,
  useResolveLineComment,
  useSaveTask,
  useTasks,
} from "@/features/plan-tracking/hooks";
import { TaskTable } from "@/features/plan-tracking/components/TaskTable";
import { PlanFormDialog } from "@/features/plans/PlanFormDialog";
import "@/features/plans/PlansPage.css";
import { ImportExcelDialog } from "@/features/plans/components/ImportExcelDialog";
import { ReturnPlanDialog } from "@/features/plans/components/ReturnPlanDialog";
import {
  useApprovalHistory,
  useApproveReportingPeriod,
  useApprovePlan,
  useExportMainPlanExcel,
  useImportMainPlanExcel,
  usePlanDetail,
  useReportingPeriods,
  useReturnPlan,
  useSubmitPlan,
  useUpdatePlan,
  useUpdateReportingPeriodProgress,
} from "@/features/plans/hooks";
import type {
  ApprovalHistoryItem,
  ImportMainPlanExcelResult,
  PlanFormValues,
  ReportingPeriod,
} from "@/features/plans/types";
import type { SaveTaskPayload, TaskListItem, TaskRowViewModel } from "@/features/plan-tracking/types";

const PLAN_STATUS_LABELS: Record<string, string> = {
  draft: "Nháp",
  pending: "Chờ kiểm soát",
  approved_1: "Đã kiểm soát – Chờ phê duyệt",
  approved_2: "Đã phê duyệt",
  returned: "Đã chuyển trả",
};

const PLAN_STATUS_COLOR: Record<string, string> = {
  draft: "#6b7280",
  pending: "#b45309",
  approved_1: "#1d4ed8",
  approved_2: "#15803d",
  returned: "#b91c1c",
};

const ACTION_LABELS: Record<string, string> = {
  submit: "Trình duyệt",
  approve: "Phê duyệt",
  return: "Chuyển trả",
  resubmit: "Trình lại",
  inherit: "Kế thừa sang đơn vị",
  sync: "Đồng bộ từ đơn vị",
  import_excel: "Import Excel",
};

function buildRows(tasks: TaskListItem[]): TaskRowViewModel[] {
  const byParent = new Map<string | null, TaskListItem[]>();
  for (const task of tasks) {
    const group = byParent.get(task.parentTaskId) ?? [];
    group.push(task);
    byParent.set(task.parentTaskId, group);
  }
  for (const group of byParent.values()) {
    group.sort((a, b) =>
      a.displayOrder !== b.displayOrder
        ? a.displayOrder - b.displayOrder
        : a.createdAt.localeCompare(b.createdAt)
    );
  }
  const rows: TaskRowViewModel[] = [];
  function walk(parentId: string | null, depth: number) {
    for (const task of byParent.get(parentId) ?? []) {
      rows.push({ task, depth });
      walk(task.id, depth + 1);
    }
  }
  walk(null, 0);
  return rows;
}

function canResolveComments(roles: string[]) {
  return roles.some((r) => ["VAN_THU", "ADMIN"].includes(r));
}

function minDeadline(createdAt: string) {
  return new Date(createdAt).toISOString().slice(0, 10);
}

function detailTrackingLink(deptCode: string | null, year: number, month: number) {
  const m = `${year}-${String(month).padStart(2, "0")}`;
  return deptCode ? `/plan-tracking/dept/${deptCode}?month=${m}` : `/plan-tracking?month=${m}`;
}

function canEditPlan(status: string, roles: string[]) {
  return status === "draft" && roles.some((r) => ["VAN_THU", "ADMIN"].includes(r));
}

function canSubmitPlan(roles: string[]) {
  return roles.includes("VAN_THU") || roles.includes("ADMIN");
}

function canApprovePlan(status: string, roles: string[]) {
  if (status === "pending") return roles.includes("TRUONG_PHONG") || roles.includes("ADMIN");
  if (status === "approved_1") return roles.includes("TRUONG_KTNB") || roles.includes("PHO_TRUONG_KTNB") || roles.includes("ADMIN");
  return false;
}

function canReturnPlan(status: string, roles: string[]) {
  return canApprovePlan(status, roles);
}

function ReportingPeriodSection({
  planId,
  planStatus,
  roles,
}: {
  planId: string;
  planStatus: string;
  roles: string[];
}) {
  const periodsQuery = useReportingPeriods(planId);
  const updateMutation = useUpdateReportingPeriodProgress(planId);
  const approveMutation = useApproveReportingPeriod(planId);

  const [editing, setEditing] = useState(false);
  const [progressText, setProgressText] = useState("");
  const [completionPercent, setCompletionPercent] = useState(0);

  const periods = periodsQuery.data ?? [];
  const openPeriod = periods.find((p) => p.status === "open");
  const closedPeriods = periods.filter((p) => p.status === "closed");

  const canUpdate = ["approved_1", "approved_2", "approved_3"].includes(planStatus);
  const canApprovePeriod = canUpdate &&
    (roles.includes("TRUONG_KTNB") || roles.includes("PHO_TRUONG_KTNB") || roles.includes("ADMIN"));

  function startEdit(period: ReportingPeriod | undefined) {
    setProgressText(period?.progressText ?? "");
    setCompletionPercent(period?.completionPercent ?? 0);
    setEditing(true);
  }

  async function handleSave() {
    try {
      await updateMutation.mutateAsync({ progressText: progressText.trim() || null, completionPercent });
      setEditing(false);
    } catch (err) {
      window.alert(err instanceof Error ? err.message : "Lưu thất bại.");
    }
  }

  async function handleApprove() {
    if (!window.confirm("Bạn có chắc muốn duyệt và đóng kỳ báo cáo này không?")) return;
    try {
      await approveMutation.mutateAsync();
    } catch (err) {
      window.alert(err instanceof Error ? err.message : "Duyệt thất bại.");
    }
  }

  if (!canUpdate) return null;

  return (
    <section className="plans-period-section">
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 14 }}>
        <h2 className="plans-section-title" style={{ margin: 0 }}>Kỳ Báo Cáo Tiến Độ</h2>
        {canApprovePeriod && openPeriod && (
          <button
            className="plans-button plans-button--success"
            disabled={approveMutation.isPending}
            onClick={() => { void handleApprove(); }}
            type="button"
          >
            {approveMutation.isPending ? "Đang duyệt..." : "✓ Duyệt kỳ báo cáo"}
          </button>
        )}
      </div>

      {periodsQuery.isLoading ? (
        <p style={{ color: "var(--ink3)", fontSize: 13 }}>Đang tải...</p>
      ) : (
        <>
          {/* Current open period */}
          <div className="plans-period-card plans-period-card--open">
            <div className="plans-period-card__header">
              <span className="plans-period-card__label">
                {openPeriod ? `📋 ${openPeriod.periodLabel}` : `📋 Kỳ hiện tại`}
              </span>
              <span className="plans-period-status plans-period-status--open">Đang mở</span>
            </div>

            {editing ? (
              <div style={{ display: "flex", flexDirection: "column", gap: 10, marginTop: 10 }}>
                <div>
                  <label style={{ fontSize: 11.5, fontWeight: 700, color: "var(--ink2)", display: "block", marginBottom: 4 }}>
                    Nội dung tiến độ
                  </label>
                  <textarea
                    onChange={(e) => setProgressText(e.target.value)}
                    placeholder="Nhập nội dung tiến độ thực hiện trong kỳ này..."
                    rows={4}
                    style={{ width: "100%", resize: "vertical", border: "1px solid var(--bdr)", borderRadius: 6, padding: "6px 10px", fontSize: 13, fontFamily: "inherit" }}
                    value={progressText}
                  />
                </div>
                <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                  <label style={{ fontSize: 11.5, fontWeight: 700, color: "var(--ink2)" }}>
                    Tỷ lệ hoàn thành:
                  </label>
                  <input
                    max={100}
                    min={0}
                    onChange={(e) => setCompletionPercent(Number(e.target.value))}
                    style={{ width: 70, border: "1px solid var(--bdr)", borderRadius: 5, padding: "4px 8px", fontSize: 13, fontFamily: "inherit" }}
                    type="number"
                    value={completionPercent}
                  />
                  <span style={{ fontSize: 12, color: "var(--ink3)" }}>%</span>
                  <div style={{ flex: 1, height: 8, background: "#e2e8f0", borderRadius: 4 }}>
                    <div style={{ height: "100%", width: `${completionPercent}%`, background: "#16a34a", borderRadius: 4, transition: "width 0.3s" }} />
                  </div>
                </div>
                <div style={{ display: "flex", gap: 8 }}>
                  <button className="plans-button plans-button--primary" disabled={updateMutation.isPending} onClick={() => { void handleSave(); }} type="button">
                    {updateMutation.isPending ? "Đang lưu..." : "Lưu tiến độ"}
                  </button>
                  <button className="plans-button plans-button--ghost" onClick={() => setEditing(false)} type="button">Hủy</button>
                </div>
              </div>
            ) : (
              <div style={{ marginTop: 10 }}>
                {openPeriod?.progressText ? (
                  <p style={{ fontSize: 13, color: "var(--ink)", lineHeight: 1.6, whiteSpace: "pre-wrap", margin: "0 0 8px" }}>
                    {openPeriod.progressText}
                  </p>
                ) : (
                  <p style={{ fontSize: 13, color: "var(--ink3)", fontStyle: "italic", margin: "0 0 8px" }}>
                    Chưa có nội dung tiến độ cho kỳ này.
                  </p>
                )}
                {openPeriod && (
                  <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 10 }}>
                    <span style={{ fontSize: 12, fontWeight: 700, color: "var(--ink2)" }}>Hoàn thành:</span>
                    <span style={{ fontSize: 14, fontWeight: 800, color: "#16a34a" }}>{openPeriod.completionPercent}%</span>
                    <div style={{ flex: 1, height: 8, background: "#e2e8f0", borderRadius: 4 }}>
                      <div style={{ height: "100%", width: `${openPeriod.completionPercent}%`, background: "#16a34a", borderRadius: 4 }} />
                    </div>
                  </div>
                )}
                <button className="plans-button plans-button--secondary" onClick={() => startEdit(openPeriod)} type="button" style={{ fontSize: 12 }}>
                  ✏️ Cập nhật tiến độ
                </button>
              </div>
            )}
          </div>

          {/* Closed periods history */}
          {closedPeriods.length > 0 && (
            <div style={{ marginTop: 12 }}>
              <p style={{ fontSize: 11.5, fontWeight: 700, color: "var(--ink3)", textTransform: "uppercase", letterSpacing: ".5px", margin: "0 0 8px" }}>
                Lịch sử kỳ đã duyệt
              </p>
              {closedPeriods.map((p) => (
                <div key={p.id} className="plans-period-card plans-period-card--closed">
                  <div className="plans-period-card__header">
                    <span className="plans-period-card__label">✓ {p.periodLabel}</span>
                    <span className="plans-period-status plans-period-status--closed">Đã duyệt</span>
                    <span style={{ fontSize: 11, color: "var(--ink3)", marginLeft: "auto" }}>
                      {p.approvedByUserName} · {p.approvedAt ? new Date(p.approvedAt).toLocaleDateString("vi-VN") : ""}
                    </span>
                  </div>
                  <div style={{ marginTop: 6 }}>
                    <span style={{ fontSize: 13, fontWeight: 700, color: "#16a34a" }}>{p.completionPercent}%</span>
                    {p.progressText && (
                      <p style={{ fontSize: 12, color: "var(--ink2)", margin: "4px 0 0", lineHeight: 1.5 }}>{p.progressText}</p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </>
      )}
    </section>
  );
}

function HistoryTimeline({ items }: { items: ApprovalHistoryItem[] }) {
  if (items.length === 0) {
    return <p style={{ color: "var(--ink3)", fontSize: "13px" }}>Chưa có bước duyệt nào được ghi nhận.</p>;
  }
  return (
    <div className="plans-timeline">
      {items.map((item) => {
        const actionLabel = ACTION_LABELS[item.action] ?? item.action;
        const fromLabel = PLAN_STATUS_LABELS[item.fromStatus] ?? item.fromStatus;
        const toLabel = PLAN_STATUS_LABELS[item.toStatus] ?? item.toStatus;
        return (
          <div className="plans-timeline__item" key={item.id}>
            <div className="plans-timeline__action">
              <strong>{actionLabel}</strong>
              <span className="plans-timeline__arrow">
                {fromLabel} → {toLabel}
              </span>
            </div>
            <div className="plans-timeline__meta">
              <span>{item.actorUserName ?? "Không rõ"}</span>
              <span>·</span>
              <span>{new Date(item.createdAt).toLocaleString("vi-VN")}</span>
            </div>
            {item.comment && (
              <div className="plans-timeline__comment">"{item.comment}"</div>
            )}
          </div>
        );
      })}
    </div>
  );
}

export function PlanDetailPage() {
  const { id } = useParams();
  const auth = useAuth();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const [selectedTask, setSelectedTask] = useState<TaskListItem | null>(null);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isReturnOpen, setIsReturnOpen] = useState(false);
  const [isImportOpen, setIsImportOpen] = useState(false);
  const [lastImportResult, setLastImportResult] = useState<ImportMainPlanExcelResult | null>(null);

  useEffect(() => {
    const action = searchParams.get("action");
    if (!action) return;
    const next = new URLSearchParams(searchParams);
    next.delete("action");
    setSearchParams(next, { replace: true });
    if (action === "import") { setLastImportResult(null); setIsImportOpen(true); }
    else if (action === "export") { void handleExportExcel(); }
    else if (action === "return") { setIsReturnOpen(true); }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const detailQuery = usePlanDetail(id ?? null);
  const approvalHistoryQuery = useApprovalHistory(id ?? null);
  const departmentsQuery = useDepartments();
  const tasksQuery = useTasks(id ?? null, null, null);
  const lineCommentsQuery = useLineComments(id ?? null);
  const saveTaskMutation = useSaveTask(id ?? null, null, null);
  const createTaskMutation = useCreateTask(id ?? null, null);
  const createLineCommentMutation = useCreateLineComment(id ?? null);
  const resolveLineCommentMutation = useResolveLineComment(id ?? null);
  const updatePlanMutation = useUpdatePlan(id ?? null);
  const submitPlanMutation = useSubmitPlan(id ?? null);
  const approvePlanMutation = useApprovePlan(id ?? null);
  const returnPlanMutation = useReturnPlan(id ?? null);
  const importExcelMutation = useImportMainPlanExcel(id ?? null);
  const exportExcelMutation = useExportMainPlanExcel(id ?? null);
  const planAttachmentsQuery = usePlanAttachments(id ?? null);
  const uploadPlanAttachmentMutation = useUploadPlanAttachment(id ?? null);
  const deletePlanAttachmentMutation = useDeleteAttachment("plan", id ?? null);

  const rows = useMemo(() => buildRows(tasksQuery.data ?? []), [tasksQuery.data]);
  const selectedComments = useMemo(() =>
    selectedTask ? (lineCommentsQuery.data ?? []).filter((c) => c.taskId === selectedTask.id) : [],
    [lineCommentsQuery.data, selectedTask]
  );

  async function handleSave(taskId: string, payload: SaveTaskPayload) {
    try { await saveTaskMutation.mutateAsync({ taskId, payload }); }
    catch (error) { throw new Error(toApiError(error).message); }
  }

  async function handleCreateTask(payload: Parameters<typeof createTaskMutation.mutateAsync>[0]) {
    try { await createTaskMutation.mutateAsync(payload); }
    catch (error) { throw new Error(toApiError(error).message); }
  }

  async function handleAddComment(taskId: string, content: string) {
    try { await createLineCommentMutation.mutateAsync({ taskId, content }); }
    catch (error) { throw new Error(toApiError(error).message); }
  }

  async function handleUpdatePlan(values: PlanFormValues) {
    try { await updatePlanMutation.mutateAsync(values); setIsEditOpen(false); }
    catch (error) { window.alert(toApiError(error).message); }
  }

  async function handleReturn(values: { comment: string | null; lineComments: Array<{ taskId: string; content: string }> }) {
    try { await returnPlanMutation.mutateAsync(values); setIsReturnOpen(false); }
    catch (error) { window.alert(toApiError(error).message); }
  }

  async function handleUploadPlanAttachment(file: File) {
    try { await uploadPlanAttachmentMutation.mutateAsync(file); }
    catch (error) { window.alert(toApiError(error).message); }
  }

  async function handleDeletePlanAttachment(attachmentId: string) {
    try { await deletePlanAttachmentMutation.mutateAsync(attachmentId); }
    catch (error) { window.alert(toApiError(error).message); }
  }

  async function handleImportExcel(file: File) {
    try { const result = await importExcelMutation.mutateAsync(file); setLastImportResult(result); }
    catch (error) { window.alert(toApiError(error).message); }
  }

  async function handleExportExcel() {
    try {
      const result = await exportExcelMutation.mutateAsync();
      const url = window.URL.createObjectURL(result.blob);
      const a = document.createElement("a");
      a.href = url; a.download = result.fileName;
      document.body.append(a); a.click(); a.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) { window.alert(toApiError(error).message); }
  }

  function handleSubmit() {
    const comment = window.prompt("Nhập ghi chú trình duyệt (có thể để trống):");
    if (comment === null) return;
    void submitPlanMutation.mutateAsync(comment || null).catch((err) => window.alert(toApiError(err).message));
  }

  function handleApprove() {
    const comment = window.prompt("Nhập ý kiến phê duyệt (có thể để trống):");
    if (comment === null) return;
    void approvePlanMutation.mutateAsync(comment || null).catch((err) => window.alert(toApiError(err).message));
  }

  if (detailQuery.isLoading) {
    return (
      <section className="plans-state">
        <div className="plans-state__spinner" />
        <h2>Đang tải chi tiết kế hoạch...</h2>
      </section>
    );
  }

  if (detailQuery.isError || !detailQuery.data) {
    return (
      <section className="plans-state">
        <h2>Không mở được chi tiết kế hoạch</h2>
        <p>{toApiError(detailQuery.error).message}</p>
        <button className="plans-button plans-button--ghost" onClick={() => navigate("/plans/main")} type="button">
          Quay lại danh sách
        </button>
      </section>
    );
  }

  const plan = detailQuery.data;
  const roles = auth.user?.roles ?? [];
  const trackingHref = detailTrackingLink(plan.departmentCode, plan.year, plan.month);
  const editable = canEditPlan(plan.status, roles);
  const hasFileRole = roles.some((r) => ["VAN_THU", "ADMIN"].includes(r));
  const canImport = hasFileRole && (plan.status === "draft" || plan.status === "returned");
  const canExport = hasFileRole;
  const submitAllowed = canSubmitPlan(roles) && (plan.status === "draft" || plan.status === "returned");
  const approveAllowed = canApprovePlan(plan.status, roles);
  const returnAllowed = canReturnPlan(plan.status, roles);

  const statusColor = PLAN_STATUS_COLOR[plan.status] ?? "#6b7280";
  const statusLabel = PLAN_STATUS_LABELS[plan.status] ?? plan.status;

  return (
    <div className="plans-page">

      {/* Hero header */}
      <section className="plans-hero">
        <div>
          <div className="plans-hero__breadcrumb">
            <Link to="/plans/main" style={{ color: "var(--red)", textDecoration: "none", fontSize: "12px" }}>
              ← Danh sách KH
            </Link>
          </div>
          <h1 className="plans-hero__title">
            Kế hoạch công tác KTNB – Tháng {String(plan.month).padStart(2, "0")}/{plan.year}
          </h1>
          <p className="plans-hero__sub">
            {getDepartmentLabel(plan.departmentCode, plan.departmentName) || "Kế hoạch tổng hợp"}
            {" · "}{plan.taskCount} công việc
          </p>
        </div>
        <div className="plans-detail-cta">
          <span
            className="plans-status-pill"
            style={{ background: `${statusColor}18`, color: statusColor, border: `1px solid ${statusColor}40` }}
          >
            {statusLabel}
          </span>
          <Link className="plans-button plans-button--ghost" to={trackingHref}>
            Mở màn theo dõi
          </Link>
        </div>
      </section>

      {/* Workflow actions */}
      <div className="plans-action-bar">
        {canImport && (
          <button className="plans-button plans-button--secondary" onClick={() => { setLastImportResult(null); setIsImportOpen(true); }} type="button">
            📥 Import Excel
          </button>
        )}
        {canExport && (
          <button className="plans-button plans-button--ghost" disabled={exportExcelMutation.isPending} onClick={() => { void handleExportExcel(); }} type="button">
            📤 {exportExcelMutation.isPending ? "Đang xuất..." : "Xuất Excel"}
          </button>
        )}
        {editable && (
          <button className="plans-button plans-button--ghost" onClick={() => setIsEditOpen(true)} type="button">
            ✏️ Sửa kỳ KH
          </button>
        )}
        <div style={{ flex: 1 }} />
        {submitAllowed && (
          <button className="plans-button plans-button--primary" disabled={submitPlanMutation.isPending} onClick={handleSubmit} type="button">
            {submitPlanMutation.isPending ? "Đang trình..." : "Trình duyệt"}
          </button>
        )}
        {approveAllowed && (
          <button className="plans-button plans-button--success" disabled={approvePlanMutation.isPending} onClick={handleApprove} type="button">
            {approvePlanMutation.isPending ? "Đang phê duyệt..." : "Phê duyệt"}
          </button>
        )}
        {returnAllowed && (
          <button className="plans-button plans-button--danger" disabled={returnPlanMutation.isPending} onClick={() => setIsReturnOpen(true)} type="button">
            Chuyển trả
          </button>
        )}
      </div>

      {/* Kỳ Báo Cáo section (only for approved plans) */}
      <ReportingPeriodSection planId={id!} planStatus={plan.status} roles={roles} />

      {/* Meta + History */}
      <div className="plans-detail-grid">
        <section className="plans-meta-section">
          <h2 className="plans-section-title">Thông tin kế hoạch</h2>
          <dl className="plans-meta">
            <div>
              <dt>Người tạo</dt>
              <dd>{plan.createdByName ?? "Không rõ"}</dd>
            </div>
            <div>
              <dt>Ngày tạo</dt>
              <dd>{new Date(plan.createdAt).toLocaleString("vi-VN")}</dd>
            </div>
            {plan.submittedAt && (
              <div>
                <dt>Ngày trình</dt>
                <dd>{new Date(plan.submittedAt).toLocaleString("vi-VN")}</dd>
              </div>
            )}
            {plan.approvedAt && (
              <div>
                <dt>Ngày duyệt</dt>
                <dd>{new Date(plan.approvedAt).toLocaleString("vi-VN")}</dd>
              </div>
            )}
            <div>
              <dt>Cập nhật</dt>
              <dd>{new Date(plan.updatedAt).toLocaleString("vi-VN")}</dd>
            </div>
            <div>
              <dt>Số công việc</dt>
              <dd>{plan.taskCount}</dd>
            </div>
          </dl>
        </section>

        <section className="plans-history-section">
          <h2 className="plans-section-title">Lịch sử phê duyệt</h2>
          {approvalHistoryQuery.isLoading ? (
            <p style={{ color: "var(--ink3)", fontSize: "13px" }}>Đang tải...</p>
          ) : approvalHistoryQuery.isError ? (
            <p style={{ color: "#b91c1c", fontSize: "13px" }}>{toApiError(approvalHistoryQuery.error).message}</p>
          ) : (
            <HistoryTimeline items={approvalHistoryQuery.data ?? []} />
          )}
        </section>
      </div>

      {/* Attachments */}
      <section className="plans-attachments">
        <h2 className="plans-section-title">File đính kèm kế hoạch</h2>
        {canImport && (
          <AttachmentUploader
            isUploading={uploadPlanAttachmentMutation.isPending}
            onUpload={(file) => { void handleUploadPlanAttachment(file); }}
          />
        )}
        {planAttachmentsQuery.isLoading ? (
          <p style={{ color: "var(--ink3)", fontSize: "13px" }}>Đang tải file đính kèm...</p>
        ) : (
          <AttachmentList
            attachments={planAttachmentsQuery.data ?? []}
            canDelete={canImport}
            isDeleting={deletePlanAttachmentMutation.isPending}
            onDelete={(aid) => { void handleDeletePlanAttachment(aid); }}
          />
        )}
      </section>

      {/* Task table */}
      <section className="plans-tasks-section">
        <h2 className="plans-section-title">Danh sách công việc</h2>
        {tasksQuery.isLoading || departmentsQuery.isLoading ? (
          <div className="plans-state plans-state--inline">
            <p>Đang tải danh sách công việc...</p>
          </div>
        ) : tasksQuery.isError ? (
          <div className="plans-state plans-state--inline">
            <p style={{ color: "#b91c1c" }}>{toApiError(tasksQuery.error).message}</p>
          </div>
        ) : (
          <TaskTable
            allComments={lineCommentsQuery.data ?? []}
            approvalHistory={approvalHistoryQuery.data ?? []}
            canAddTask={editable}
            canResolveComment={canResolveComments(roles)}
            departments={departmentsQuery.data ?? []}
            isResolvingComment={resolveLineCommentMutation.isPending}
            minDeadline={minDeadline(plan.createdAt)}
            onAddComment={handleAddComment}
            onCloseDetails={() => setSelectedTask(null)}
            onCreateTask={handleCreateTask}
            onOpenDetails={setSelectedTask}
            onResolveComment={(commentId) => { void resolveLineCommentMutation.mutateAsync(commentId); }}
            onSave={handleSave}
            planDepartmentId={plan.departmentId}
            planId={id}
            planStatus={plan.status}
            rows={rows}
            scope="main"
            selectedComments={selectedComments}
            selectedTask={selectedTask}
            userRoles={roles}
          />
        )}
      </section>

      {/* Dialogs */}
      <PlanFormDialog
        initialValues={{ year: plan.year, month: plan.month, departmentId: plan.departmentId }}
        isPending={updatePlanMutation.isPending}
        onClose={() => setIsEditOpen(false)}
        onSubmit={handleUpdatePlan}
        open={isEditOpen}
        title="Cập nhật thông tin kỳ kế hoạch"
      />

      <ReturnPlanDialog
        isPending={returnPlanMutation.isPending}
        onClose={() => setIsReturnOpen(false)}
        onSubmit={handleReturn}
        open={isReturnOpen}
        tasks={tasksQuery.data ?? []}
      />

      <ImportExcelDialog
        isPending={importExcelMutation.isPending}
        onClose={() => setIsImportOpen(false)}
        onSubmit={handleImportExcel}
        open={isImportOpen}
        result={lastImportResult}
      />
    </div>
  );
}
