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
import { useCreateLineComment, useCreateTask, useDepartments, useLineComments, useResolveLineComment, useSaveTask, useTasks } from "@/features/plan-tracking/hooks";
import { TaskTable } from "@/features/plan-tracking/components/TaskTable";
import { PlanFormDialog } from "@/features/plans/PlanFormDialog";
import "@/features/plans/PlansPage.css";
import { ImportExcelDialog } from "@/features/plans/components/ImportExcelDialog";
import { ReturnPlanDialog } from "@/features/plans/components/ReturnPlanDialog";
import {
  useApprovalHistory,
  useApprovePlan,
  useExportMainPlanExcel,
  useImportMainPlanExcel,
  usePlanDetail,
  useSubmitPlan,
  useReturnPlan,
  useUpdatePlan,
} from "@/features/plans/hooks";
import type {
  ImportMainPlanExcelResult,
  PlanFormValues,
} from "@/features/plans/types";
import type { SaveTaskPayload, TaskListItem, TaskRowViewModel } from "@/features/plan-tracking/types";

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
    for (const task of byParent.get(parentTaskId) ?? []) {
      rows.push({ task, depth });
      walk(task.id, depth + 1);
    }
  }

  walk(null, 0);
  return rows;
}

function canResolveComments(userRoles: string[]) {
  return userRoles.some((role) => ["VAN_THU", "ADMIN"].includes(role));
}

function minDeadline(createdAt: string) {
  return new Date(createdAt).toISOString().slice(0, 10);
}

function detailTrackingLink(departmentCode: string | null, year: number, month: number) {
  const monthValue = `${year}-${String(month).padStart(2, "0")}`;
  if (departmentCode) {
    return `/plan-tracking/dept/${departmentCode}?month=${monthValue}`;
  }

  return `/plan-tracking?month=${monthValue}`;
}

function canEditPlan(status: string, roles: string[]) {
  if (status !== "draft") {
    return false;
  }

  return roles.some((role) => ["VAN_THU", "ADMIN"].includes(role));
}

function canSubmitPlan(roles: string[]) {
  return roles.includes("VAN_THU");
}

function canApprovePlan(status: string, roles: string[]) {
  if (status === "pending") {
    return roles.includes("TRUONG_KH");
  }

  if (status === "approved_1") {
    return roles.includes("TRUONG_KTNB");
  }

  return false;
}

function canReturnPlan(status: string, roles: string[]) {
  return canApprovePlan(status, roles);
}

