import { useEffect, useState } from "react";
import { planFormSchema } from "@/features/plans/schema";
import type { PlanFormValues } from "@/features/plans/types";

function toMonthValue(values: PlanFormValues) {
  return `${values.year}-${String(values.month).padStart(2, "0")}`;
}

export function PlanFormDialog({
  open,
  title,
  initialValues,
  isPending,
  onClose,
  onSubmit,
}: {
  open: boolean;
  title: string;
  initialValues: PlanFormValues;
  isPending: boolean;
  onClose: () => void;
  onSubmit: (values: PlanFormValues) => Promise<void> | void;
}) {
  const [monthValue, setMonthValue] = useState(() => toMonthValue(initialValues));
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    setMonthValue(toMonthValue(initialValues));
    setError(null);
  }, [initialValues, open]);

  if (!open) {
    return null;
  }

  async function handleSubmit() {
    const [yearText, monthText] = monthValue.split("-");
    const parsed = planFormSchema.safeParse({
      year: Number(yearText),
      month: Number(monthText),
      departmentId: null,
    });

    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message ?? "Dữ liệu không hợp lệ.");
      return;
    }

    setError(null);
    await onSubmit(parsed.data);
  }

  return (
    <div className="plans-modal-backdrop" role="presentation">
      <div
        aria-labelledby="plan-form-title"
        aria-modal="true"
        className="plans-modal"
        role="dialog"
      >
        <div className="plans-modal__header">
          <div>
            <p className="plans-modal__eyebrow">Kế hoạch</p>
            <h2 id="plan-form-title">{title}</h2>
          </div>
          <button className="plans-modal__close" onClick={onClose} type="button">
            Đóng
          </button>
        </div>

        <div className="plans-modal__body">
          <label className="plans-field">
            <span>Kỳ kế hoạch</span>
            <input
              className="plans-input"
              onChange={(event) => setMonthValue(event.target.value)}
              type="month"
              value={monthValue}
            />
          </label>

          {error ? <p className="plans-error">{error}</p> : null}
        </div>

        <div className="plans-modal__footer">
          <button className="plans-button plans-button--ghost" onClick={onClose} type="button">
            Hủy
          </button>
          <button
            className="plans-button plans-button--primary"
            disabled={isPending}
            onClick={() => {
              void handleSubmit();
            }}
            type="button"
          >
            {isPending ? "Đang lưu..." : "Lưu"}
          </button>
        </div>
      </div>
    </div>
  );
}
