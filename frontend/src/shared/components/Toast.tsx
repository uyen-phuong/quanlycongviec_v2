import { useEffect } from "react";
import { createPortal } from "react-dom";
import type { ToastState } from "@/shared/hooks/useToast";
import "@/shared/components/Toast.css";

interface ToastProps {
  toast: ToastState | null;
  onDismiss: () => void;
  /** Auto-dismiss after ms (default 4000, 0 = no auto-dismiss) */
  duration?: number;
}

export function Toast({ toast, onDismiss, duration = 4000 }: ToastProps) {
  useEffect(() => {
    if (!toast || duration === 0) return;
    const t = setTimeout(onDismiss, duration);
    return () => clearTimeout(t);
  }, [toast, duration, onDismiss]);

  if (!toast) return null;

  return createPortal(
    <div className={`toast toast--${toast.type}`} onClick={onDismiss} role="alert">
      <span className="toast-msg">{toast.message}</span>
      <button className="toast-close" type="button" onClick={onDismiss} aria-label="Đóng">×</button>
    </div>,
    document.body,
  );
}
