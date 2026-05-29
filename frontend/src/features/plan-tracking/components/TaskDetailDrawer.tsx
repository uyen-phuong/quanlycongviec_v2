import { useState } from "react";
import { toApiError } from "@/shared/api/client";
import { AttachmentList } from "@/features/attachments/AttachmentList";
import { AttachmentUploader } from "@/features/attachments/AttachmentUploader";
import {
  useDeleteAttachment,
  useTaskAttachments,
  useUploadTaskAttachment,
} from "@/features/attachments/hooks";
import type { LineComment, TaskListItem } from "@/features/plan-tracking/types";

type Tab = "details" | "attachments";

function formatDateTime(value: string | null) {
  if (!value) return "Chưa có";
  return new Intl.DateTimeFormat("vi-VN", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value));
}

function translateRole(role: string | null | undefined): string {
  switch (role) {
    case "controller": return "Kiểm soát viên";
    case "approver": return "Phê duyệt viên";
    case "creator": return "Người tạo";
    default: return role || "Không rõ";
  }
}

// Bám CLAUDE.md §10: locked sub task vẫn cho upload nếu user có quyền mutate task
function canUploadTaskAttachment(
  task: TaskListItem,
  scope: "main" | "sub",
  userRoles: string[],
) {
  if (task.isHeader) return false;

  if (!task.isLocked) {
    return scope === "main"
      ? userRoles.some((r) => ["VAN_THU", "ADMIN"].includes(r))
      : userRoles.some((r) => ["TRUONG_PHONG", "PHO_TRUONG_KTNB"].includes(r));
  }

  // locked (inherited) sub task
  return (
    scope === "sub" &&
    userRoles.some((r) => ["NHAN_VIEN", "TRUONG_PHONG", "PHO_TRUONG_KTNB"].includes(r))
  );
}

function canDeleteTaskAttachment(
  task: TaskListItem,
  scope: "main" | "sub",
  userRoles: string[],
) {
  return canUploadTaskAttachment(task, scope, userRoles);
}

