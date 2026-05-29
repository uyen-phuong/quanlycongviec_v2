import type { LineComment, TaskListItem } from "@/features/plan-tracking/types";

function formatDateTime(value: string | null) {
  if (!value) {
    return "Chưa có";
  }

  return new Intl.DateTimeFormat("vi-VN", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value));
}

export function CommentDrawer({
  task,
  comments,
  canResolve,
  isResolving,
  onClose,
  onResolve,
}: {
  task: TaskListItem | null;
  comments: LineComment[];
  canResolve: boolean;
  isResolving: boolean;
  onClose: () => void;
  onResolve: (commentId: string) => void;
}) {
  if (!task) {
    return null;
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
          <button className="comment-drawer__close" onClick={onClose} type="button">
            Đóng
          </button>
        </div>

        <div className="comment-drawer__body">
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
            <h3>Nhận xét</h3>
            {comments.length === 0 ? (
              <p>Chưa có nhận xét cho dòng này.</p>
            ) : (
              <div className="comment-drawer__timeline">
                {comments.map((comment) => (
                  <article className="comment-card" key={comment.id}>
                    <div className="comment-card__meta">
                      <strong>{comment.authorUserName || "Không rõ người gửi"}</strong>
                      <span>{comment.authorRole}</span>
                      <span>{formatDateTime(comment.createdAt)}</span>
                    </div>
                    <p className="comment-card__content">{comment.content}</p>
                    <div className="comment-card__footer">
                      <span
                        className={
                          comment.isResolved
                            ? "comment-card__status comment-card__status--resolved"
                            : "comment-card__status"
                        }
                      >
                        {comment.isResolved ? "Đã giải quyết" : "Đang mở"}
                      </span>
                      {!comment.isResolved && canResolve ? (
                        <button
                          className="comment-card__action"
                          disabled={isResolving}
                          onClick={() => onResolve(comment.id)}
                          type="button"
                        >
                          {isResolving ? "Đang xử lý..." : "Giải quyết"}
                        </button>
                      ) : null}
                    </div>
                  </article>
                ))}
              </div>
            )}
          </section>
        </div>
      </aside>
    </div>
  );
}
