import { useEffect, useState } from "react";

type Props = {
  open: boolean;
  title: string;
  value: string;
  readOnly?: boolean;
  onClose: () => void;
  onConfirm: (next: string) => void;
};

export function DetailEditorModal({
  open,
  title,
  value,
  readOnly,
  onClose,
  onConfirm,
}: Props) {
  const [draft, setDraft] = useState(value);

  useEffect(() => {
    if (open) setDraft(value);
  }, [open, value]);

  useEffect(() => {
    if (!open) return;
    function onKey(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, onClose]);

  if (!open) return null;

  function handleConfirm() {
    onConfirm(draft);
    onClose();
  }

  return (
    <div className="mo show" onClick={onClose}>
      <div className="mw detail-box" onClick={(e) => e.stopPropagation()}>
        <div className="mh">
          <div className="mt2">{title}</div>
          <button className="mclose" onClick={onClose} type="button" aria-label="Đóng">
            ×
          </button>
        </div>
        <div className="mc">
          <textarea
            autoFocus
            className="detail-editor"
            onChange={(e) => setDraft(e.target.value)}
            placeholder="Nhập nội dung..."
            readOnly={readOnly}
            value={draft}
          />
        </div>
        {!readOnly && (
          <div className="mf">
            <button className="btn-g" onClick={handleConfirm} type="button">
              Xác nhận
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
