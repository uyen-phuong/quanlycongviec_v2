import { cn } from "@/shared/utils";
import "./StatusBadge.css";

export type StatusVariant = "green" | "red" | "gold" | "blue" | "gray";

interface StatusBadgeProps {
  status: string;
  variant?: StatusVariant;
  className?: string;
}

export function StatusBadge({ status, variant, className }: StatusBadgeProps) {
  // Auto-resolve variant if not provided
  let resolvedVariant: StatusVariant = variant ?? "gray";
  if (!variant) {
    const s = status.toLowerCase();
    if (s.includes("hoàn thành") || s.includes("đạt")) {
      resolvedVariant = "green";
    } else if (s.includes("đang thực hiện") || s.includes("xử lý")) {
      resolvedVariant = "blue";
    } else if (s.includes("chờ") || s.includes("mới")) {
      resolvedVariant = "gold";
    } else if (s.includes("trả") || s.includes("hủy") || s.includes("không đạt")) {
      resolvedVariant = "red";
    }
  }

  return (
    <span
      className={cn(
        "status-badge",
        `status-badge--${resolvedVariant}`,
        className
      )}
    >
      {status}
    </span>
  );
}
