import { useState } from "react";
import { useAuth } from "@/shared/auth/useAuth";
import {
  usePersonalTasks,
  useCreatePersonalTask,
  useSavePersonalTask,
  useDeletePersonalTask,
} from "@/features/plan-tracking/hooks";
import type { TaskListItem, WorkStatus } from "@/features/plan-tracking/types";
import { ConfirmDialog } from "@/shared/components/ConfirmDialog";

export function PersonalTasksPage() {
  const auth = useAuth();
  const tasksQuery = usePersonalTasks();
  const createMutation = useCreatePersonalTask();
  const saveMutation = useSavePersonalTask();
  const deleteMutation = useDeletePersonalTask();

  const [newTitle, setNewTitle] = useState("");
  const [newDeadline, setNewDeadline] = useState("");
  const [newPriority, setNewPriority] = useState("normal");
  const [newComplexity, setNewComplexity] = useState("medium");
  const [newNote, setNewNote] = useState("");
  const [error, setError] = useState<string | null>(null);

  // Edit states
  const [editingTask, setEditingTask] = useState<TaskListItem | null>(null);
  const [editTitle, setEditTitle] = useState("");
  const [editDeadline, setEditDeadline] = useState("");
  const [editPriority, setEditPriority] = useState("normal");
  const [editComplexity, setEditComplexity] = useState("medium");
  const [editNote, setEditNote] = useState("");

  // Delete state
  const [taskToDelete, setTaskToDelete] = useState<TaskListItem | null>(null);

  const tasks = tasksQuery.data ?? [];

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    if (!newTitle.trim()) {
      setError("Vui lòng nhập nội dung công việc.");
      return;
    }
    setError(null);
    try {
      await createMutation.mutateAsync({
        title: newTitle.trim(),
        deadline: newDeadline ? `${newDeadline}T00:00:00` : null,
        noteText: newNote.trim() || null,
        priority: newPriority,
        complexity: newComplexity,
        displayOrder: (tasks.length + 1) * 10,
      });
      setNewTitle("");
      setNewDeadline("");
      setNewNote("");
      setNewPriority("normal");
      setNewComplexity("medium");
    } catch {
      setError("Không thể tạo công việc cá nhân. Vui lòng thử lại.");
    }
  }

  async function handleStatusChange(task: TaskListItem, status: WorkStatus) {
    try {
      await saveMutation.mutateAsync({
        taskId: task.id,
        payload: {
          parentTaskId: task.parentTaskId,
          outlineIndex: task.outlineIndex,
          displayOrder: task.displayOrder,
          isHeader: task.isHeader,
          title: task.title,
          workType: task.workType,
          workStatus: status,
          deadline: task.deadline,
          assigneeUserId: task.assigneeUserId,
          controllerUserId: task.controllerUserId,
          ownerDepartmentId: task.ownerDepartmentId,
          bksMemberText: task.bksMemberText,
          ktnbLeaderText: task.ktnbLeaderText,
          noteText: task.noteText,
          progressText: task.progressText,
          reasonNotCompleted: task.reasonNotCompleted,
          priority: task.priority,
          complexity: task.complexity,
          supportingDepartmentIds: task.supportingDepartmentIds,
        },
      });
    } catch {
      alert("Cập nhật trạng thái thất bại.");
    }
  }

  function startEdit(task: TaskListItem) {
    setEditingTask(task);
    setEditTitle(task.title);
    setEditDeadline(task.deadline ? task.deadline.slice(0, 10) : "");
    setEditPriority(task.priority || "normal");
    setEditComplexity(task.complexity || "medium");
    setEditNote(task.noteText ?? "");
  }

  async function handleSaveEdit(e: React.FormEvent) {
    e.preventDefault();
    if (!editingTask) return;
    if (!editTitle.trim()) return;

    try {
      await saveMutation.mutateAsync({
        taskId: editingTask.id,
        payload: {
          parentTaskId: editingTask.parentTaskId,
          outlineIndex: editingTask.outlineIndex,
          displayOrder: editingTask.displayOrder,
          isHeader: editingTask.isHeader,
          title: editTitle.trim(),
          workType: editingTask.workType,
          workStatus: editingTask.workStatus,
          deadline: editDeadline ? `${editDeadline}T00:00:00` : null,
          assigneeUserId: editingTask.assigneeUserId,
          controllerUserId: editingTask.controllerUserId,
          ownerDepartmentId: editingTask.ownerDepartmentId,
          bksMemberText: editingTask.bksMemberText,
          ktnbLeaderText: editingTask.ktnbLeaderText,
          noteText: editNote.trim() || null,
          progressText: editingTask.progressText,
          reasonNotCompleted: editingTask.reasonNotCompleted,
          priority: editPriority,
          complexity: editComplexity,
          supportingDepartmentIds: editingTask.supportingDepartmentIds,
        },
      });
      setEditingTask(null);
    } catch {
      alert("Lưu chỉnh sửa thất bại.");
    }
  }

  async function handleDeleteConfirm() {
    if (!taskToDelete) return;
    try {
      await deleteMutation.mutateAsync(taskToDelete.id);
      setTaskToDelete(null);
    } catch {
      alert("Xóa công việc thất bại.");
    }
  }

  return (
    <main className="plan-tracking-page" style={{ padding: "2rem" }}>
      <header className="plan-tracking-page__header" style={{ marginBottom: "2rem" }}>
        <div>
          <h1 className="plan-tracking-page__title">Công việc cá nhân của tôi</h1>
          <p className="plan-tracking-page__subtitle">
            Ghi chú và tự quản lý các đầu việc cá nhân. Không áp dụng quy trình xét duyệt.
          </p>
        </div>
      </header>

      <div style={{ display: "grid", gridTemplateColumns: "1fr 2fr", gap: "2rem", alignItems: "start" }}>
        {/* Create Task Form */}
        <section className="task-table__add-panel" style={{ padding: "1.5rem", borderRadius: "16px", backgroundColor: "#fff", border: "1px stroke var(--stone-200)", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
          <h3 style={{ fontSize: "1.1rem", fontWeight: 700, marginBottom: "1rem", color: "var(--ink)" }}>Tạo nhanh công việc</h3>
          <form onSubmit={(e) => { void handleCreate(e); }} style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
            <div>
              <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Nội dung công việc *</label>
              <input
                className="task-table__add-input"
                onChange={(e) => setNewTitle(e.target.value)}
                placeholder="Nhập nội dung cần làm..."
                required
                style={{ width: "100%" }}
                type="text"
                value={newTitle}
              />
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "0.5rem" }}>
              <div>
                <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Độ khẩn</label>
                <select
                  className="task-table__add-select"
                  onChange={(e) => setNewPriority(e.target.value)}
                  style={{ width: "100%" }}
                  value={newPriority}
                >
                  <option value="normal">Bình thường</option>
                  <option value="urgent">Khẩn</option>
                  <option value="critical">Hỏa tốc</option>
                </select>
              </div>

              <div>
                <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Độ phức tạp</label>
                <select
                  className="task-table__add-select"
                  onChange={(e) => setNewComplexity(e.target.value)}
                  style={{ width: "100%" }}
                  value={newComplexity}
                >
                  <option value="low">Thấp</option>
                  <option value="medium">Trung bình</option>
                  <option value="high">Cao</option>
                </select>
              </div>
            </div>

            <div>
              <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Hạn hoàn thành</label>
              <input
                className="task-table__add-input"
                onChange={(e) => setNewDeadline(e.target.value)}
                style={{ width: "100%" }}
                type="date"
                value={newDeadline}
              />
            </div>

            <div>
              <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Ghi chú chi tiết</label>
              <textarea
                className="task-table__add-textarea"
                onChange={(e) => setNewNote(e.target.value)}
                placeholder="Mô tả thêm..."
                rows={3}
                style={{ width: "100%" }}
                value={newNote}
              />
            </div>

            {error && <p style={{ color: "var(--red-600)", fontSize: "0.8rem" }}>{error}</p>}

            <button
              className="task-table__add-btn task-table__add-btn--primary"
              disabled={createMutation.isPending}
              style={{ width: "100%", padding: "0.75rem", borderRadius: "8px", fontWeight: 700 }}
              type="submit"
            >
              {createMutation.isPending ? "Đang tạo..." : "Thêm công việc"}
            </button>
          </form>
        </section>

        {/* Task List Grid */}
        <section>
          {tasksQuery.isLoading ? (
            <div style={{ padding: "3rem", textCombineUpright: "inherit", color: "var(--stone-500)", textAlign: "center" }}>
              Đang tải danh sách công việc cá nhân...
            </div>
          ) : tasks.length === 0 ? (
            <div style={{ padding: "5rem 2rem", border: "1px dashed var(--stone-300)", borderRadius: "16px", backgroundColor: "#fff", textAlign: "center" }}>
              <p style={{ color: "var(--stone-500)", fontSize: "0.95rem" }}>Chưa có công việc cá nhân nào.</p>
              <p style={{ color: "var(--stone-400)", fontSize: "0.8rem", marginTop: "0.5rem" }}>Hãy sử dụng biểu mẫu bên trái để bắt đầu lập kế hoạch cá nhân của bạn.</p>
            </div>
          ) : (
            <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
              {tasks.map((task) => (
                <div
                  key={task.id}
                  style={{
                    backgroundColor: "#fff",
                    borderRadius: "16px",
                    border: "1px solid var(--stone-200)",
                    padding: "1.25rem",
                    boxShadow: "0 1px 2px rgba(0,0,0,0.02)",
                    display: "flex",
                    flexDirection: "column",
                    gap: "0.75rem",
                    transition: "transform 0.2s, box-shadow 0.2s",
                  }}
                  className="personal-task-card"
                >
                  <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: "1rem" }}>
                    <div>
                      <h4 style={{ fontSize: "1.05rem", fontWeight: 700, color: "var(--ink)", margin: 0, textDecoration: task.workStatus === "done" ? "line-through" : "none", opacity: task.workStatus === "done" ? 0.6 : 1 }}>
                        {task.title}
                      </h4>
                      {task.deadline && (
                        <p style={{ fontSize: "0.8rem", color: "var(--stone-500)", margin: "0.25rem 0 0" }}>
                          Hạn chót: {new Date(task.deadline).toLocaleDateString("vi-VN")}
                        </p>
                      )}
                    </div>

                    <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
                      {/* Direct dropdown edit of basic status */}
                      <select
                        onChange={(e) => void handleStatusChange(task, e.target.value as WorkStatus)}
                        style={{
                          padding: "0.3rem 0.6rem",
                          borderRadius: "20px",
                          fontSize: "0.8rem",
                          fontWeight: 700,
                          backgroundColor:
                            task.workStatus === "done" ? "rgba(16, 185, 129, 0.1)" :
                            task.workStatus === "in_progress" ? "rgba(59, 130, 246, 0.1)" :
                            "rgba(107, 114, 128, 0.1)",
                          color:
                            task.workStatus === "done" ? "#10b981" :
                            task.workStatus === "in_progress" ? "#3b82f6" :
                            "#6b7280",
                          border: "none",
                          cursor: "pointer",
                        }}
                        value={task.workStatus}
                      >
                        <option value="not_started">Chưa bắt đầu</option>
                        <option value="in_progress">Đang làm</option>
                        <option value="done">Hoàn thành</option>
                      </select>
                    </div>
                  </div>

                  {task.noteText && (
                    <p style={{ fontSize: "0.85rem", color: "var(--stone-600)", margin: 0, backgroundColor: "var(--stone-50)", padding: "0.5rem 0.75rem", borderRadius: "8px" }}>
                      {task.noteText}
                    </p>
                  )}

                  <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: "0.5rem", borderTop: "1px solid var(--stone-100)", paddingTop: "0.75rem" }}>
                    <div style={{ display: "flex", gap: "0.5rem" }}>
                      <span className={`task-badge task-badge--priority-${task.priority}`} style={{ fontSize: "0.75rem", padding: "0.2rem 0.5rem", borderRadius: "12px" }}>
                        {task.priority === "critical" ? "Hỏa tốc" : task.priority === "urgent" ? "Khẩn" : "Thường"}
                      </span>
                      <span className={`task-badge task-badge--complexity-${task.complexity}`} style={{ fontSize: "0.75rem", padding: "0.2rem 0.5rem", borderRadius: "12px" }}>
                        {task.complexity === "high" ? "Phức tạp: Cao" : task.complexity === "medium" ? "Phức tạp: Trung bình" : "Phức tạp: Thấp"}
                      </span>
                    </div>

                    <div style={{ display: "flex", gap: "0.5rem" }}>
                      <button
                        onClick={() => startEdit(task)}
                        style={{ background: "none", border: "none", color: "var(--indigo-600)", fontSize: "0.8rem", cursor: "pointer", fontWeight: 600 }}
                        type="button"
                      >
                        Sửa
                      </button>
                      <button
                        onClick={() => setTaskToDelete(task)}
                        style={{ background: "none", border: "none", color: "var(--red-600)", fontSize: "0.8rem", cursor: "pointer", fontWeight: 600 }}
                        type="button"
                      >
                        Xóa
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>

      {/* Edit Task Modal */}
      {editingTask && (
        <div style={{ position: "fixed", top: 0, left: 0, right: 0, bottom: 0, backgroundColor: "rgba(0,0,0,0.4)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 100 }}>
          <form onSubmit={(e) => { void handleSaveEdit(e); }} className="task-table__add-panel" style={{ width: "450px", padding: "2rem", borderRadius: "20px", backgroundColor: "#fff" }}>
            <h3 style={{ fontSize: "1.2rem", fontWeight: 800, marginBottom: "1.5rem", color: "var(--ink)" }}>Chỉnh sửa công việc</h3>
            
            <div style={{ display: "flex", flexDirection: "column", gap: "1.2rem" }}>
              <div>
                <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Nội dung công việc</label>
                <input
                  className="task-table__add-input"
                  onChange={(e) => setEditTitle(e.target.value)}
                  required
                  style={{ width: "100%" }}
                  type="text"
                  value={editTitle}
                />
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "0.5rem" }}>
                <div>
                  <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Độ khẩn</label>
                  <select
                    className="task-table__add-select"
                    onChange={(e) => setEditPriority(e.target.value)}
                    style={{ width: "100%" }}
                    value={editPriority}
                  >
                    <option value="normal">Bình thường</option>
                    <option value="urgent">Khẩn</option>
                    <option value="critical">Hỏa tốc</option>
                  </select>
                </div>

                <div>
                  <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Độ phức tạp</label>
                  <select
                    className="task-table__add-select"
                    onChange={(e) => setEditComplexity(e.target.value)}
                    style={{ width: "100%" }}
                    value={editComplexity}
                  >
                    <option value="low">Thấp</option>
                    <option value="medium">Trung bình</option>
                    <option value="high">Cao</option>
                  </select>
                </div>
              </div>

              <div>
                <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Hạn hoàn thành</label>
                <input
                  className="task-table__add-input"
                  onChange={(e) => setEditDeadline(e.target.value)}
                  style={{ width: "100%" }}
                  type="date"
                  value={editDeadline}
                />
              </div>

              <div>
                <label style={{ display: "block", fontSize: "0.8rem", fontWeight: 600, marginBottom: "0.3rem", color: "var(--stone-600)" }}>Ghi chú chi tiết</label>
                <textarea
                  className="task-table__add-textarea"
                  onChange={(e) => setEditNote(e.target.value)}
                  rows={4}
                  style={{ width: "100%" }}
                  value={editNote}
                />
              </div>
            </div>

            <div style={{ display: "flex", gap: "0.75rem", marginTop: "2rem", justifyContent: "flex-end" }}>
              <button
                className="task-table__add-btn task-table__add-btn--primary"
                type="submit"
              >
                Lưu thay đổi
              </button>
              <button
                className="task-table__add-btn"
                onClick={() => setEditingTask(null)}
                type="button"
              >
                Hủy
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Confirm Delete Dialog */}
      {taskToDelete && (
        <ConfirmDialog
          title="Xóa công việc"
          message={`Bạn có chắc chắn muốn xóa công việc "${taskToDelete.title}"?`}
          open={true}
          onConfirm={() => { void handleDeleteConfirm(); }}
          onCancel={() => setTaskToDelete(null)}
        />
      )}
    </main>
  );
}
