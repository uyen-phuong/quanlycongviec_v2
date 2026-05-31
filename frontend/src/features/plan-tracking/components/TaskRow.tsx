import { useEffect, useMemo, useRef, useState } from "react";
import { getDepartmentLabel } from "@/shared/departmentLabels";
import { UnitPickerModal } from "@/features/plan-tracking/components/UnitPickerModal";
import type { DepartmentLookupDto } from "@/shared/api/dtos";
import { DetailEditorModal } from "@/features/plan-tracking/components/DetailEditorModal";
import { ConfirmDialog } from "@/shared/components/ConfirmDialog";
import { useAuth } from "@/shared/auth/useAuth";
import type { ApprovalHistoryItem } from "@/features/plans/types";
import type {
  AdminUserListItemDto,
  LineComment,
  SaveTaskPayload,
  TaskListItem,
  TaskRowViewModel,
  WorkStatus,
  WorkType,
} from "@/features/plan-tracking/types";

const workTypeOptions: Array<{ value: WorkType; label: string }> = [
  { value: 0, label: "Công việc chung" },
  { value: 1, label: "Công việc riêng" },
  { value: 2, label: "Công việc cá nhân" },
];

function workTypeLabel(value: WorkType) {
  return workTypeOptions.find((o) => o.value === value)?.label ?? "-";
}

const priorityOptions = [
  { value: "normal", label: "Bình thường" },
  { value: "urgent", label: "Khẩn" },
  { value: "critical", label: "Hỏa tốc" },
];

function priorityLabel(value: string) {
  return priorityOptions.find((o) => o.value === value)?.label ?? "Bình thường";
}

const complexityOptions = [
  { value: "low", label: "Thấp" },
  { value: "medium", label: "Trung bình" },
  { value: "high", label: "Cao" },
];

function complexityLabel(value: string) {
  return complexityOptions.find((o) => o.value === value)?.label ?? "Trung bình";
}

const workStatusOptions: Array<{ value: WorkStatus; label: string }> = [
  { value: "not_started", label: "Chưa bắt đầu" },
  { value: "in_progress", label: "Đang thực hiện" },
  { value: "done", label: "Hoàn thành" },
  { value: "overdue", label: "Quá hạn" },
  { value: "paused", label: "Tạm dừng" },
];

function buildDraft(task: TaskListItem) {
  return {
    title: task.title,
    bksMemberText: task.bksMemberText ?? "",
    ktnbLeaderText: task.ktnbLeaderText ?? "",
    deadline: task.deadline ? task.deadline.slice(0, 10) : "",
    workType: (task.workType as WorkType) ?? 0,
    workStatus: task.workStatus,
    progressText: task.progressText ?? "",
    reasonNotCompleted: task.reasonNotCompleted ?? "",
    noteText: task.noteText ?? "",
    ownerDepartmentId: task.ownerDepartmentId ?? "",
    supportingDepartmentIds: task.supportingDepartmentIds,
    assigneeUserId: task.assigneeUserId ?? "",
    controllerUserId: task.controllerUserId ?? "",
    priority: task.priority ?? "normal",
    complexity: task.complexity ?? "medium",
  };
}

function draftKeyOf(draft: ReturnType<typeof buildDraft>) {
  return JSON.stringify({
    ...draft,
    supportingDepartmentIds: [...draft.supportingDepartmentIds].sort((a, b) => a.localeCompare(b)),
  });
}

function normalizePayload(
  task: TaskListItem,
  draft: ReturnType<typeof buildDraft>,
  scope: "main" | "sub",
  fullEdit: boolean,
): SaveTaskPayload {
  if (!fullEdit) {
    return {
      parentTaskId: task.parentTaskId,
      outlineIndex: task.outlineIndex,
      displayOrder: task.displayOrder,
      isHeader: task.isHeader,
      title: task.title,
      workType: task.workType,
      workStatus: draft.workStatus,
      deadline: task.deadline,
      assigneeUserId: task.assigneeUserId,
      controllerUserId: task.controllerUserId,
      ownerDepartmentId: task.ownerDepartmentId,
      bksMemberText: task.bksMemberText,
      ktnbLeaderText: task.ktnbLeaderText,
      noteText: task.noteText,
      progressText: draft.progressText || null,
      reasonNotCompleted: draft.reasonNotCompleted || null,
      priority: task.priority,
      complexity: task.complexity,
      supportingDepartmentIds: [...task.supportingDepartmentIds].sort((a, b) => a.localeCompare(b)),
    };
  }

  return {
    parentTaskId: task.parentTaskId,
    outlineIndex: task.outlineIndex,
    displayOrder: task.displayOrder,
    isHeader: task.isHeader,
    title: draft.title.trim() || task.title,
    workType: draft.workType,
    workStatus: draft.workStatus,
    deadline: draft.deadline ? `${draft.deadline}T00:00:00` : null,
    assigneeUserId: draft.assigneeUserId || null,
    controllerUserId: draft.controllerUserId || null,
    ownerDepartmentId: scope === "sub" ? null : (draft.ownerDepartmentId || null),
    bksMemberText: draft.bksMemberText.trim() || null,
    ktnbLeaderText: draft.ktnbLeaderText.trim() || null,
    noteText: draft.noteText || null,
    progressText: draft.progressText || null,
    reasonNotCompleted: draft.reasonNotCompleted || null,
    priority: draft.priority,
    complexity: draft.complexity,
    supportingDepartmentIds: [...draft.supportingDepartmentIds].sort((a, b) => a.localeCompare(b)),
  };
}