function statusClass(status: string) {
  return `plans-status plans-status--${status}`;
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
    if (!action) {
      return;
    }

    const next = new URLSearchParams(searchParams);
    next.delete("action");
    setSearchParams(next, { replace: true });

    if (action === "import") {
      setLastImportResult(null);
      setIsImportOpen(true);
    } else if (action === "export") {
      void handleExportExcel();
    } else if (action === "return") {
      setIsReturnOpen(true);
    }
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
  const selectedComments = useMemo(() => {
    if (!selectedTask) {
      return [];
    }

    return (lineCommentsQuery.data ?? []).filter((item) => item.taskId === selectedTask.id);
  }, [lineCommentsQuery.data, selectedTask]);

  async function handleSave(taskId: string, payload: SaveTaskPayload) {
    try {
      await saveTaskMutation.mutateAsync({ taskId, payload });
    } catch (error) {
      throw new Error(toApiError(error).message);
    }
  }

  async function handleCreateTask(payload: Parameters<typeof createTaskMutation.mutateAsync>[0]) {
    try {
      await createTaskMutation.mutateAsync(payload);
    } catch (error) {
      throw new Error(toApiError(error).message);
    }
  }

  async function handleAddComment(taskId: string, content: string) {
    try {
      await createLineCommentMutation.mutateAsync({ taskId, content });
    } catch (error) {
      throw new Error(toApiError(error).message);
    }
  }

  async function handleUpdatePlan(values: PlanFormValues) {
    try {
      await updatePlanMutation.mutateAsync(values);
      setIsEditOpen(false);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  async function handleReturn(values: {
    comment: string | null;
    lineComments: Array<{ taskId: string; content: string }>;
  }) {
    try {
      await returnPlanMutation.mutateAsync(values);
      setIsReturnOpen(false);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  async function handleUploadPlanAttachment(file: File) {
    try {
      await uploadPlanAttachmentMutation.mutateAsync(file);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  async function handleDeletePlanAttachment(attachmentId: string) {
    try {
      await deletePlanAttachmentMutation.mutateAsync(attachmentId);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  async function handleImportExcel(file: File) {
    try {
      const result = await importExcelMutation.mutateAsync(file);
      setLastImportResult(result);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  async function handleExportExcel() {
    try {
      const result = await exportExcelMutation.mutateAsync();
      const url = window.URL.createObjectURL(result.blob);
      const anchor = document.createElement("a");
      anchor.href = url;
      anchor.download = result.fileName;
      document.body.append(anchor);
      anchor.click();
      anchor.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  function handleSubmit() {
    const comment = window.prompt("Nhap ghi chu nop ke hoach (co the de trong):");
    void submitPlanMutation.mutateAsync(comment ?? null).catch((error) => {
      window.alert(toApiError(error).message);
    });
  }

  function handleApprove() {
    const comment = window.prompt("Nhap ghi chu phe duyet (co the de trong):");
    void approvePlanMutation.mutateAsync(comment ?? null).catch((error) => {
      window.alert(toApiError(error).message);
    });
  }

  if (detailQuery.isLoading) {
    return (
      <section className="plans-state">
        <h2>Dang tai chi tiet ke hoach</h2>
        <p>Frontend dang dong bo metadata, task table, line comments va workflow history.</p>
      </section>
    );
  }

  if (detailQuery.isError || !detailQuery.data) {
    return (
      <section className="plans-state">
        <h2>Khong mo duoc chi tiet</h2>
        <p>{toApiError(detailQuery.error).message}</p>
      </section>
    );
  }

  const plan = detailQuery.data;
  const trackingHref = detailTrackingLink(plan.departmentCode, plan.year, plan.month);
  const editable = canEditPlan(plan.status, auth.user?.roles ?? []);
  const hasImportExportRole =
    auth.user?.roles.some((role) => ["VAN_THU", "ADMIN"].includes(role)) ?? false;
  const canImportExport = hasImportExportRole && (plan.status === "draft" || plan.status === "returned");
  const canExport = hasImportExportRole;
  const submitAllowed = canSubmitPlan(auth.user?.roles ?? []);
  const approveAllowed = canApprovePlan(plan.status, auth.user?.roles ?? []);
  const returnAllowed = canReturnPlan(plan.status, auth.user?.roles ?? []);

  return (
    <div className="plans-page">
      <section className="plans-hero">
        <div>
          <h1>
            Chi tiet ke hoach{" "}
            {String(plan.month).padStart(2, "0")}/{plan.year}
          </h1>
          <p>
            Detail page nay tai su dung cung `TaskTable` voi man tracking, chi them
            metadata, workflow va approval history xung quanh.
          </p>
        </div>
        <div className="plans-detail-cta">
          <span className={statusClass(plan.status)}>{plan.status}</span>
          <Link className="plans-button plans-button--ghost" to={trackingHref}>
            Mo man tracking
          </Link>
          <button
            className="plans-button plans-button--secondary"
            onClick={() => navigate("/plans/main")}
            type="button"
          >
            Ve danh sach
          </button>
        </div>
      </section>

      <div className="plans-detail-grid">
        <section>
          <div className="plans-detail-header">
            <h2>Thong tin ke hoach</h2>
            <div className="plans-table__actions">
              {canImportExport ? (
                <button
                  className="plans-button plans-button--secondary"
                  onClick={() => {
                    setLastImportResult(null);
                    setIsImportOpen(true);
                  }}
                  type="button"
                >
                  Upload Excel
                </button>
              ) : null}
              {canExport ? (
                <button
                  className="plans-button plans-button--ghost"
                  disabled={exportExcelMutation.isPending}
                  onClick={() => {
                    void handleExportExcel();
                  }}
                  type="button"
                >
                  {exportExcelMutation.isPending ? "Dang xuat..." : "Xuat Excel"}
                </button>
              ) : null}
              {editable ? (
                <button
                  className="plans-button plans-button--ghost"
                  onClick={() => setIsEditOpen(true)}
                  type="button"
                >
                  Sua ky
                </button>
              ) : null}
              {submitAllowed ? (
                <button
                  className="plans-button plans-button--primary"
                  disabled={submitPlanMutation.isPending}
                  onClick={handleSubmit}
                  type="button"
                >
                  Nop duyet
                </button>
              ) : null}
              {approveAllowed ? (
                <button
                  className="plans-button plans-button--success"
                  disabled={approvePlanMutation.isPending}
                  onClick={handleApprove}
                  type="button"
                >
                  Phe duyet
                </button>
              ) : null}
              {returnAllowed ? (
                <button
                  className="plans-button plans-button--danger"
                  disabled={returnPlanMutation.isPending}
                  onClick={() => setIsReturnOpen(true)}
                  type="button"
                >
                  Chuyen tra
                </button>
              ) : null}
            </div>
          </div>

          <dl className="plans-meta">
            <div>
              <dt>Phong</dt>
              <dd>{getDepartmentLabel(plan.departmentCode, plan.departmentName) || "Ke hoach tong hop"}</dd>
            </div>
            <div>
              <dt>Nguoi tao</dt>
              <dd>{plan.createdByName ?? "Khong ro"}</dd>
            </div>
            <div>
              <dt>So task</dt>
              <dd>{plan.taskCount}</dd>
            </div>
            <div>
              <dt>Ngay tao</dt>
              <dd>{new Date(plan.createdAt).toLocaleString("vi-VN")}</dd>
            </div>
            <div>
              <dt>Cap nhat</dt>
              <dd>{new Date(plan.updatedAt).toLocaleString("vi-VN")}</dd>
            </div>
          </dl>
        </section>

        <section>
          <div className="plans-detail-header">
            <h2>Approval history</h2>
          </div>

          {approvalHistoryQuery.isLoading ? (
            <p>Dang tai workflow history...</p>
          ) : approvalHistoryQuery.isError ? (
            <p>{toApiError(approvalHistoryQuery.error).message}</p>
          ) : (
            <div className="plans-timeline">
              {(approvalHistoryQuery.data ?? []).map((item) => (
                <div className="plans-timeline__item" key={item.id}>
                  <strong>
                    {item.action} {item.fromStatus} -&gt; {item.toStatus}
                  </strong>
                  <p>
                    {item.actorUserName ?? "Khong ro"} ·{" "}
                    {new Date(item.createdAt).toLocaleString("vi-VN")}
                  </p>
                  {item.comment ? <p>{item.comment}</p> : null}
                </div>
              ))}
              {(approvalHistoryQuery.data ?? []).length === 0 ? (
                <p>Chua co buoc workflow nao duoc ghi nhan.</p>
              ) : null}
            </div>
          )}
        </section>
      </div>

      <section className="plans-attachments">
        <div className="plans-detail-header">
          <h2>File dinh kem ke hoach</h2>
        </div>
        {canImportExport ? (
          <AttachmentUploader
            isUploading={uploadPlanAttachmentMutation.isPending}
            onUpload={(file) => { void handleUploadPlanAttachment(file); }}
          />
        ) : null}
        {planAttachmentsQuery.isLoading ? (
          <p>Dang tai file dinh kem...</p>
        ) : (
          <AttachmentList
            attachments={planAttachmentsQuery.data ?? []}
            canDelete={canImportExport}
            isDeleting={deletePlanAttachmentMutation.isPending}
            onDelete={(id) => { void handleDeletePlanAttachment(id); }}
          />
        )}
      </section>

      {tasksQuery.isLoading || departmentsQuery.isLoading ? (
        <section className="plans-state">
          <h2>Dang tai bang task</h2>
          <p>TaskTable dang cho task list, department lookup va line comments.</p>
        </section>
      ) : tasksQuery.isError ? (
        <section className="plans-state">
          <h2>Khong tai duoc task</h2>
          <p>{toApiError(tasksQuery.error).message}</p>
        </section>
      ) : (
        <TaskTable
          allComments={lineCommentsQuery.data ?? []}
          approvalHistory={approvalHistoryQuery.data ?? []}
          canAddTask={editable}
          canResolveComment={canResolveComments(auth.user?.roles ?? [])}
          departments={departmentsQuery.data ?? []}
          isResolvingComment={resolveLineCommentMutation.isPending}
          minDeadline={minDeadline(plan.createdAt)}
          onAddComment={handleAddComment}
          onCloseDetails={() => setSelectedTask(null)}
          onCreateTask={handleCreateTask}
          onOpenDetails={setSelectedTask}
          onResolveComment={(commentId) => {
            void resolveLineCommentMutation.mutateAsync(commentId);
          }}
          onSave={handleSave}
          planDepartmentId={plan.departmentId}
          planId={id}
          rows={rows}
          scope="main"
          selectedComments={selectedComments}
          selectedTask={selectedTask}
          userRoles={auth.user?.roles ?? []}
        />
      )}

      <PlanFormDialog
        initialValues={{
          year: plan.year,
          month: plan.month,
          departmentId: plan.departmentId,
        }}
        isPending={updatePlanMutation.isPending}
        onClose={() => setIsEditOpen(false)}
        onSubmit={handleUpdatePlan}
        open={isEditOpen}
        title="Cap nhat thong tin ky ke hoach"
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
