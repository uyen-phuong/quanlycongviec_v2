import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toApiError } from "@/shared/api/client";
import { getDepartmentLabel } from "@/shared/departmentLabels";
import { useAuth } from "@/shared/auth/useAuth";
import { PlanFormDialog } from "@/features/plans/PlanFormDialog";
import "@/features/plans/PlansPage.css";
import { useCreatePlan, useDeleteMainPlan, usePlanList, useUpdatePlan } from "@/features/plans/hooks";
import type {
  ApprovalStatus,
  PlanFormValues,
  PlanListFilters,
  PlanListItem,
} from "@/features/plans/types";

const statusOptions: Array<{ value: ApprovalStatus | ""; label: string }> = [
  { value: "", label: "Tất cả trạng thái" },
  { value: "draft", label: "Draft" },
  { value: "pending", label: "Pending" },
  { value: "approved_1", label: "Approved 1" },
  { value: "approved_2", label: "Approved 2" },
  { value: "approved_3", label: "Approved 3" },
  { value: "returned", label: "Returned" },
];

function parseMonthFilter(value: string) {
  if (!value) {
    return { year: null, month: null };
  }

  const [year, month] = value.split("-").map(Number);
  return { year, month };
}

function toMonthValue(year: number | null, month: number | null) {
  if (!year || !month) {
    return "";
  }

  return `${year}-${String(month).padStart(2, "0")}`;
}

function canCreate(roles: string[]) {
  return roles.some((role) => ["VAN_THU", "ADMIN"].includes(role));
}

function formatMonth(plan: PlanListItem) {
  return `${String(plan.month).padStart(2, "0")}/${plan.year}`;
}

function statusClass(status: ApprovalStatus) {
  return `plans-status plans-status--${status}`;
}