const PLAN_EDITABLE_STATUSES = new Set(["draft", "returned"]);

function canFullEdit(task: TaskListItem, scope: "main" | "sub", userRoles: string[], planStatus: string | null, currentUserId?: string, isProjectLeader?: boolean) {
  if (task.category === 4) {
    return task.assigneeUserId === currentUserId;
  }
  if (task.category === 3) {
    return userRoles.some((r) => ["TRUONG_PHONG", "PHO_TRUONG_KTNB", "TRUONG_NHOM", "ADMIN"].includes(r));
  }
  if (task.category === 2) {
    return isProjectLeader || userRoles.includes("ADMIN");
  }
  if (task.isLocked && scope !== "sub") return false;
  if (!planStatus || !PLAN_EDITABLE_STATUSES.has(planStatus)) return false;
  return scope === "main"
    ? userRoles.some((r) => ["VAN_THU", "ADMIN"].includes(r))
    : userRoles.some((r) => ["TRUONG_PHONG", "PHO_TRUONG_KTNB", "TRUONG_NHOM"].includes(r));
}

function canProgressEdit(task: TaskListItem, scope: "main" | "sub", userRoles: string[], planStatus: string | null, currentUserId?: string) {
  if (task.category === 4) return false;
  if (task.category === 3) {
    return userRoles.includes("NHAN_VIEN") && task.assigneeUserId === currentUserId;
  }
  if (task.category === 2) {
    return task.assigneeUserId === currentUserId;
  }
  if (task.isHeader) return false;
  if (!planStatus || !PLAN_EDITABLE_STATUSES.has(planStatus)) return false;
  return scope === "sub" && userRoles.includes("NHAN_VIEN");
}

function canCommentAsController(scope: "main" | "sub", userRoles: string[]) {
  return scope === "main"
    ? userRoles.some((r) => ["TRUONG_KH", "ADMIN"].includes(r))
    : userRoles.some((r) => ["TRUONG_NHOM", "TRUONG_PHONG", "ADMIN"].includes(r));
}

function canCommentAsApprover(scope: "main" | "sub", userRoles: string[]) {
  return scope === "main"
    ? userRoles.some((r) => ["TRUONG_KTNB", "ADMIN"].includes(r))
    : userRoles.some((r) => ["PHO_TRUONG_KTNB", "ADMIN"].includes(r));
}

function formatVnDateTime(value: string | null | undefined) {
  if (!value) return "";
  return new Date(value).toLocaleString("vi-VN", {
    day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit",
  });
}

function findHistoryActor(
  history: ApprovalHistoryItem[],
  toStatus: string,
) {
  return [...history]
    .reverse()
    .find((h) => h.action === "approve" && h.toStatus === toStatus);
}

function rowClass(task: TaskListItem) {
  if (task.isHeader) return "task-row--header";
  if (task.hasOpenComment) return "task-row--open-comment";
  return "";
}

function saveLabel(state: "idle" | "saving" | "saved" | "error") {
  if (state === "saving") return "Đang lưu...";
  if (state === "saved") return "Đã lưu";
  if (state === "error") return "Lỗi lưu";
  return "";
}

