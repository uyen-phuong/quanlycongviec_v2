import { apiClient } from "@/shared/api/client";
import type {
  ApiEnvelope,
  DepartmentLookupDto,
} from "@/shared/api/dtos";
import type {
  AdminUserListItemDto,
  CreateTaskPayload,
  LineComment,
  ResolvedPlan,
  SaveTaskPayload,
  TaskDetailResponse,
  TaskListItem,
} from "@/features/plan-tracking/types";

function mapTaskDetailToListItem(task: TaskDetailResponse): TaskListItem {
  return {
    id: task.id,
    planId: task.planId,
    projectId: task.projectId,
    category: task.category,
    parentTaskId: task.parentTaskId,
    outlineIndex: task.outlineIndex,
    displayOrder: task.displayOrder,
    isHeader: task.isHeader,
    title: task.title,
    workType: task.workType,
    workStatus: task.workStatus,
    deadline: task.deadline,
    assigneeUserId: task.assigneeUserId,
    assigneeName: task.assigneeName,
    controllerUserId: task.controllerUserId,
    ownerDepartmentId: task.ownerDepartmentId,
    ownerDepartmentCode: task.ownerDepartmentCode,
    ownerDepartmentName: task.ownerDepartmentName,
    bksMemberText: task.bksMemberText,
    ktnbLeaderText: task.ktnbLeaderText,
    noteText: task.noteText,
    isLocked: task.isLocked,
    hasOpenComment: task.hasOpenComment,
    workflowStatus: task.workflowStatus,
    submittedAt: task.submittedAt,
    approvedAt: task.approvedAt,
    progressText: task.progressText,
    reasonNotCompleted: task.reasonNotCompleted,
    priority: task.priority,
    complexity: task.complexity,
    supportingDepartmentIds: task.supportingDepartments.map((department) => department.id),
    createdAt: task.createdAt,
    updatedAt: task.updatedAt,
  };
}