export function PlanListPage() {
  const auth = useAuth();
  const navigate = useNavigate();
  const [filters, setFilters] = useState<PlanListFilters>({
    page: 1,
    pageSize: 10,
    year: null,
    month: null,
    status: "",
  });
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingPlan, setEditingPlan] = useState<PlanListItem | null>(null);

  const plansQuery = usePlanList(filters);
  const createPlanMutation = useCreatePlan();
  const updatePlanMutation = useUpdatePlan(editingPlan?.id ?? null);
  const deletePlanMutation = useDeleteMainPlan();

  async function handleDelete(plan: PlanListItem) {
    const label = `${String(plan.month).padStart(2, "0")}/${plan.year}`;
    if (!window.confirm(`Xóa kế hoạch kỳ ${label}? Hành động này không thể hoàn tác.`)) {
      return;
    }

    try {
      await deletePlanMutation.mutateAsync(plan.id);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  const canOpenCreate = canCreate(auth.user?.roles ?? []);

  const initialFormValues = useMemo<PlanFormValues>(() => {
    if (editingPlan) {
      return {
        year: editingPlan.year,
        month: editingPlan.month,
        departmentId: null,
      };
    }

    const now = new Date();
    return {
      year: now.getFullYear(),
      month: now.getMonth() + 1,
      departmentId: null,
    };
  }, [editingPlan]);

  async function handleSubmit(values: PlanFormValues) {
    try {
      if (editingPlan) {
        await updatePlanMutation.mutateAsync(values);
      } else {
        const created = await createPlanMutation.mutateAsync(values);
        navigate(`/plans/main/${created.id}`);
      }

      setIsDialogOpen(false);
      setEditingPlan(null);
    } catch (error) {
      window.alert(toApiError(error).message);
    }
  }

  const isPending = createPlanMutation.isPending || updatePlanMutation.isPending;

  return (
    <div className="plans-page">
      <section className="plans-hero">
        <div>
          <h1>Danh sách kế hoạch</h1>
          <p>Danh mục kế hoạch tổng hợp theo tháng, dùng chung cho luồng task workflow mới.</p>
        </div>
        {canOpenCreate ? (
          <button
            className="plans-button plans-button--primary"
            onClick={() => {
              setEditingPlan(null);
              setIsDialogOpen(true);
            }}
            type="button"
          >
            Tạo kế hoạch
          </button>
        ) : null}
      </section>

      <section className="plans-panel">
        <div className="plans-filters">
          <label className="plans-field">
            <span>Kỳ kế hoạch</span>
            <input
              className="plans-input"
              onChange={(event) => {
                const next = parseMonthFilter(event.target.value);
                setFilters((current) => ({
                  ...current,
                  page: 1,
                  year: next.year,
                  month: next.month,
                }));
              }}
              type="month"
              value={toMonthValue(filters.year, filters.month)}
            />
          </label>

          <label className="plans-field">
            <span>Trạng thái</span>
            <select
              className="plans-input"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  page: 1,
                  status: event.target.value as ApprovalStatus | "",
                }))
              }
              value={filters.status}
            >
              {statusOptions.map((option) => (
                <option key={option.value || "all"} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>

        </div>

        {plansQuery.isLoading ? (
          <div className="plans-state">
            <h2>Đang tải danh sách</h2>
            <p>Frontend đang gọi paged endpoint và đồng bộ filter hiện tại.</p>
          </div>
        ) : plansQuery.isError ? (
          <div className="plans-state">
            <h2>Không tải được danh sách</h2>
            <p>{toApiError(plansQuery.error).message}</p>
          </div>
        ) : (
          <>
            <table className="plans-table">
              <thead>
                <tr>
                  <th>Kỳ</th>
                  <th>Phòng</th>
                  <th>Trạng thái</th>
                  <th>Người tạo</th>
                  <th>Ngày nộp</th>
                  <th>Cập nhật</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {plansQuery.data?.items.map((plan) => (
                  <tr key={plan.id}>
                    <td className="plans-table__title">{formatMonth(plan)}</td>
                    <td>{getDepartmentLabel(plan.departmentCode, plan.departmentName) || "Kế hoạch tổng hợp"}</td>
                    <td>
                      <span className={statusClass(plan.status)}>{plan.status}</span>
                    </td>
                    <td>{plan.createdByName ?? "Không rõ"}</td>
                    <td>
                      {plan.submittedAt
                        ? new Date(plan.submittedAt).toLocaleString("vi-VN")
                        : "-"}
                    </td>
                    <td>{new Date(plan.updatedAt).toLocaleString("vi-VN")}</td>
                    <td>
                      <div className="plans-table__actions">
                        <button
                          className="plans-link"
                          onClick={() =>
                            navigate(`/plans/main/${plan.id}`)
                          }
                          type="button"
                        >
                          Mở chi tiết
                        </button>
                        {canOpenCreate && plan.status === "draft" ? (
                          <>
                            <button
                              className="plans-link"
                              onClick={() => {
                                setEditingPlan(plan);
                                setIsDialogOpen(true);
                              }}
                              type="button"
                            >
                              Sửa kỳ
                            </button>
                            <button
                              className="plans-link plans-link--danger"
                              disabled={deletePlanMutation.isPending}
                              onClick={() => { void handleDelete(plan); }}
                              type="button"
                            >
                              Xóa
                            </button>
                          </>
                        ) : null}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {plansQuery.data?.items.length === 0 ? (
              <div className="plans-state">
                <h2>Chưa có bản ghi nào</h2>
                <p>Hãy đổi filter hoặc tạo kế hoạch mới cho kỳ đang cần theo dõi.</p>
              </div>
            ) : null}

            <div className="plans-pagination">
              <div>
                Tổng {plansQuery.data?.meta.total ?? 0} bản ghi, trang{" "}
                {plansQuery.data?.meta.page ?? 1}
              </div>
              <div className="plans-table__actions">
                <button
                  className="plans-button plans-button--ghost"
                  disabled={(plansQuery.data?.meta.page ?? 1) <= 1}
                  onClick={() =>
                    setFilters((current) => ({
                      ...current,
                      page: Math.max(1, current.page - 1),
                    }))
                  }
                  type="button"
                >
                  Trang trước
                </button>
                <button
                  className="plans-button plans-button--ghost"
                  disabled={
                    (plansQuery.data?.meta.page ?? 1) *
                      (plansQuery.data?.meta.pageSize ?? filters.pageSize) >=
                    (plansQuery.data?.meta.total ?? 0)
                  }
                  onClick={() =>
                    setFilters((current) => ({
                      ...current,
                      page: current.page + 1,
                    }))
                  }
                  type="button"
                >
                  Trang sau
                </button>
              </div>
            </div>
          </>
        )}
      </section>

      <PlanFormDialog
        initialValues={initialFormValues}
        isPending={isPending}
        onClose={() => {
          setIsDialogOpen(false);
          setEditingPlan(null);
        }}
        onSubmit={handleSubmit}
        open={isDialogOpen}
        title={editingPlan ? "Cập nhật kỳ kế hoạch" : "Tạo kế hoạch mới"}
      />
    </div>
  );
}
