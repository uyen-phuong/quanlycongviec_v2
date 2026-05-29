import { useState } from "react";
import "@/features/plan-tracking/components/TaskTable.css";
import { TaskDetailDrawer } from "@/features/plan-tracking/components/TaskDetailDrawer";
import { TaskHeaderRow } from "@/features/plan-tracking/components/TaskHeaderRow";
import { TaskRow } from "@/features/plan-tracking/components/TaskRow";
import { getDepartmentLabel } from "@/shared/departmentLabels";
import type { DepartmentLookupDto } from "@/shared/api/dtos";
import type { ApprovalHistoryItem } from "@/features/plans/types";
import type {
  CreateTaskPayload,
  LineComment,
  SaveTaskPayload,
  TaskListItem,
  TaskRowViewModel,
} from "@/features/plan-tracking/types";

function AddTaskRow({
  planId,
  scope,
  departments,
  nextDisplayOrder,
  onSave,
  onCancel,
}: {
  planId: string;
  scope: "main" | "sub";
  departments: DepartmentLookupDto[];
  nextDisplayOrder: number;
  onSave: (payload: CreateTaskPayload) => Promise<void>;
  onCancel: () => void;
}) {
  const [title, setTitle] = useState("");
  const [isHeader, setIsHeader] = useState(false);
  const [ownerDepartmentId, setOwnerDepartmentId] = useState("");
  const [bksMemberText, setBksMemberText] = useState("");
  const [ktnbLeaderText, setKtnbLeaderText] = useState("");
  const [deadline, setDeadline] = useState("");
  const [noteText, setNoteText] = useState("");
  const [supportingDepartmentIds, setSupportingDepartmentIds] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const needsOwner = scope === "main" && !isHeader;
  const supportingOptions = departments.filter((d) => d.id !== ownerDepartmentId);

  async function handleSave() {
    if (!title.trim()) {
      setError("Nội dung công việc không được để trống.");
      return;
    }
    if (needsOwner && !ownerDepartmentId) {
      setError("Cần chọn đơn vị đầu mối.");
      return;
    }
    setError(null);
    setSaving(true);
    try {
      await onSave({
        planId,
        parentTaskId: null,
        outlineIndex: null,
        displayOrder: nextDisplayOrder,
        isHeader,
        title: title.trim(),
        workType: 0,
        workStatus: "not_started",
        deadline: deadline ? `${deadline}T00:00:00` : null,
        assigneeUserId: null,
        ownerDepartmentId: needsOwner ? ownerDepartmentId : null,
        bksMemberText: bksMemberText.trim() || null,
        ktnbLeaderText: ktnbLeaderText.trim() || null,
        noteText: noteText.trim() || null,
        progressText: null,
        reasonNotCompleted: null,
        supportingDepartmentIds,
      });
    } catch {
      setError("Lưu thất bại. Vui lòng thử lại.");
      setSaving(false);
    }
  }

  return (
    <tr className="task-table__add-row">
      <td />
      <td colSpan={14}>
        <div className="task-table__add-panel">
          <div className="task-table__add-fields">
            <label className="task-table__add-header-check">
              <input
                checked={isHeader}
                onChange={(e) => {
                  setIsHeader(e.target.checked);
                  if (e.target.checked) {
                    setOwnerDepartmentId("");
                    setSupportingDepartmentIds([]);
                    setBksMemberText("");
                    setKtnbLeaderText("");
                    setDeadline("");
                    setNoteText("");
                  }
                }}
                type="checkbox"
              />
              <span>Dòng nhóm</span>
            </label>
            <input
              autoFocus
              className="task-table__add-input"
              onChange={(e) => setTitle(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter" && (e.ctrlKey || e.metaKey)) void handleSave();
                if (e.key === "Escape") onCancel();
              }}
              placeholder="Nội dung công việc..."
              value={title}
            />
          </div>

          {!isHeader && (
            <div className="task-table__add-grid">
              {scope === "main" && (
                <>
                  <label className="task-table__add-field">
                    <span>Thành viên BKS</span>
                    <input
                      className="task-table__add-input"
                      onChange={(e) => setBksMemberText(e.target.value)}
                      placeholder="Nhập thành viên BKS"
                      value={bksMemberText}
                    />
                  </label>

                  <label className="task-table__add-field">
                    <span>Lãnh đạo KTNB</span>
                    <input
                      className="task-table__add-input"
                      onChange={(e) => setKtnbLeaderText(e.target.value)}
                      placeholder="Nhập lãnh đạo KTNB"
                      value={ktnbLeaderText}
                    />
                  </label>
                </>
              )}

              {needsOwner && (
                <label className="task-table__add-field">
                  <span>Phòng đầu mối</span>
                  <select
                    className="task-table__add-select"
                    onChange={(e) => {
                      const nextOwnerId = e.target.value;
                      setOwnerDepartmentId(nextOwnerId);
                      setSupportingDepartmentIds((prev) => prev.filter((id) => id !== nextOwnerId));
                    }}
                    value={ownerDepartmentId}
                  >
                    <option value="">-- Đơn vị đầu mối --</option>
                    {departments.map((d) => (
                      <option key={d.id} value={d.id}>
                        {d.code} - {getDepartmentLabel(d.code, d.name)}
                      </option>
                    ))}
                  </select>
                </label>
              )}

              {scope === "main" && (
                <label className="task-table__add-field">
                  <span>Phòng phối hợp</span>
                  <select
                    className="task-table__add-select task-table__add-select--multi"
                    multiple
                    onChange={(e) => {
                      const values = Array.from(e.target.selectedOptions, (option) => option.value);
                      setSupportingDepartmentIds(values);
                    }}
                    value={supportingDepartmentIds}
                  >
                    {supportingOptions.map((d) => (
                      <option key={d.id} value={d.id}>
                        {d.code} - {getDepartmentLabel(d.code, d.name)}
                      </option>
                    ))}
                  </select>
                </label>
              )}

              <label className="task-table__add-field">
                <span>Hạn hoàn thành</span>
                <input
                  className="task-table__add-input"
                  onChange={(e) => setDeadline(e.target.value)}
                  type="date"
                  value={deadline}
                />
              </label>

              <label className="task-table__add-field task-table__add-field--wide">
                <span>Ghi chú</span>
                <textarea
                  className="task-table__add-textarea"
                  onChange={(e) => setNoteText(e.target.value)}
                  placeholder="Ghi chú thêm nếu cần"
                  rows={2}
                  value={noteText}
                />
              </label>
            </div>
          )}

          {error && <p className="task-table__add-error">{error}</p>}

          <div className="task-table__add-actions">
            <button
              className="task-table__add-btn task-table__add-btn--primary"
              disabled={saving}
              onClick={() => { void handleSave(); }}
              type="button"
            >
              {saving ? "Lưu..." : "Lưu"}
            </button>
            <button
              className="task-table__add-btn"
              disabled={saving}
              onClick={onCancel}
              type="button"
            >
              Hủy
            </button>
            <span className="task-table__add-hint">Ctrl+Enter để lưu nhanh</span>
          </div>
        </div>
      </td>
    </tr>
  );
}

