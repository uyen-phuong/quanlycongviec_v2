import type { PlanStatus, ResolvedPlan } from "@/features/plan-tracking/types";
import { getDepartmentLabel } from "@/shared/departmentLabels";

const statusLabels: Record<PlanStatus, string> = {
  draft: "Chưa phê duyệt",
  pending: "Chờ kiểm soát",
  approved_1: "Đã kiểm soát",
  approved_2: "Đã phê duyệt đơn vị",
  approved_3: "Đã phê duyệt",
  returned: "Đã chuyển trả",
};

export function StatusBar({
  resolvedPlan,
  roleLabel,
}: {
  resolvedPlan: ResolvedPlan | null;
  roleLabel: string;
}) {
  return (
    <section className="rounded-[28px] border border-stone-200 bg-white px-6 py-5 shadow-sm">
      <div className="flex flex-wrap items-center gap-4">
        <span className="rounded-full bg-stone-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-stone-600">
          Vai trò {roleLabel}
        </span>
        <span
          className={`rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] ${
            resolvedPlan?.status
              ? "bg-amber-100 text-amber-900"
              : "bg-stone-100 text-stone-600"
          }`}
        >
          {resolvedPlan?.status ? statusLabels[resolvedPlan.status] : "Chưa có kế hoạch"}
        </span>
        {resolvedPlan?.departmentName ? (
          <span className="text-sm text-stone-600">
            Đơn vị: <strong className="text-stone-800">{getDepartmentLabel(resolvedPlan.departmentCode, resolvedPlan.departmentName)}</strong>
          </span>
        ) : null}
        {resolvedPlan ? (
          <span className="text-sm text-stone-600">
            Kỳ kế hoạch:{" "}
            <strong className="text-stone-800">
              {String(resolvedPlan.month).padStart(2, "0")}/{resolvedPlan.year}
            </strong>
          </span>
        ) : null}
      </div>
    </section>
  );
}
