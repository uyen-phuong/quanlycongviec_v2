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

export const departmentTaskKeys = {
  tasks: (departmentCode?: string | null) => ["department-tasks", "tasks", departmentCode] as const,
};

export const personalTaskKeys = {
  tasks: () => ["personal-tasks", "tasks"] as const,
};

export const projectKeys = {
  projects: ["projects"] as const,
  tasks: (projectId: string | null) => ["projects", "tasks", projectId] as const,
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

export function useDepartmentUsers(departmentId: string | null) {
  return useQuery({
    queryKey: ["plan-tracking", "users", departmentId] as const,
    queryFn: () => planTrackingApi.listDepartmentUsers(departmentId!),
    enabled: Boolean(departmentId),
    staleTime: 5 * 60 * 1000,
  });
}

export function useSubmitTaskWorkflow(planId: string | null, departmentCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (comment?: string | null) =>
      planTrackingApi.submitTaskWorkflow(planId!, { departmentCode, comment }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useApproveTaskWorkflow(planId: string | null, departmentCode: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (comment?: string | null) =>
      planTrackingApi.approveTaskWorkflow(planId!, { departmentCode, comment }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useReturnTaskWorkflow(planId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (values: {
      departmentCode?: string | null;
      comment?: string | null;
      lineComments: Array<{ taskId: string; content: string }>;
    }) => planTrackingApi.returnTaskWorkflow(planId!, values),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useSubmitTaskSingle(planId: string | null, workType: number | null, departmentCode: string | null) {
  const queryClient = useQueryClient();
  const key = [...planTrackingKeys.tasks(planId, workType), departmentCode] as const;

  return useMutation({
    mutationFn: ({ taskId, comment }: { taskId: string; comment?: string | null }) =>
      planTrackingApi.submitTask(taskId, comment),
    onSuccess: (updatedTask) => {
      queryClient.setQueryData<TaskListItem[]>(
        key,
        (currentTasks) =>
          currentTasks?.map((task) =>
            task.id === updatedTask.id ? updatedTask : task,
          ) ?? [],
      );
      void queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useAssignTaskSingle(planId: string | null, workType: number | null, departmentCode: string | null) {
  const queryClient = useQueryClient();
  const key = [...planTrackingKeys.tasks(planId, workType), departmentCode] as const;

  return useMutation({
    mutationFn: ({ taskId, assigneeUserId, controllerUserId }: { taskId: string; assigneeUserId: string; controllerUserId?: string | null }) =>
      planTrackingApi.assignTask(taskId, assigneeUserId, controllerUserId),
    onSuccess: (updatedTask) => {
      queryClient.setQueryData<TaskListItem[]>(
        key,
        (currentTasks) =>
          currentTasks?.map((task) =>
            task.id === updatedTask.id ? updatedTask : task,
          ) ?? [],
      );
      void queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useApproveTaskSingle(planId: string | null, workType: number | null, departmentCode: string | null) {
  const queryClient = useQueryClient();
  const key = [...planTrackingKeys.tasks(planId, workType), departmentCode] as const;

  return useMutation({
    mutationFn: ({ taskId, comment }: { taskId: string; comment?: string | null }) =>
      planTrackingApi.approveTask(taskId, comment),
    onSuccess: (updatedTask) => {
      queryClient.setQueryData<TaskListItem[]>(
        key,
        (currentTasks) =>
          currentTasks?.map((task) =>
            task.id === updatedTask.id ? updatedTask : task,
          ) ?? [],
      );
      void queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useReturnTaskSingle(planId: string | null, workType: number | null, departmentCode: string | null) {
  const queryClient = useQueryClient();
  const key = [...planTrackingKeys.tasks(planId, workType), departmentCode] as const;

  return useMutation({
    mutationFn: ({ taskId, comment }: { taskId: string; comment: string }) =>
      planTrackingApi.returnTask(taskId, comment),
    onSuccess: (updatedTask) => {
      queryClient.setQueryData<TaskListItem[]>(
        key,
        (currentTasks) =>
          currentTasks?.map((task) =>
            task.id === updatedTask.id ? updatedTask : task,
          ) ?? [],
      );
      void queryClient.invalidateQueries({ queryKey: ["plan-tracking"] });
    },
  });
}

export function useDepartmentTasks(departmentCode?: string | null) {
  return useQuery({
    queryKey: departmentTaskKeys.tasks(departmentCode),
    queryFn: () => planTrackingApi.listDepartmentTasks(departmentCode),
    staleTime: 30 * 1000,
  });
}

export function useCreateDepartmentTask(departmentCode?: string | null) {
  const queryClient = useQueryClient();
  const key = departmentTaskKeys.tasks(departmentCode);

  return useMutation({
    mutationFn: (payload: Omit<CreateTaskPayload, "planId">) =>
      planTrackingApi.createDepartmentTask(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useSaveDepartmentTask(departmentCode?: string | null) {
  const queryClient = useQueryClient();
  const key = departmentTaskKeys.tasks(departmentCode);

  return useMutation({
    mutationFn: ({ taskId, payload }: { taskId: string; payload: Parameters<typeof planTrackingApi.saveTask>[1] }) =>
      planTrackingApi.saveTask(taskId, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useDeleteDepartmentTask(departmentCode?: string | null) {
  const queryClient = useQueryClient();
  const key = departmentTaskKeys.tasks(departmentCode);

  return useMutation({
    mutationFn: (taskId: string) => planTrackingApi.deleteTask(taskId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useSubmitDepartmentTaskSingle(departmentCode?: string | null) {
  const queryClient = useQueryClient();
  const key = departmentTaskKeys.tasks(departmentCode);

  return useMutation({
    mutationFn: ({ taskId, comment }: { taskId: string; comment?: string | null }) =>
      planTrackingApi.submitTask(taskId, comment),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useAssignDepartmentTaskSingle(departmentCode?: string | null) {
  const queryClient = useQueryClient();
  const key = departmentTaskKeys.tasks(departmentCode);

  return useMutation({
    mutationFn: ({ taskId, assigneeUserId, controllerUserId }: { taskId: string; assigneeUserId: string; controllerUserId?: string | null }) =>
      planTrackingApi.assignTask(taskId, assigneeUserId, controllerUserId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useApproveDepartmentTaskSingle(departmentCode?: string | null) {
  const queryClient = useQueryClient();
  const key = departmentTaskKeys.tasks(departmentCode);

  return useMutation({
    mutationFn: ({ taskId, comment }: { taskId: string; comment?: string | null }) =>
      planTrackingApi.approveTask(taskId, comment),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useReturnDepartmentTaskSingle(departmentCode?: string | null) {
  const queryClient = useQueryClient();
  const key = departmentTaskKeys.tasks(departmentCode);

  return useMutation({
    mutationFn: ({ taskId, comment }: { taskId: string; comment: string }) =>
      planTrackingApi.returnTask(taskId, comment),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function usePersonalTasks() {
  return useQuery({
    queryKey: personalTaskKeys.tasks(),
    queryFn: planTrackingApi.listPersonalTasks,
    staleTime: 30 * 1000,
  });
}

export function useCreatePersonalTask() {
  const queryClient = useQueryClient();
  const key = personalTaskKeys.tasks();

  return useMutation({
    mutationFn: (payload: { title: string; deadline?: string | null; noteText?: string | null; priority?: string | null; complexity?: string | null; displayOrder: number }) =>
      planTrackingApi.createPersonalTask(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useSavePersonalTask() {
  const queryClient = useQueryClient();
  const key = personalTaskKeys.tasks();

  return useMutation({
    mutationFn: ({ taskId, payload }: { taskId: string; payload: Parameters<typeof planTrackingApi.saveTask>[1] }) =>
      planTrackingApi.saveTask(taskId, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useDeletePersonalTask() {
  const queryClient = useQueryClient();
  const key = personalTaskKeys.tasks();

  return useMutation({
    mutationFn: (taskId: string) => planTrackingApi.deleteTask(taskId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useProjects() {
  return useQuery({
    queryKey: projectKeys.projects,
    queryFn: planTrackingApi.listProjects,
    staleTime: 30 * 1000,
  });
}

export function useCreateProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: { name: string; description: string; leaderId: string; subLeaderId: string | null; memberUserIds: string[] }) =>
      planTrackingApi.createProject(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: projectKeys.projects });
    },
  });
}

export function useSubmitProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => planTrackingApi.submitProject(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: projectKeys.projects });
    },
  });
}

export function useApproveProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => planTrackingApi.approveProject(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: projectKeys.projects });
    },
  });
}

export function useReturnProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, comment }: { id: string; comment: string }) => planTrackingApi.returnProject(id, comment),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: projectKeys.projects });
    },
  });
}

export function useProjectTasks(projectId: string | null) {
  return useQuery({
    queryKey: projectKeys.tasks(projectId),
    queryFn: () => planTrackingApi.listProjectTasks(projectId!),
    enabled: Boolean(projectId),
    staleTime: 30 * 1000,
  });
}

export function useCreateProjectTask(projectId: string | null) {
  const queryClient = useQueryClient();
  const key = projectKeys.tasks(projectId);

  return useMutation({
    mutationFn: (payload: Omit<CreateTaskPayload, "planId">) =>
      planTrackingApi.createProjectTask(projectId!, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useSaveProjectTask(projectId: string | null) {
  const queryClient = useQueryClient();
  const key = projectKeys.tasks(projectId);

  return useMutation({
    mutationFn: ({ taskId, payload }: { taskId: string; payload: Parameters<typeof planTrackingApi.saveTask>[1] }) =>
      planTrackingApi.saveTask(taskId, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}

export function useDeleteProjectTask(projectId: string | null) {
  const queryClient = useQueryClient();
  const key = projectKeys.tasks(projectId);

  return useMutation({
    mutationFn: (taskId: string) => planTrackingApi.deleteTask(taskId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
    },
  });
}