export function TaskRow({
  row,
  scope,
  userRoles,
  departments,
  users,
  minDeadline,
  planDepartmentId,
  planStatus,
  canResolveComment,
  onOpenDetails,
  onSave,
  onDelete,
  onAddComment,
  allComments,
  approvalHistory,
  onSubmitTaskSingle,
  onAssignTaskSingle,
  onApproveTaskSingle,
  onReturnTaskSingle,
  isProjectLeader,
}: {
  row: TaskRowViewModel;
  scope: "main" | "sub";
  userRoles: string[];
  departments: DepartmentLookupDto[];
  users: AdminUserListItemDto[];
  minDeadline: string;
  planDepartmentId: string | null;
  planStatus: string | null;
  canResolveComment: boolean;
  onOpenDetails: (task: TaskListItem) => void;
  onSave: (taskId: string, payload: SaveTaskPayload) => Promise<void>;
  onDelete?: (taskId: string) => Promise<void>;
  onAddComment: (taskId: string, content: string) => Promise<void>;
  allComments: LineComment[];
  approvalHistory: ApprovalHistoryItem[];
  onSubmitTaskSingle?: (taskId: string, comment?: string | null) => Promise<void>;
  onAssignTaskSingle?: (taskId: string, assigneeUserId: string, controllerUserId?: string | null) => Promise<void>;
  onApproveTaskSingle?: (taskId: string, comment?: string | null) => Promise<void>;
  onReturnTaskSingle?: (taskId: string, comment: string) => Promise<void>;
  isProjectLeader?: boolean;
}) {
  const task = row.task;
  const auth = useAuth();
  const currentUser = auth.user;

  const renderWorkflowButtons = () => {
    if (task.isHeader) return null;

    const isNV = userRoles.includes("NHAN_VIEN");
    const isTP = userRoles.includes("TRUONG_PHONG") || userRoles.includes("PHO_TRUONG_KTNB");
    const isTTo = userRoles.includes("TRUONG_NHOM");
    const isAdmin = userRoles.includes("ADMIN");
    const isAssignee = task.assigneeUserId === currentUser?.id;

    const buttons = [];

    // 1. New / Returned -> Submit (NV, TP, Admin)
    if (task.workflowStatus === "new" || task.workflowStatus === "returned") {
      if (isNV || isTP || isAdmin) {
        buttons.push(
          <button
            key="submit"
            className="wf-btn wf-btn--primary"
            onClick={async () => {
              const comment = window.prompt("Nhập ý kiến trình duyệt (tùy chọn):");
              if (comment !== null && onSubmitTaskSingle) {
                await onSubmitTaskSingle(task.id, comment);
              }
            }}
            type="button"
          >
            Trình duyệt
          </button>
        );
      }
    }

    // 2. Pending Assign -> Assign/Giao việc (TP, Admin)
    if (task.workflowStatus === "pending_assign") {
      if (isTP || isAdmin) {
        if (draft.assigneeUserId) {
          buttons.push(
            <button
              key="assign"
              className="wf-btn wf-btn--success"
              onClick={async () => {
                if (onAssignTaskSingle) {
                  await onAssignTaskSingle(task.id, draft.assigneeUserId, draft.controllerUserId || null);
                }
              }}
              type="button"
            >
              Giao việc
            </button>
          );
        } else {
          buttons.push(
            <span key="assign-hint" className="wf-hint">
              Chọn CB để giao việc
            </span>
          );
        }
      }
    }

    // 3. In Progress -> Submit Completed/Trình hoàn thành (Assignee, TP, Admin)
    if (task.workflowStatus === "in_progress") {
      if (isAssignee || isTP || isAdmin) {
        buttons.push(
          <button
            key="submit-complete"
            className="wf-btn wf-btn--primary"
            onClick={async () => {
              const comment = window.prompt("Nhập báo cáo hoàn thành (tùy chọn):");
              if (comment !== null && onSubmitTaskSingle) {
                await onSubmitTaskSingle(task.id, comment);
              }
            }}
            type="button"
          >
            Trình hoàn thành
          </button>
        );
      }
    }

    // 4. Pending Review -> Review Approve/Duyệt kiểm soát, Return/Trả lại (TruongNhom, TP, Admin)
    if (task.workflowStatus === "pending_review") {
      if (isTTo || isTP || isAdmin) {
        buttons.push(
          <button
            key="review-approve"
            className="wf-btn wf-btn--success"
            onClick={async () => {
              const comment = window.prompt("Nhập ý kiến kiểm soát (tùy chọn):");
              if (comment !== null && onApproveTaskSingle) {
                await onApproveTaskSingle(task.id, comment);
              }
            }}
            type="button"
          >
            Duyệt kiểm soát
          </button>
        );
        buttons.push(
          <button
            key="review-return"
            className="wf-btn wf-btn--danger"
            onClick={async () => {
              const comment = window.prompt("Nhập nguyên nhân trả lại (bắt buộc):");
              if (comment && onReturnTaskSingle) {
                await onReturnTaskSingle(task.id, comment);
              } else if (comment === "") {
                window.alert("Nguyên nhân trả lại là bắt buộc.");
              }
            }}
            type="button"
          >
            Trả lại
          </button>
        );
      }
    }

    // 5. Pending Approval -> Approve/Phê duyệt, Return/Trả lại (TP, PhoTruongKtnb, Admin)
    if (task.workflowStatus === "pending_approval") {
      if (isTP || isAdmin) {
        buttons.push(
          <button
            key="approve"
            className="wf-btn wf-btn--success"
            onClick={async () => {
              const comment = window.prompt("Nhập ý kiến phê duyệt (tùy chọn):");
              if (comment !== null && onApproveTaskSingle) {
                await onApproveTaskSingle(task.id, comment);
              }
            }}
            type="button"
          >
            Phê duyệt
          </button>
        );
        buttons.push(
          <button
            key="approve-return"
            className="wf-btn wf-btn--danger"
            onClick={async () => {
              const comment = window.prompt("Nhập nguyên nhân trả lại (bắt buộc):");
              if (comment && onReturnTaskSingle) {
                await onReturnTaskSingle(task.id, comment);
              } else if (comment === "") {
                window.alert("Nguyên nhân trả lại là bắt buộc.");
              }
            }}
            type="button"
          >
            Trả lại
          </button>
        );
      }
    }

    if (buttons.length === 0) return null;

    return <div className="wf-buttons">{buttons}</div>;
  };

  const [draft, setDraft] = useState(() => buildDraft(task));
  const [saveState, setSaveState] = useState<"idle" | "saving" | "saved" | "error">("idle");
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [activeField, setActiveField] = useState<string | null>(null);
  const [ownerPickerOpen, setOwnerPickerOpen] = useState(false);
  const [supportingPickerOpen, setSupportingPickerOpen] = useState(false);
  const [progressEditorOpen, setProgressEditorOpen] = useState(false);
  const [commentColumn, setCommentColumn] = useState<"control" | "approve" | null>(null);
  const [commentText, setCommentText] = useState("");
  const [commentSaving, setCommentSaving] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  async function submitInlineComment() {
    if (!commentText.trim()) return;
    setCommentSaving(true);
    try {
      await onAddComment(task.id, commentText.trim());
      setCommentText("");
      setCommentColumn(null);
    } finally {
      setCommentSaving(false);
    }
  }

  function renderCommentForm() {
    return (
      <div className="comment-inline">
        <textarea
          autoFocus
          className="inp-sm"
          onChange={(e) => setCommentText(e.target.value)}
          placeholder="Nội dung nhận xét..."
          rows={3}
          value={commentText}
        />
        <div className="comment-inline-actions">
          <button
            className="ph-btn"
            disabled={commentSaving || !commentText.trim()}
            onClick={() => { void submitInlineComment(); }}
            type="button"
          >
            {commentSaving ? "Đang lưu..." : "Gửi"}
          </button>
          <button
            className="ph-btn"
            disabled={commentSaving}
            onClick={() => { setCommentColumn(null); setCommentText(""); }}
            type="button"
          >
            Hủy
          </button>
        </div>
      </div>
    );
  }
  const onSaveRef = useRef(onSave);
  const reasonTextareaRef = useRef<HTMLTextAreaElement | null>(null);
  const noteTextareaRef = useRef<HTMLTextAreaElement | null>(null);
  const pendingDeferredSaveRef = useRef(false);

  const fullEdit = canFullEdit(task, scope, userRoles, planStatus, currentUser?.id, isProjectLeader);
  const progressEdit = canProgressEdit(task, scope, userRoles, planStatus, currentUser?.id);
  const canEditAny = fullEdit || progressEdit;
  const canCommentControl = canCommentAsController(scope, userRoles) && !task.isHeader;
  const canCommentApprove = canCommentAsApprover(scope, userRoles) && !task.isHeader;
  const history = Array.isArray(approvalHistory) ? approvalHistory : [];

  const submitHistory = [...history]
    .reverse()
    .find((h) => h.action === "submit" || h.action === "resubmit");
  const controllerHistory = scope === "main"
    ? findHistoryActor(history, "approved_1")
    : findHistoryActor(history, "approved_2");
  const approverHistory = scope === "main"
    ? findHistoryActor(history, "approved_2")
    : findHistoryActor(history, "approved_3");

  const taskComments = allComments.filter((c) => c.taskId === task.id);
  const controllerComments = taskComments.filter((c) => c.authorRole === "controller");
  const approverComments = taskComments.filter((c) => c.authorRole === "approver");

  useEffect(() => {
    onSaveRef.current = onSave;
  }, [onSave]);

  useEffect(() => {
    const autosize = (element: HTMLTextAreaElement | null) => {
      if (!element) return;
      element.style.height = "0px";
      element.style.height = `${element.scrollHeight}px`;
    };

    autosize(reasonTextareaRef.current);
    autosize(noteTextareaRef.current);
  }, [draft.reasonNotCompleted, draft.noteText]);

  const nextDraft = useMemo(() => buildDraft(task), [task]);
  const draftKey = useMemo(() => draftKeyOf(draft), [draft]);
  const nextDraftKey = useMemo(() => draftKeyOf(nextDraft), [nextDraft]);
  const lastTaskDraftKeyRef = useRef(nextDraftKey);

  useEffect(() => {
    const previousTaskDraftKey = lastTaskDraftKeyRef.current;
    lastTaskDraftKeyRef.current = nextDraftKey;

    if (previousTaskDraftKey === nextDraftKey) return;
    if (activeField) return;
    if (draftKey !== previousTaskDraftKey) return;
    if (draftKey === nextDraftKey) return;

    setDraft(nextDraft);
  }, [activeField, draftKey, nextDraft, nextDraftKey]);

  const payload = useMemo(() => normalizePayload(task, draft, scope, fullEdit), [draft, fullEdit, task, scope]);
  const serverPayload = useMemo(() => normalizePayload(task, buildDraft(task), scope, fullEdit), [fullEdit, task, scope]);
  const payloadKey = useMemo(() => JSON.stringify(payload), [payload]);
  const serverPayloadKey = useMemo(() => JSON.stringify(serverPayload), [serverPayload]);
  const isDirty = payloadKey !== serverPayloadKey;

  const payloadRef = useRef(payload);
  useEffect(() => { payloadRef.current = payload; }, [payload]);
  const isDirtyRef = useRef(isDirty);
  useEffect(() => { isDirtyRef.current = isDirty; }, [isDirty]);

  useEffect(() => {
    if (!pendingDeferredSaveRef.current) return;
    if (saveState === "saving") return;
    if (!isDirty) {
      pendingDeferredSaveRef.current = false;
      return;
    }
    pendingDeferredSaveRef.current = false;
    void handleManualSave();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isDirty, payloadKey, saveState]);

  function requestDeferredSave() {
    pendingDeferredSaveRef.current = true;
    // Schedule a microtask save attempt after React flushes the latest draft state.
    window.setTimeout(() => {
      if (!pendingDeferredSaveRef.current) return;
      if (!isDirtyRef.current) {
        pendingDeferredSaveRef.current = false;
        return;
      }
      pendingDeferredSaveRef.current = false;
      void handleManualSave();
    }, 0);
  }

  function clearActiveField(field: string) {
    setActiveField((current) => (current === field ? null : current));
  }

  function handleBlurWithDeferredSave(field: string) {
    clearActiveField(field);
    requestDeferredSave();
  }

  async function handleManualSave() {
    if (!canEditAny || task.isHeader) return;
    if (!isDirtyRef.current) return;

    if (scope === "main" && !task.isHeader && !draft.ownerDepartmentId) {
      setSaveState("error");
      setSaveMessage("Cần chọn phòng đầu mối trước khi lưu.");
      return;
    }

    setSaveState("saving");
    setSaveMessage(null);
    try {
      await onSaveRef.current(task.id, payloadRef.current);
      setSaveState("saved");
      window.setTimeout(() => {
        setSaveState("idle");
      }, 1200);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Không lưu được.";
      setSaveState("error");
      setSaveMessage(msg);
      setDraft(buildDraft(task));
    }
  }

  async function handleDeleteTask() {
    if (!onDelete || task.isHeader) return;
    setDeleteConfirmOpen(true);
  }

  async function confirmDelete() {
    if (!onDelete) return;
    setDeleteConfirmOpen(false);
    setDeleteError(null);
    try {
      await onDelete(task.id);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Không xóa được.";
      setDeleteError(msg);
    }
  }

  const ownerDepartment = departments.find((d) => d.id === draft.ownerDepartmentId);
  // Header tasks (inherited structural ancestors or user-added group rows) never carry an
  // owner — showing the plan department for them creates visual duplication with the child leaf.
  const effectiveOwner = task.isHeader
    ? null
    : ownerDepartment;
  const ownerDepartmentLabel = effectiveOwner
    ? getDepartmentLabel(effectiveOwner.code, effectiveOwner.name)
    : !task.isHeader && task.ownerDepartmentCode && task.ownerDepartmentName
      ? getDepartmentLabel(task.ownerDepartmentCode, task.ownerDepartmentName)
      : null;
  const ownerOptions = departments.filter((d) => scope !== "sub" || d.id === planDepartmentId);
  const canEditOwner = scope === "main" && fullEdit && !task.isHeader;
  const supportingOptions = departments.filter(
    (d) =>
      (scope !== "sub" || d.id !== planDepartmentId) &&
      d.id !== draft.ownerDepartmentId,
  );

  const depthPad = row.depth * 16 + 8;

  return (
    <tr className={rowClass(task)}>
      <td className="c-stt">{task.outlineIndex || "-"}</td>

      <td
        className={`c-content${row.depth === 1 ? " l2" : row.depth >= 2 ? " l3" : ""}`}
        style={{ paddingLeft: `${depthPad}px` }}
      >
        {fullEdit ? (
          <textarea
            className="inp-sm"
            onBlur={() => setActiveField((f) => (f === "title" ? null : f))}
            onChange={(e) => setDraft((prev) => ({ ...prev, title: e.target.value }))}
            onFocus={() => setActiveField("title")}
            rows={2}
            style={{ fontWeight: task.isHeader ? 700 : undefined, width: "100%" }}
            value={draft.title}
          />
        ) : (
          <div style={{ fontWeight: task.isHeader ? 700 : undefined }}>{task.title}</div>
        )}
        <div
          style={{
            fontSize: "10px",
            color: saveState === "error" ? "#c0392b" : "var(--ink3)",
            marginTop: "2px",
            minHeight: "14px",
            visibility: saveLabel(saveState) || saveMessage ? "visible" : "hidden",
          }}
        >
          {saveMessage ?? saveLabel(saveState) ?? "."}
        </div>
        {task.isLocked && (
          <div style={{ fontSize: "10px", color: "var(--ink4)", marginTop: "2px" }}>
            Kế thừa - Chỉ cập nhật tiến độ
          </div>
        )}
        {task.hasOpenComment && canResolveComment && (
          <button
            onClick={() => onOpenDetails(task)}
            style={{
              marginTop: "5px",
              display: "flex",
              alignItems: "center",
              gap: "5px",
              fontSize: "11px",
              fontWeight: 700,
              padding: "4px 10px",
              border: "1px solid #c0392b",
              borderRadius: "4px",
              background: "#fff",
              color: "#c0392b",
              cursor: "pointer",
              boxShadow: "none",
              letterSpacing: ".2px",
              animation: "pulse-resolve 1.8s ease-in-out infinite",
            }}
            title="Còn nhận xét chưa giải quyết — cần xử lý trước khi kiểm soát/phê duyệt"
            type="button"
          >
            <svg fill="none" height="11" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24" width="11">
              <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/>
              <line x1="12" x2="12" y1="9" y2="13"/>
              <line x1="12" x2="12.01" y1="17" y2="17"/>
            </svg>
            Giải quyết nhận xét
          </button>
        )}
      </td>

      <td className="c-member">
        {fullEdit && !task.isHeader ? (
          <textarea
            className="inp-sm"
            onBlur={() => setActiveField((f) => (f === "bksMemberText" ? null : f))}
            onChange={(e) => setDraft((prev) => ({ ...prev, bksMemberText: e.target.value }))}
            onFocus={() => setActiveField("bksMemberText")}
            placeholder="-"
            rows={2}
            value={draft.bksMemberText}
          />
        ) : (
          draft.bksMemberText || <span className="ph-empty">-</span>
        )}
      </td>

      <td className="c-leader">
        {fullEdit && !task.isHeader ? (
          <textarea
            className="inp-sm"
            onBlur={() => setActiveField((f) => (f === "ktnbLeaderText" ? null : f))}
            onChange={(e) => setDraft((prev) => ({ ...prev, ktnbLeaderText: e.target.value }))}
            onFocus={() => setActiveField("ktnbLeaderText")}
            placeholder="-"
            rows={2}
            value={draft.ktnbLeaderText}
          />
        ) : (
          draft.ktnbLeaderText || <span className="ph-empty">-</span>
        )}
      </td>

      <td className="c-worktype">
        {fullEdit && !task.isHeader && scope === "sub" ? (
          <select
            className="inp-sm"
            onChange={(e) => {
              setDraft((prev) => ({ ...prev, workType: Number(e.target.value) as WorkType }));
              requestDeferredSave();
            }}
            value={draft.workType}
          >
            {workTypeOptions.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        ) : (
          <span>{workTypeLabel(draft.workType)}</span>
        )}
      </td>

      <td className="c-priority">
        {task.isHeader ? (
          <span className="ph-empty">-</span>
        ) : fullEdit ? (
          <select
            className="inp-sm"
            onChange={(e) => {
              setDraft((prev) => ({ ...prev, priority: e.target.value }));
              requestDeferredSave();
            }}
            value={draft.priority}
          >
            {priorityOptions.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        ) : (
          <span className={`badge-priority priority-${draft.priority}`}>
            {priorityLabel(draft.priority)}
          </span>
        )}
      </td>

      <td className="c-complexity">
        {task.isHeader ? (
          <span className="ph-empty">-</span>
        ) : fullEdit ? (
          <select
            className="inp-sm"
            onChange={(e) => {
              setDraft((prev) => ({ ...prev, complexity: e.target.value }));
              requestDeferredSave();
            }}
            value={draft.complexity}
          >
            {complexityOptions.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        ) : (
          <span className={`badge-complexity complexity-${draft.complexity}`}>
            {complexityLabel(draft.complexity)}
          </span>
        )}
      </td>

      <td className="c-unit">
        {task.isHeader ? (
          <span className="ph-empty">-</span>
        ) : (
        <div className="unit-stack">
          <div className="unit-group">
            <div className="unit-label">Phòng đầu mối</div>
            <div className="tag-wrap">
              {ownerDepartmentLabel ? (
                <span className="tag tag--primary">{ownerDepartmentLabel}</span>
              ) : (
                <span className="ph-empty">Chưa gán</span>
              )}
              {canEditOwner && (
                <button
                  className="tag-add"
                  onClick={() => setOwnerPickerOpen(true)}
                  type="button"
                >
                  {ownerDepartment ? "Đổi" : "+ Thêm"}
                </button>
              )}
            </div>
          </div>

          <div className="unit-group">
            <div className="unit-label">Phòng phối hợp</div>
            <div className="tag-wrap">
              {draft.supportingDepartmentIds.length > 0 ? (
                draft.supportingDepartmentIds.map((id) => {
                  const dept = departments.find((d) => d.id === id);
                  return dept ? (
                    <span className="tag" key={id}>
                      {getDepartmentLabel(dept.code, dept.name)}
                      {fullEdit && !task.isHeader && (
                        <span
                          className="x"
                          onClick={() => {
                            setDraft((prev) => ({
                              ...prev,
                              supportingDepartmentIds: prev.supportingDepartmentIds.filter((x) => x !== id),
                            }));
                            requestDeferredSave();
                          }}
                        >
                          ✕
                        </span>
                      )}
                    </span>
                  ) : null;
                })
              ) : (
                <span className="ph-empty">Không có</span>
              )}

              {fullEdit && !task.isHeader && (
                <button
                  className="tag-add"
                  onClick={() => setSupportingPickerOpen(true)}
                  type="button"
                >
                  + Thêm
                </button>
              )}
            </div>
          </div>
        </div>
        )}

        <UnitPickerModal
          departments={ownerOptions}
          mode="single"
          onChange={(ids) => {
            const nextOwnerId = ids[0] ?? "";
            setDraft((prev) => ({
              ...prev,
              ownerDepartmentId: nextOwnerId,
              supportingDepartmentIds: prev.supportingDepartmentIds.filter((id) => id !== nextOwnerId),
            }));
            requestDeferredSave();
          }}
          onClose={() => setOwnerPickerOpen(false)}
          open={ownerPickerOpen}
          selectedIds={draft.ownerDepartmentId ? [draft.ownerDepartmentId] : []}
          title="Chọn phòng đầu mối"
        />
        <UnitPickerModal
          departments={supportingOptions}
          mode="multi"
          onChange={(ids) => {
            setDraft((prev) => ({
              ...prev,
              supportingDepartmentIds: ids,
            }));
          }}
          onClose={() => {
            setSupportingPickerOpen(false);
            requestDeferredSave();
          }}
          open={supportingPickerOpen}
          selectedIds={draft.supportingDepartmentIds}
          title="Chọn phòng phối hợp"
        />
      </td>

      <td className="c-deadline">
        <input
          className="inp-sm"
          disabled={!fullEdit || task.isHeader}
          onBlur={() => setActiveField((current) => (current === "deadline" ? null : current))}
          min={minDeadline}
          onChange={(e) => setDraft((prev) => ({ ...prev, deadline: e.target.value }))}
          onFocus={() => setActiveField("deadline")}
          type="date"
          value={draft.deadline}
        />
      </td>

      <td className="c-progress">
        <div className="xcell">
          <div className="xpreview">
            {draft.progressText ? (
              <div className="xpreview-text">{draft.progressText}</div>
            ) : (
              <div className="xempty">(Chưa có nội dung)</div>
            )}
          </div>
          <button
            className="xbtn"
            onClick={() => setProgressEditorOpen(true)}
            type="button"
          >
            Xem chi tiet
          </button>
        </div>
        <DetailEditorModal
          onClose={() => setProgressEditorOpen(false)}
          onConfirm={(next) => {
            setDraft((prev) => ({ ...prev, progressText: next }));
            requestDeferredSave();
          }}
          open={progressEditorOpen}
          readOnly={!(fullEdit || progressEdit) || task.isHeader}
          title="Tiến độ thực hiện"
          value={draft.progressText}
        />
      </td>

      <td className="c-workstatus">
        {task.isHeader ? (
          <span>-</span>
        ) : (
          <>
            <span className={`status-badge status-${draft.workStatus}`}>
              {workStatusOptions.find((o) => o.value === draft.workStatus)?.label ?? draft.workStatus}
            </span>
            {renderWorkflowButtons()}
          </>
        )}
      </td>

      <td className="c-person">
        {fullEdit && !task.isHeader ? (
          <select
            className="inp-sm"
            onBlur={() => handleBlurWithDeferredSave("assigneeUserId")}
            onChange={(e) => {
              setDraft((prev) => ({ ...prev, assigneeUserId: e.target.value || "" }));
            }}
            onFocus={() => setActiveField("assigneeUserId")}
            value={draft.assigneeUserId}
            style={{ width: "100%" }}
          >
            <option value="">-- Cán bộ đầu mối --</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName} ({u.username})
              </option>
            ))}
          </select>
        ) : (
          <div className="ph-wrap">
            {task.assigneeName ? (
              <>
                <div className="ph-name">{task.assigneeName}</div>
                <div className="ph-meta">{new Date(task.updatedAt).toLocaleDateString("vi-VN")}</div>
              </>
            ) : (
              <span className="ph-empty">(Chưa gán)</span>
            )}
            <button className="ph-btn" onClick={() => onOpenDetails(task)} type="button">
              Chi tiết
            </button>
          </div>
        )}
      </td>

      <td className="c-control">
        <div className="ph-wrap">
          {fullEdit && !task.isHeader ? (
            <select
              className="inp-sm"
              onBlur={() => handleBlurWithDeferredSave("controllerUserId")}
              onChange={(e) => {
                setDraft((prev) => ({ ...prev, controllerUserId: e.target.value || "" }));
              }}
              onFocus={() => setActiveField("controllerUserId")}
              value={draft.controllerUserId}
              style={{ width: "100%", marginBottom: "5px" }}
            >
              <option value="">-- Người kiểm soát --</option>
              {users.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.fullName} ({u.username})
                </option>
              ))}
            </select>
          ) : task.controllerUserId ? (
            <div className="ph-name">
              {users.find((u) => u.id === task.controllerUserId)?.fullName || task.assigneeName || "Người kiểm soát"}
            </div>
          ) : controllerHistory ? (
            <>
              <div className="ph-name">{controllerHistory.actorUserName || "Không rõ"}</div>
              <div className="ph-meta">{formatVnDateTime(controllerHistory.createdAt)}</div>
              {controllerHistory.comment && (
                <div className="ph-comment">"{controllerHistory.comment}"</div>
              )}
            </>
          ) : (
            <span className="ph-empty">(Chưa có người kiểm soát)</span>
          )}

          {controllerComments.map((c) => (
            <div className="ph-line-comment" key={c.id}>
              <div className="ph-meta">
                <strong>{c.authorUserName || "?"}</strong> · {formatVnDateTime(c.createdAt)}
                {c.isResolved && <span className="ph-resolved"> · Đã xử lý</span>}
              </div>
              <div className="ph-comment">{c.content}</div>
            </div>
          ))}

          {commentColumn === "control" ? (
            renderCommentForm()
          ) : canCommentControl ? (
            <button className="ph-btn" onClick={() => { setCommentColumn("control"); setCommentText(""); }} type="button">
              Thêm nhận xét
            </button>
          ) : (
            <button className="ph-btn" onClick={() => onOpenDetails(task)} type="button">
              Xem chi tiết
            </button>
          )}
        </div>
      </td>

      <td className="c-approver">
        <div className="ph-wrap">
          {approverHistory ? (
            <>
              <div className="ph-name">{approverHistory.actorUserName || "Không rõ"}</div>
              <div className="ph-meta">{formatVnDateTime(approverHistory.createdAt)}</div>
              {approverHistory.comment && (
                <div className="ph-comment">"{approverHistory.comment}"</div>
              )}
            </>
          ) : (
            <span className="ph-empty">(Chưa có người phê duyệt)</span>
          )}

          {approverComments.map((c) => (
            <div className="ph-line-comment" key={c.id}>
              <div className="ph-meta">
                <strong>{c.authorUserName || "?"}</strong> · {formatVnDateTime(c.createdAt)}
                {c.isResolved && <span className="ph-resolved"> · Đã xử lý</span>}
              </div>
              <div className="ph-comment">{c.content}</div>
            </div>
          ))}

          {commentColumn === "approve" ? (
            renderCommentForm()
          ) : canCommentApprove ? (
            <button className="ph-btn" onClick={() => { setCommentColumn("approve"); setCommentText(""); }} type="button">
              Thêm nhận xét
            </button>
          ) : (
            <button className="ph-btn" onClick={() => onOpenDetails(task)} type="button">
              Xem chi tiết
            </button>
          )}
        </div>
      </td>

      <td className="c-reason">
        <textarea
          className="inp-sm"
          disabled={!(fullEdit || progressEdit) || task.isHeader}
          onBlur={() => handleBlurWithDeferredSave("reasonNotCompleted")}
          onChange={(e) => setDraft((prev) => ({ ...prev, reasonNotCompleted: e.target.value }))}
          onFocus={() => setActiveField("reasonNotCompleted")}
          ref={reasonTextareaRef}
          rows={2}
          value={draft.reasonNotCompleted}
        />
      </td>

      <td className="c-note">
        <textarea
          className="inp-sm"
          disabled={!fullEdit || task.isHeader}
          onBlur={() => setActiveField((current) => (current === "noteText" ? null : current))}
          onChange={(e) => setDraft((prev) => ({ ...prev, noteText: e.target.value }))}
          onFocus={() => setActiveField("noteText")}
          ref={noteTextareaRef}
          rows={2}
          value={draft.noteText}
        />
      </td>

      <td className="c-act">
        <div className="row-actions">
          {canEditAny && !task.isHeader ? (
            <button
              className="bsave"
              disabled={!isDirty || saveState === "saving"}
              onClick={() => { void handleManualSave(); }}
              title="Lưu dòng này"
              type="button"
            >
              {saveState === "saving" ? "..." : "Lưu"}
            </button>
          ) : null}
          {fullEdit && onDelete ? (
            <button className="bdel" onClick={() => { void handleDeleteTask(); }} title="Xóa dòng này" type="button">
              <svg fill="none" height="11" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="11">
                <polyline points="3 6 5 6 21 6" />
                <path d="M19 6l-1 14H6L5 6" />
              </svg>
            </button>
          ) : null}
          {deleteError && (
            <div style={{ fontSize: "10px", color: "#c0392b", marginTop: 2 }} title={deleteError}>
              Xóa thất bại
            </div>
          )}
        </div>
      </td>

      <ConfirmDialog
        open={deleteConfirmOpen}
        title="Xóa dòng công việc"
        message={`Bạn có chắc muốn xóa "${task.title}"? Thao tác này không thể hoàn tác.`}
        confirmLabel="Xóa"
        danger
        onConfirm={() => { void confirmDelete(); }}
        onCancel={() => setDeleteConfirmOpen(false)}
      />
    </tr>
  );
}
