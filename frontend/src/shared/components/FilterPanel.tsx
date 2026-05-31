import { type ReactNode, FormEvent } from "react";
import { cn } from "@/shared/utils";
import { Filter, Search, X } from "lucide-react";
import "./FilterPanel.css";

interface FilterPanelProps {
  title?: string;
  children: ReactNode;
  onSearch: () => void;
  onReset: () => void;
  className?: string;
  isSearching?: boolean;
}

export function FilterPanel({
  title = "Bộ lọc tìm kiếm",
  children,
  onSearch,
  onReset,
  className,
  isSearching = false,
}: FilterPanelProps) {
  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onSearch();
  };

  return (
    <form className={cn("filter-panel", className)} onSubmit={handleSubmit}>
      <div className="filter-panel-header">
        <h3 className="filter-panel-title">
          <Filter size={18} className="text-red-700" />
          {title}
        </h3>
      </div>

      <div className="filter-panel-grid">{children}</div>

      <div className="filter-actions">
        <button
          type="button"
          onClick={onReset}
          className="btn-agri-secondary"
          disabled={isSearching}
        >
          <X size={16} />
          Xóa bộ lọc
        </button>
        <button
          type="submit"
          className="btn-agri-primary"
          disabled={isSearching}
        >
          <Search size={16} />
          {isSearching ? "Đang tìm..." : "Tìm kiếm"}
        </button>
      </div>
    </form>
  );
}

// Sub-components for convenient usage
export function FilterField({
  label,
  children,
  className,
}: {
  label: string;
  children: ReactNode;
  className?: string;
}) {
  return (
    <div className={cn("filter-field", className)}>
      <label className="filter-label">{label}</label>
      {children}
    </div>
  );
}
