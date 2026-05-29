import { useEffect, useMemo, useState } from "react";
import { returnPlanSchema } from "@/features/plans/schema";
import type { TaskListItem } from "@/features/plan-tracking/types";

interface CommentDraft {
  taskId: string;
  content: string;
}

function toggleSelection(selected: string[], taskId: string) {
  return selected.includes(taskId)
    ? selected.filter((id) => id !== taskId)
    : [...selected, taskId];
}

export function ReturnPlanDialog({
  open,
  isPending,
  tasks,
  onClose,
  onSubmit,
}: {
  open: boolean;
  isPending: boolean;
  tasks: TaskListItem[];
  onClose: () => void;
  onSubmit: (values: {
    comment: string | null;
    lineComments: CommentDraft[];
  }) => Promise<void> | void;
}) {
  const [selectedTaskIds, setSelectedTaskIds] = useState<string[]>([]);
  const [comment, setComment] = useState("");
  const [commentMap, setCommentMap] = useState<Record<string, string>>({});
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    setSelectedTaskIds([]);
    setComment("");
    setCommentMap({});
    setError(null);
  }, [open]);

  const selectableTasks = useMemo(
    () => tasks.filter((task) => !task.isHeader),
    [tasks],
  );

  if (!open) {
    return null;
  }

  async function handleSubmit() {
    const lineComments = selectedTaskIds.map((taskId) => ({
      taskId,
      content: (commentMap[taskId] ?? "").trim(),
    }));

    const parsed = returnPlanSchema.safeParse({
      comment: comment.trim() || null,
      lineComments,
    });

    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message ?? "Dữ liệu chuyển trả không hợp lệ.");
      return;
    }

    setError(null);
    await onSubmit(parsed.data);
  }

  return (
    <div className="plans-modal-backdrop" role="presentation">
      <div aria-modal="true" className="plans-modal plans-modal--wide" role="dialog">
        <div className="plans-modal__header">
          <div>
            <p className="plans-modal__eyebrow">Workflow</p>
            <h2>Chuyển trả kế hoạch</h2>
          </div>
          <button className="plans-modal__close" onClick={onClose} type="button">
            Đóng
          </button>
        </div>

        <div className="plans-modal__body">
          <label className="plans-field">
            <span>Ghi chú chung (không bắt buộc)</span>
            <textarea
              className="plans-textarea"
              onChange={(event) => setComment(event.target.value)}
              rows={3}
              value={comment}
            />
          </label>

          <div className="plans-return-list">
            {selectableTasks.map((task) => {
              const checked = selectedTaskIds.includes(task.id);

              return (
                <div className="plans-return-item" key={task.id}>
                  <label className="plans-return-item__toggle">
                    <input
                      checked={checked}
                      onChange={() => {
                        setSelectedTaskIds((current) => toggleSelection(current, task.id));
                      }}
                      type="checkbox"
                    />
                    <span>
                      <strong>{task.outlineIndex ?? "-"}</strong> {task.title}
                    </span>
                  </label>

                  {checked ? (
                    <textarea
                      className="plans-textarea"
                      onChange={(event) =>
                        setCommentMap((current) => ({
                          ...current,
                          [task.id]: event.target.value,
                        }))
                      }
                      placeholder="Nhập nhận xét cho công việc này"
                      rows={3}
                      value={commentMap[task.id] ?? ""}
                    />
                  ) : null}
                </div>
              );
            })}
          </div>

          {error ? <p className="plans-error">{error}</p> : null}
        </div>

        <div className="plans-modal__footer">
          <button className="plans-button plans-button--ghost" onClick={onClose} type="button">
            Hủy
          </button>
          <button
            className="plans-button plans-button--danger"
            disabled={isPending}
            onClick={() => {
              void handleSubmit();
            }}
            type="button"
          >
            {isPending ? "Đang gửi..." : "Xác nhận chuyển trả"}
          </button>
        </div>
      </div>
    </div>
  );
}
