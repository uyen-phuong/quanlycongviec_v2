import { useEffect, useMemo, useState } from "react";
import type { ImportMainPlanExcelResult } from "@/features/plans/types";

const maxBytes = 10 * 1024 * 1024;

function validateFile(file: File | null) {
  if (!file) {
    return "Cần chọn file .xlsx.";
  }

  if (!file.name.toLowerCase().endsWith(".xlsx")) {
    return "Chỉ hỗ trợ file .xlsx.";
  }

  if (file.size > maxBytes) {
    return "File phải nhỏ hơn hoặc bằng 10MB.";
  }

  return null;
}

export function ImportExcelDialog({
  open,
  isPending,
  result,
  onClose,
  onSubmit,
}: {
  open: boolean;
  isPending: boolean;
  result: ImportMainPlanExcelResult | null;
  onClose: () => void;
  onSubmit: (file: File) => Promise<void> | void;
}) {
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    setFile(null);
    setError(null);
  }, [open]);

  const fileLabel = useMemo(() => {
    if (!file) {
      return "Chưa chọn file";
    }

    return `${file.name} · ${(file.size / 1024 / 1024).toFixed(2)} MB`;
  }, [file]);

  if (!open) {
    return null;
  }

  async function handleSubmit() {
    const validationError = validateFile(file);
    if (validationError) {
      setError(validationError);
      return;
    }

    setError(null);
    await onSubmit(file!);
  }

  return (
    <div className="plans-modal-backdrop" role="presentation">
      <div aria-modal="true" className="plans-modal" role="dialog">
        <div className="plans-modal__header">
          <div>
            <p className="plans-modal__eyebrow">Excel</p>
            <h2>Tải lên phụ lục 03 vào kế hoạch tổng hợp</h2>
          </div>
          <button className="plans-modal__close" onClick={onClose} type="button">
            Đóng
          </button>
        </div>

        <div className="plans-modal__body">
          <label className="plans-upload-dropzone">
            <input
              accept=".xlsx"
              className="hidden"
              onChange={(event) => setFile(event.target.files?.[0] ?? null)}
              type="file"
            />
            <span className="plans-upload-dropzone__title">Chọn file Excel</span>
            <span className="plans-upload-dropzone__hint">
              Chỉ nhận `.xlsx`, tối đa 10MB. Import sẽ thay thế công việc hiện có của kế hoạch này.
            </span>
            <strong>{fileLabel}</strong>
          </label>

          {error ? <p className="plans-error">{error}</p> : null}

          {result ? (
            <div className="plans-import-result">
              <p>
                <strong>Import thành công:</strong> {result.fileName}
              </p>
              <p>
                Sheet `{result.sheetName}`, tổng dòng {result.totalRows}, task rows{" "}
                {result.taskRows}, replaced {result.replacedTasks}.
              </p>
            </div>
          ) : null}
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
            {isPending ? "Đang import..." : "Import ngay"}
          </button>
        </div>
      </div>
    </div>
  );
}