export function TaskDetailDrawer({
  task,
  scope,
  userRoles,
  comments,
  canResolve,
  isResolving,
  onClose,
  onResolve,
}: {
  task: TaskListItem | null;
  scope: "main" | "sub";
  userRoles: string[];
  comments: LineComment[];
  canResolve: boolean;
  isResolving: boolean;
  onClose: () => void;
  onResolve: (commentId: string) => void;
}) {
  const [activeTab, setActiveTab] = useState<Tab>("details");
  const [uploadError, setUploadError] = useState<string | null>(null);

  const taskId = task?.id ?? null;
  const attachmentsQuery = useTaskAttachments(taskId);
  const uploadMutation = useUploadTaskAttachment(taskId);
  const deleteMutation = useDeleteAttachment("task", taskId);

  if (!task) return null;

  const allowUpload = canUploadTaskAttachment(task, scope, userRoles);
  const allowDelete = canDeleteTaskAttachment(task, scope, userRoles);

  async function handleUpload(file: File) {
    setUploadError(null);
    try {
      await uploadMutation.mutateAsync(file);
    } catch (error) {
      setUploadError(toApiError(error).message);
    }
  }

  async function handleDelete(attachmentId: string) {
    try {
      await deleteMutation.mutateAsync(attachmentId);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  return (
    <div className="comment-drawer open">
      <div className="comment-drawer__backdrop" onClick={onClose} />
      <aside className="comment-drawer__panel">
        <div className="comment-drawer__header">
          <div>
            <p className="comment-drawer__eyebrow">Chi tiết dòng công việc</p>
            <h2 className="comment-drawer__title">
              {task.outlineIndex ? `${task.outlineIndex}. ` : ""}
              {task.title}
            </h2>
          </div>
          <button className="comment-drawer__close" onClick={onClose} title="Đóng" type="button">
            ✕
          </button>
        </div>

        <div className="comment-drawer__tabs">
          <button
            className={`comment-drawer__tab${activeTab === "details" ? " comment-drawer__tab--active" : ""}`}
            onClick={() => setActiveTab("details")}
            type="button"
          >
            Tiến độ &amp; Nhận xét
            {comments.filter((c) => !c.isResolved).length > 0 && (
              <span className="comment-drawer__tab-badge">
                {comments.filter((c) => !c.isResolved).length}
              </span>
            )}
          </button>
          <button
            className={`comment-drawer__tab${activeTab === "attachments" ? " comment-drawer__tab--active" : ""}`}
            onClick={() => setActiveTab("attachments")}
            type="button"
          >
            File đính kèm {attachmentsQuery.data ? `(${attachmentsQuery.data.length})` : ""}
          </button>
        </div>

        <div className="comment-drawer__body">
          {activeTab === "details" ? (
            <>
              <section className="comment-drawer__section">
                <h3>Nội dung tiến độ</h3>
                <p>{task.progressText || "Chưa cập nhật tiến độ."}</p>
              </section>

              <section className="comment-drawer__section">
                <h3>Nguyên nhân chưa hoàn thành</h3>
                <p>{task.reasonNotCompleted || "Không có."}</p>
              </section>

              <section className="comment-drawer__section">
                <h3>Ghi chú</h3>
                <p>{task.noteText || "Không có."}</p>
              </section>

              <section className="comment-drawer__section">
                <h3>
                  Yêu cầu chỉnh sửa
                  {comments.filter((c) => !c.isResolved).length > 0 && (
                    <span className="comment-drawer__section-badge">
                      {comments.filter((c) => !c.isResolved).length} chưa giải quyết
                    </span>
                  )}
                </h3>
                {comments.length === 0 ? (
                  <p className="comment-drawer__empty">Không có yêu cầu chỉnh sửa cho dòng này.</p>
                ) : (
                  <div className="comment-drawer__timeline">
                    {comments.map((comment) => (
                      <article
                        className={`comment-card${!comment.isResolved ? " comment-card--open" : ""}`}
                        key={comment.id}
                      >
                        <div className="comment-card__meta">
                          <strong>{comment.authorUserName || "Không rõ người gửi"}</strong>
                          <span className="comment-card__role">{translateRole(comment.authorRole)}</span>
                          <span className="comment-card__time">{formatDateTime(comment.createdAt)}</span>
                        </div>
                        <p className="comment-card__content">{comment.content}</p>
                        <div className="comment-card__footer">
                          <span
                            className={
                              comment.isResolved
                                ? "comment-card__status comment-card__status--resolved"
                                : "comment-card__status comment-card__status--open"
                            }
                          >
                            {comment.isResolved ? "✓ Đã giải quyết" : "● Đang mở"}
                          </span>
                          {!comment.isResolved && canResolve ? (
                            <button
                              className="comment-card__resolve-btn"
                              disabled={isResolving}
                              onClick={() => onResolve(comment.id)}
                              type="button"
                            >
                              {isResolving ? "Đang xử lý..." : "✓ Đánh dấu đã giải quyết"}
                            </button>
                          ) : null}
                        </div>
                      </article>
                    ))}
                  </div>
                )}
              </section>
            </>
          ) : (
            <section className="comment-drawer__section">
              <h3>File đính kèm</h3>

              {allowUpload ? (
                <>
                  <AttachmentUploader
                    isUploading={uploadMutation.isPending}
                    onUpload={(file) => { void handleUpload(file); }}
                  />
                  {uploadError ? (
                    <p className="attachment-uploader__error">{uploadError}</p>
                  ) : null}
                </>
              ) : null}

              {attachmentsQuery.isLoading ? (
                <p>Đang tải danh sách file...</p>
              ) : attachmentsQuery.isError ? (
                <p>Không tải được file đính kèm.</p>
              ) : (
                <AttachmentList
                  attachments={attachmentsQuery.data ?? []}
                  canDelete={allowDelete}
                  isDeleting={deleteMutation.isPending}
                  onDelete={(id) => { void handleDelete(id); }}
                />
              )}
            </section>
          )}
        </div>
      </aside>
    </div>
  );
}
