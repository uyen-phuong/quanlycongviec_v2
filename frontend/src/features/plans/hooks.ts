import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { plansApi } from "@/features/plans/api";
import type {
  PlanDetail,
  PlanFormValues,
  PlanListFilters,
} from "@/features/plans/types";

export const plansKeys = {
  all: ["plans"] as const,
  lists: () => ["plans", "lists"] as const,
  list: (filters: PlanListFilters) =>
    [
      "plans",
      "list",
      filters.page,
      filters.pageSize,
      filters.year,
      filters.month,
      filters.status,
      filters.departmentId ?? null,
    ] as const,
  detail: (id: string | null) =>
    ["plans", "detail", id] as const,
  history: (planId: string | null) => ["plans", "history", planId] as const,
};

export function usePlanList(filters: PlanListFilters) {
  return useQuery({
    queryKey: plansKeys.list(filters),
    queryFn: () => plansApi.list(filters),
    staleTime: 30 * 1000,
  });
}

export function usePlanDetail(id: string | null) {
  return useQuery({
    queryKey: plansKeys.detail(id),
    queryFn: () => plansApi.getDetail(id!),
    enabled: Boolean(id),
    staleTime: 30 * 1000,
  });
}

export function useApprovalHistory(planId: string | null) {
  return useQuery({
    queryKey: plansKeys.history(planId),
    queryFn: () => plansApi.listApprovalHistory(planId!),
    enabled: Boolean(planId),
    staleTime: 30 * 1000,
  });
}

function invalidatePlanViews(queryClient: ReturnType<typeof useQueryClient>, planId?: string | null) {
  void queryClient.invalidateQueries({ queryKey: plansKeys.all });
  if (planId) {
    void queryClient.invalidateQueries({
      queryKey: ["plan-tracking", "tasks", planId],
      exact: false,
    });
    void queryClient.invalidateQueries({
      queryKey: ["plan-tracking", "line-comments", planId],
      exact: false,
    });
  }
  void queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
}

export function useCreatePlan() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: PlanFormValues) => plansApi.create(values),
    onSuccess: () => {
      invalidatePlanViews(queryClient);
    },
  });
}

export function useDeleteMainPlan() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => plansApi.deleteMainPlan(id),
    onSuccess: () => {
      invalidatePlanViews(queryClient);
    },
  });
}

export function useUpdatePlan(id: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: PlanFormValues) => plansApi.update(id!, values),
    onSuccess: () => {
      invalidatePlanViews(queryClient, id);
    },
  });
}

function updateDetailCache(
  queryClient: ReturnType<typeof useQueryClient>,
  detail: PlanDetail,
) {
  queryClient.setQueryData(plansKeys.detail(detail.id), detail);
}

export function useSubmitPlan(planId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (comment?: string | null) => plansApi.submit(planId!, comment),
    onSuccess: (detail) => {
      updateDetailCache(queryClient, detail);
      invalidatePlanViews(queryClient, planId);
    },
  });
}

export function useApprovePlan(planId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (comment?: string | null) => plansApi.approve(planId!, comment),
    onSuccess: (detail) => {
      updateDetailCache(queryClient, detail);
      invalidatePlanViews(queryClient, planId);
    },
  });
}

export function useReturnPlan(planId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: {
      comment: string | null;
      lineComments: Array<{ taskId: string; content: string }>;
    }) => plansApi.returnPlan(planId!, values),
    onSuccess: (detail) => {
      updateDetailCache(queryClient, detail);
      invalidatePlanViews(queryClient, planId);
    },
  });
}

export function useImportMainPlanExcel(planId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) => plansApi.importMainPlanExcel(planId!, file),
    onSuccess: () => {
      invalidatePlanViews(queryClient, planId);
    },
  });
}

export function useExportMainPlanExcel(planId: string | null) {
  return useMutation({
    mutationFn: () => plansApi.exportMainPlanExcel(planId!),
  });
}
