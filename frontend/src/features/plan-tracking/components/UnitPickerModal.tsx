import { useEffect } from "react";
import { getDepartmentLabel } from "@/shared/departmentLabels";
import type { DepartmentLookupDto } from "@/shared/api/dtos";

type Props = {
  open: boolean;
  title: string;
  mode: "single" | "multi";
  departments: DepartmentLookupDto[];
  selectedIds: string[];
  onClose: () => void;
  onChange: (nextIds: string[]) => void;
};

export function UnitPickerModal({
  open,
  title,
  mode,
  departments,
  selectedIds,
  onClose,
  onChange,
}: Props) {
  useEffect(() => {
    if (!open) return;
    function onKey(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, onClose]);

  if (!open) return null;

  const selected = new Set(selectedIds);

  function handlePick(id: string) {
    if (mode === "single") {
      onChange([id]);
      onClose();
      return;
    }
    const next = new Set(selected);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    onChange([...next].sort((a, b) => a.localeCompare(b)));
  }

  return (
    <div className="mo show" onClick={onClose}>
      <div className="mw unit-picker-modal" onClick={(e) => e.stopPropagation()}>
        <div className="mh">
          <div className="mt2">{title}</div>
          <button className="mclose" onClick={onClose} type="button" aria-label="Dong">
            ×
          </button>
        </div>
        <div className="mc">
          <div className="units-grid">
            {departments.map((d) => {
              const checked = selected.has(d.id);
              return (
                <label className="upill-wrap" key={d.id}>
                  <input
                    checked={checked}
                    className="uchk"
                    onChange={() => handlePick(d.id)}
                    type={mode === "single" ? "radio" : "checkbox"}
                    name="unit-picker"
                  />
                  <span className={`upill${checked ? " upill--on" : ""}`}>
                    {getDepartmentLabel(d.code, d.name)}
                  </span>
                </label>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}
