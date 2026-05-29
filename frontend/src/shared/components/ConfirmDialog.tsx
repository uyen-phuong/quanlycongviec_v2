import { useEffect, useRef } from "react";
import { createPortal } from "react-dom";
import "@/shared/components/ConfirmDialog.css";

interface ConfirmDialogProps {
  open: boolean;
  title?: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmDialog({
  open,
  title,
  message,
  confirmLabel = "Xác nhận",
  cancelLabel = "Hủy",
  danger = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  const confirmBtnRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (open) {
      confirmBtnRef.current?.focus();
    }
  }, [open]);

  useEffect(() => {
    if (!open) return;
    function handleKey(e: KeyboardEvent) {
      if (e.key === "Escape") onCancel();
    }
    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [open, onCancel]);

  if (!open) return null;

  return createPortal(
    <div className="cdlg-backdrop" role="presentation" onClick={onCancel}>
      <div
        className="cdlg"
        role="dialog"
        aria-modal="true"
        aria-labelledby={title ? "cdlg-title" : undefined}
        onClick={(e) => e.stopPropagation()}
      >
        {title && <div className="cdlg-title" id="cdlg-title">{title}</div>}
        <div className="cdlg-msg">{message}</div>
        <div className="cdlg-actions">
          <button className="cdlg-btn cdlg-btn--cancel" type="button" onClick={onCancel}>
            {cancelLabel}
          </button>
          <button
            ref={confirmBtnRef}
            className={`cdlg-btn${danger ? " cdlg-btn--danger" : " cdlg-btn--confirm"}`}
            type="button"
            onClick={onConfirm}
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>,
    document.body,
  );
}
