import { apiClient } from "@/shared/api/client";
import type {
  ApiEnvelope,
  DepartmentLookupDto,
} from "@/shared/api/dtos";
import type {
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
    ownerDepartmentId: task.ownerDepartmentId,
    ownerDepartmentCode: task.ownerDepartmentCode,
    ownerDepartmentName: task.ownerDepartmentName,
    bksMemberText: task.bksMemberText,
    ktnbLeaderText: task.ktnbLeaderText,
    noteText: task.noteText,
    isLocked: task.isLocked,
    hasOpenComment: task.hasOpenComment,
    approvalStatus: task.approvalStatus,
    submittedAt: task.submittedAt,
    approvedAt: task.approvedAt,
    progressText: task.progressText,
    reasonNotCompleted: task.reasonNotCompleted,
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
};
