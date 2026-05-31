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
import { DataTable, type ColumnDef } from "@/shared/components/DataTable";
import { FilterPanel, FilterField } from "@/shared/components/FilterPanel";
import { Pagination } from "@/shared/components/Pagination";
import { StatusBadge } from "@/shared/components/StatusBadge";

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

  const columns: ColumnDef<PlanListItem>[] = [
    {
      key: "month",
      header: "Kỳ",
      className: "plans-table__title",
      cell: (p) => formatMonth(p),
    },
    {
      key: "department",
      header: "Phòng",
      cell: (p) => getDepartmentLabel(p.departmentCode, p.departmentName) || "Kế hoạch tổng hợp",
    },
    {
      key: "status",
      header: "Trạng thái",
      cell: (p) => <StatusBadge status={p.status} />,
    },
    {
      key: "createdByName",
      header: "Người tạo",
      cell: (p) => p.createdByName ?? "Không rõ",
    },
    {
      key: "submittedAt",
      header: "Ngày nộp",
      cell: (p) => p.submittedAt ? new Date(p.submittedAt).toLocaleString("vi-VN") : "-",
    },
    {
      key: "updatedAt",
      header: "Cập nhật",
      cell: (p) => new Date(p.updatedAt).toLocaleString("vi-VN"),
    },
    {
      key: "actions",
      header: "",
      cell: (plan) => (
        <div className="plans-table__actions">
          <button className="plans-link" onClick={() => navigate(`/plans/main/${plan.id}`)} type="button">
            Mở chi tiết
          </button>
          {canOpenCreate && plan.status === "draft" ? (
            <>
              <button className="plans-link" onClick={() => { setEditingPlan(plan); setIsDialogOpen(true); }} type="button">
                Sửa kỳ
              </button>
              <button className="plans-link plans-link--danger" disabled={deletePlanMutation.isPending} onClick={() => { void handleDelete(plan); }} type="button">
                Xóa
              </button>
            </>
          ) : null}
        </div>
      ),
    },
  ];

  const totalPages = plansQuery.data
    ? Math.ceil(plansQuery.data.meta.total / filters.pageSize)
    : 1;

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
        <FilterPanel
          onSearch={() => { /* Fetch is driven by filters state */ }}
          onReset={() => setFilters({ page: 1, pageSize: 10, year: null, month: null, status: "" })}
        >
          <FilterField label="Kỳ kế hoạch">
            <input
              className="filter-input"
              type="month"
              value={toMonthValue(filters.year, filters.month)}
              onChange={(event) => {
                const next = parseMonthFilter(event.target.value);
                setFilters((current) => ({
                  ...current,
                  page: 1,
                  year: next.year,
                  month: next.month,
                }));
              }}
            />
          </FilterField>
          <FilterField label="Trạng thái">
            <select
              className="filter-select"
              value={filters.status}
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  page: 1,
                  status: event.target.value as ApprovalStatus | "",
                }))
              }
            >
              {statusOptions.map((option) => (
                <option key={option.value || "all"} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </FilterField>
        </FilterPanel>

        {plansQuery.isError ? (
          <div className="plans-state">
            <h2>Không tải được danh sách</h2>
            <p>{toApiError(plansQuery.error).message}</p>
          </div>
        ) : (
          <>
            <DataTable
              data={plansQuery.data?.items ?? []}
              columns={columns}
              keyExtractor={(p) => p.id}
              isLoading={plansQuery.isLoading}
            />

            <Pagination
              currentPage={filters.page}
              totalPages={totalPages}
              onPageChange={(page) => setFilters((p) => ({ ...p, page }))}
            />
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
