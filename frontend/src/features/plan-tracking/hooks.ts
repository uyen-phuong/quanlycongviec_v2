import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { planTrackingApi } from "@/features/plan-tracking/api";
import { plansApi } from "@/features/plans/api";
import type { ReturnLineCommentDraft } from "@/features/plans/types";
import type { CreateTaskPayload, TaskListItem } from "@/features/plan-tracking/types";

export const planTrackingKeys = {
  departments: ["departments"] as const,
  resolvePlan: (
    scope: "main" | "sub",
    departmentCode: string | null,
    year: number | null,
    month: number | null,
  ) => ["plan-tracking", "resolve", scope, departmentCode, year, month] as const,
  tasks: (planId: string | null, workType: number | null) =>
    ["plan-tracking", "tasks", planId, workType] as const,
  lineComments: (planId: string | null) =>
    ["plan-tracking", "line-comments", planId] as const,
};

export function useDepartments() {
  return useQuery({
    queryKey: planTrackingKeys.departments,
    queryFn: planTrackingApi.listDepartments,
    staleTime: 5 * 60 * 1000,
  });
}

export function useResolvePlan(params: {
  scope: "main" | "sub";
  departmentCode: string | null;
  year: number | null;
  month: number | null;
}) {
  return useQuery({
    queryKey: planTrackingKeys.resolvePlan(
      params.scope,
      params.departmentCode,
      params.year,
      params.month,
    ),
    queryFn: () => planTrackingApi.resolvePlan(params),
    staleTime: 30 * 1000,
  });
}

export function useTasks(planId: string | null, workType: number | null, departmentCode: string | null) {
  return useQuery({
    queryKey: [...planTrackingKeys.tasks(planId, workType), departmentCode] as const,
    queryFn: () => planTrackingApi.listTasks(planId!, workType, departmentCode),
    enabled: Boolean(planId),
    staleTime: 30 * 1000,
  });
}

export function useLineComments(planId: string | null) {
  return useQuery({
    queryKey: planTrackingKeys.lineComments(planId),
    queryFn: () => planTrackingApi.listLineComments(planId!),
    enabled: Boolean(planId),
    staleTime: 30 * 1000,
  });
}

export function useApprovalHistory(planId: string | null) {
  return useQuery({
    queryKey: ["plan-tracking", "approval-history", planId] as const,
    queryFn: () => plansApi.listApprovalHistory(planId!),
    enabled: Boolean(planId),
    staleTime: 30 * 1000,
  });
}

export function useSaveTask(planId: string | null, workType: number | null, departmentCode: string | null) {
  const queryClient = useQueryClient();
  const key = [...planTrackingKeys.tasks(planId, workType), departmentCode] as const;

  return useMutation({
    mutationFn: ({
      taskId,
      payload,
    }: {
      taskId: string;
      payload: Parameters<typeof planTrackingApi.saveTask>[1];
    }) => planTrackingApi.saveTask(taskId, payload),
    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey: key });
      const previousTasks = queryClient.getQueryData<TaskListItem[]>(key);
      return { previousTasks };
    },
    onError: (_error, _variables, context) => {
      if (context?.previousTasks) {
        queryClient.setQueryData(key, context.previousTasks);
      }
    },
    onSuccess: (updatedTask) => {
      queryClient.setQueryData<TaskListItem[]>(
        key,
        (currentTasks) =>
          currentTasks?.map((task) =>
            task.id === updatedTask.id ? updatedTask : task,
          ) ?? [],
      );
      void queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
      void queryClient.invalidateQueries({
        queryKey: ["plans", "detail", planId],
        exact: false,
      });
      void queryClient.invalidateQueries({
        queryKey: ["plans", "history", planId],
        exact: false,
      });
    },
  });
}

export function useCreateTask(planId: string | null, departmentCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateTaskPayload) => planTrackingApi.createTask(payload),
    onSuccess: (newTask) => {
      queryClient.setQueryData<TaskListItem[]>(
        [...planTrackingKeys.tasks(planId, null), departmentCode] as const,
        (prev) => (prev ? [...prev, newTask] : [newTask]),
      );
      void queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
      void queryClient.invalidateQueries({
        queryKey: ["plans", "detail", planId],
        exact: false,
      });
      void queryClient.invalidateQueries({
        queryKey: ["plans", "history", planId],
        exact: false,
      });
    },
  });
}

export function useDeleteTask(planId: string | null, workType: number | null, departmentCode: string | null) {
  const queryClient = useQueryClient();
  const key = [...planTrackingKeys.tasks(planId, workType), departmentCode] as const;

  return useMutation({
    mutationFn: (taskId: string) => planTrackingApi.deleteTask(taskId),
    onSuccess: (_deleted, taskId) => {
      queryClient.setQueryData<TaskListItem[]>(
        key,
        (prev) => prev?.filter((task) => task.id !== taskId) ?? [],
      );
      void queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useCreateLineComment(planId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ taskId, content }: { taskId: string; content: string }) =>
      planTrackingApi.createLineComment(taskId, content),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: planTrackingKeys.lineComments(planId),
      });
      await queryClient.invalidateQueries({
        queryKey: ["plan-tracking", "tasks", planId],
        exact: false,
      });
    },
  });
}

export function useResolveLineComment(planId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (commentId: string) => planTrackingApi.resolveLineComment(commentId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: planTrackingKeys.lineComments(planId),
      });
      await queryClient.invalidateQueries({
        queryKey: ["plan-tracking", "tasks", planId],
        exact: false,
      });
    },
  });
}

export function useSubmitPlan(planId: string | null, _departmentCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (comment?: string | null) => planTrackingApi.submitPlan(planId!, comment),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useApprovePlan(planId: string | null, _departmentCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (comment?: string | null) => planTrackingApi.approvePlan(planId!, comment),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useReturnPlan(planId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: {
      comment: string | null;
      lineComments: ReturnLineCommentDraft[];
    }) => plansApi.returnPlan(planId!, values),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}