export const planTrackingApi = {
  async resolvePlan(params: {
    scope: "main" | "sub";
    departmentCode?: string | null;
    year?: number | null;
    month?: number | null;
  }) {
    const response = await apiClient.get<ApiEnvelope<ResolvedPlan>>(
      "/plans/resolve",
      {
        params: {
          scope: params.scope,
          departmentCode: params.departmentCode ?? undefined,
          year: params.year ?? undefined,
          month: params.month ?? undefined,
        },
      },
    );

    return response.data.data;
  },

  async listDepartments() {
    const response = await apiClient.get<ApiEnvelope<DepartmentLookupDto[]>>(
      "/departments",
    );

    return response.data.data;
  },

  async listTasks(planId: string, workType?: number | null, departmentCode?: string | null) {
    const response = await apiClient.get<ApiEnvelope<TaskListItem[]>>(
      `/plans/${planId}/tasks`,
      {
        params: {
          workType: workType ?? undefined,
          departmentCode: departmentCode ?? undefined,
        },
      },
    );

    return response.data.data;
  },

  async createTask(payload: CreateTaskPayload) {
    const response = await apiClient.post<ApiEnvelope<TaskDetailResponse>>(
      "/tasks",
      payload,
    );

    return mapTaskDetailToListItem(response.data.data);
  },

  async saveTask(taskId: string, payload: SaveTaskPayload) {
    const response = await apiClient.put<ApiEnvelope<TaskDetailResponse>>(
      `/tasks/${taskId}`,
      payload,
    );

    return mapTaskDetailToListItem(response.data.data);
  },

  async deleteTask(taskId: string) {
    const response = await apiClient.delete<ApiEnvelope<boolean>>(
      `/tasks/${taskId}`,
    );

    return response.data.data;
  },

  async submitTask(taskId: string, comment?: string | null) {
    const response = await apiClient.post<ApiEnvelope<TaskDetailResponse>>(
      `/tasks/${taskId}/submit`,
      { comment: comment ?? null },
    );
    return mapTaskDetailToListItem(response.data.data);
  },

  async assignTask(taskId: string, assigneeUserId: string, controllerUserId?: string | null) {
    const response = await apiClient.post<ApiEnvelope<TaskDetailResponse>>(
      `/tasks/${taskId}/assign`,
      { assigneeUserId, controllerUserId: controllerUserId ?? null },
    );
    return mapTaskDetailToListItem(response.data.data);
  },

  async approveTask(taskId: string, comment?: string | null) {
    const response = await apiClient.post<ApiEnvelope<TaskDetailResponse>>(
      `/tasks/${taskId}/approve`,
      { comment: comment ?? null },
    );
    return mapTaskDetailToListItem(response.data.data);
  },

  async returnTask(taskId: string, comment: string) {
    const response = await apiClient.post<ApiEnvelope<TaskDetailResponse>>(
      `/tasks/${taskId}/return`,
      { comment },
    );
    return mapTaskDetailToListItem(response.data.data);
  },

  async listLineComments(planId: string) {
    const response = await apiClient.get<ApiEnvelope<LineComment[]>>(
      `/plans/${planId}/line-comments`,
    );

    return response.data.data;
  },

  async resolveLineComment(commentId: string) {
    const response = await apiClient.post<ApiEnvelope<LineComment>>(
      `/line-comments/${commentId}/resolve`,
      {},
    );

    return response.data.data;
  },

  async createLineComment(taskId: string, content: string) {
    const response = await apiClient.post<ApiEnvelope<LineComment>>(
      `/tasks/${taskId}/line-comments`,
      { content },
    );

    return response.data.data;
  },

  async submitPlan(planId: string, comment?: string | null) {
    const response = await apiClient.post<ApiEnvelope<{ status: string }>>(
      `/plans/${planId}/submit`,
      { comment: comment ?? null },
    );

    return response.data.data;
  },

  async approvePlan(planId: string, comment?: string | null) {
    const response = await apiClient.post<ApiEnvelope<{ status: string }>>(
      `/plans/${planId}/approve`,
      { comment: comment ?? null },
    );

    return response.data.data;
  },

  async returnTaskWorkflow(
    planId: string,
    payload: {
      departmentCode?: string | null;
      comment?: string | null;
      lineComments: Array<{ taskId: string; content: string }>;
    },
  ) {
    const response = await apiClient.post<ApiEnvelope<{ status: string }>>(
      `/plans/${planId}/task-workflow/return`,
      payload,
    );

    return response.data.data;
  },

  async submitTaskWorkflow(
    planId: string,
    payload: {
      departmentCode?: string | null;
      comment?: string | null;
    },
  ) {
    const response = await apiClient.post<ApiEnvelope<{ status: string }>>(
      `/plans/${planId}/task-workflow/submit`,
      payload,
    );

    return response.data.data;
  },

  async approveTaskWorkflow(
    planId: string,
    payload: {
      departmentCode?: string | null;
      comment?: string | null;
    },
  ) {
    const response = await apiClient.post<ApiEnvelope<{ status: string }>>(
      `/plans/${planId}/task-workflow/approve`,
      payload,
    );

    return response.data.data;
  },

  async listDepartmentUsers(departmentId: string) {
    const response = await apiClient.get<ApiEnvelope<AdminUserListItemDto[]>>(
      "/admin/users",
      {
        params: {
          departmentId,
          isActive: true,
          pageSize: 200,
        },
      },
    );

    return response.data.data;
  },

  async listDepartmentTasks(departmentCode?: string | null) {
    const response = await apiClient.get<ApiEnvelope<TaskDetailResponse[]>>(
      "/departments/tasks",
      {
        params: {
          departmentCode: departmentCode ?? undefined,
        },
      },
    );
    return response.data.data.map(mapTaskDetailToListItem);
  },

  async createDepartmentTask(payload: Omit<CreateTaskPayload, "planId">) {
    const response = await apiClient.post<ApiEnvelope<TaskDetailResponse>>(
      "/departments/tasks",
      payload,
    );
    return mapTaskDetailToListItem(response.data.data);
  },

  async listPersonalTasks() {
    const response = await apiClient.get<ApiEnvelope<TaskDetailResponse[]>>(
      "/personal/tasks",
    );
    return response.data.data.map(mapTaskDetailToListItem);
  },

  async createPersonalTask(payload: {
    title: string;
    deadline?: string | null;
    noteText?: string | null;
    priority?: string | null;
    complexity?: string | null;
    displayOrder: number;
  }) {
    const response = await apiClient.post<ApiEnvelope<TaskDetailResponse>>(
      "/personal/tasks",
      payload,
    );
    return mapTaskDetailToListItem(response.data.data);
  },

  async listProjects() {
    const response = await apiClient.get<ApiEnvelope<any[]>>("/projects");
    return response.data.data;
  },

  async createProject(payload: {
    name: string;
    description: string;
    leaderId: string;
    subLeaderId: string | null;
    memberUserIds: string[];
  }) {
    const response = await apiClient.post<ApiEnvelope<any>>("/projects", payload);
    return response.data.data;
  },

  async submitProject(id: string) {
    const response = await apiClient.post<ApiEnvelope<boolean>>(`/projects/${id}/submit`);
    return response.data.data;
  },

  async approveProject(id: string) {
    const response = await apiClient.post<ApiEnvelope<boolean>>(`/projects/${id}/approve`);
    return response.data.data;
  },

  async returnProject(id: string, comment: string) {
    const response = await apiClient.post<ApiEnvelope<boolean>>(`/projects/${id}/return`, { comment });
    return response.data.data;
  },

  async listProjectTasks(projectId: string) {
    const response = await apiClient.get<ApiEnvelope<TaskDetailResponse[]>>(`/projects/${projectId}/tasks`);
    return response.data.data.map(mapTaskDetailToListItem);
  },

  async createProjectTask(projectId: string, payload: Omit<CreateTaskPayload, "planId">) {
    const response = await apiClient.post<ApiEnvelope<TaskDetailResponse>>(`/projects/${projectId}/tasks`, payload);
    return mapTaskDetailToListItem(response.data.data);
  },

  async listAllUsers() {
    const response = await apiClient.get<ApiEnvelope<AdminUserListItemDto[]>>(
      "/admin/users",
      {
        params: {
          isActive: true,
          pageSize: 1000,
        },
      },
    );
    return response.data.data;
  },
};