export function TaskTable({
  rows,
  scope,
  userRoles,
  departments,
  minDeadline,
  planDepartmentId,
  planId,
  planStatus,
  canAddTask,
  selectedTask,
  selectedComments,
  allComments,
  approvalHistory,
  canResolveComment,
  isResolvingComment,
  onOpenDetails,
  onCloseDetails,
  onResolveComment,
  onAddComment,
  onSave,
  onCreateTask,
  onDeleteTask,
}: {
  rows: TaskRowViewModel[];
  scope: "main" | "sub";
  userRoles: string[];
  departments: DepartmentLookupDto[];
  minDeadline: string;
  planDepartmentId: string | null;
  planId?: string;
  planStatus?: string | null;
  canAddTask?: boolean;
  selectedTask: TaskListItem | null;
  selectedComments: LineComment[];
  allComments: LineComment[];
  approvalHistory: ApprovalHistoryItem[];
  canResolveComment: boolean;
  isResolvingComment: boolean;
  onOpenDetails: (task: TaskListItem) => void;
  onCloseDetails: () => void;
  onResolveComment: (commentId: string) => void;
  onAddComment: (taskId: string, content: string) => Promise<void>;
  onSave: (taskId: string, payload: SaveTaskPayload) => Promise<void>;
  onCreateTask?: (payload: CreateTaskPayload) => Promise<void>;
  onDeleteTask?: (taskId: string) => Promise<void>;
}) {
  const [isAdding, setIsAdding] = useState(false);
  const nextDisplayOrder = (rows.length + 1) * 10;

  return (
    <>
      <div className="twrap">
        <table className="task-table">
          <TaskHeaderRow />
          <tbody>
            {rows.map((row) => (
              <TaskRow
                allComments={allComments}
                approvalHistory={approvalHistory}
                canResolveComment={canResolveComment}
                departments={departments}
                key={row.task.id}
                minDeadline={minDeadline}
                onAddComment={onAddComment}
                onDelete={onDeleteTask}
                onOpenDetails={onOpenDetails}
                onSave={onSave}
                planDepartmentId={planDepartmentId}
                planStatus={planStatus ?? null}
                row={row}
                scope={scope}
                userRoles={userRoles}
              />
            ))}
            {isAdding && planId && onCreateTask && (
              <AddTaskRow
                departments={departments}
                nextDisplayOrder={nextDisplayOrder}
                onCancel={() => setIsAdding(false)}
                onSave={async (payload) => {
                  await onCreateTask(payload);
                  setIsAdding(false);
                }}
                planId={planId}
                scope={scope}
              />
            )}
          </tbody>
        </table>
      </div>

      {canAddTask && !isAdding && (
        <button
          className="task-table__add-trigger"
          onClick={() => setIsAdding(true)}
          type="button"
        >
          + Thêm công việc
        </button>
      )}

      <TaskDetailDrawer
        canResolve={canResolveComment}
        comments={selectedComments}
        isResolving={isResolvingComment}
        onClose={onCloseDetails}
        onResolve={onResolveComment}
        scope={scope}
        task={selectedTask}
        userRoles={userRoles}
      />
    </>
  );
}
