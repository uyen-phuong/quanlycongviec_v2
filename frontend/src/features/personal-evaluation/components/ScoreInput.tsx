import { useEffect, useState } from "react";

interface ScoreInputProps {
  value: number | null;
  onCommit: (value: number | null) => void;
  readOnly?: boolean;
  max?: number;
  min?: number;
  className?: string;
}

export function ScoreInput({ value, onCommit, readOnly, max = 20, min = 0, className }: ScoreInputProps) {
  const [draft, setDraft] = useState<string>(value == null ? "" : String(value));

  useEffect(() => {
    setDraft(value == null ? "" : String(value));
  }, [value]);

  function commit() {
    const trimmed = draft.trim();
    if (trimmed === "") {
      if (value !== null) onCommit(null);
      return;
    }
    const n = Number(trimmed);
    if (!Number.isFinite(n)) {
      setDraft(value == null ? "" : String(value));
      return;
    }
    const clamped = Math.min(Math.max(n, min), max);
    if (clamped !== value) onCommit(clamped);
  }

  return (
    <input
      className={`inp-sm ${className ?? ""}`}
      type="number"
      step="0.1"
      min={min}
      max={max}
      value={draft}
      readOnly={readOnly}
      data-readonly={readOnly ? "true" : undefined}
      onChange={(e) => setDraft(e.target.value)}
      onBlur={commit}
    />
  );
}
