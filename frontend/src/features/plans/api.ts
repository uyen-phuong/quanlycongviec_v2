import { apiClient } from "@/shared/api/client";
import type {
  ApiEnvelope,
  DepartmentLookupDto,
  PagedMeta,
} from "@/shared/api/dtos";
import type {
  ApprovalHistoryItem,
  ImportMainPlanExcelResult,
  PlanDetail,
  PlanFormValues,
  PlanListFilters,
  PlanListItem,
  ReportingPeriod,
  ReturnLineCommentDraft,
} from "@/features/plans/types";

export const plansApi = {
  async list(filters: PlanListFilters) {
    const response = await apiClient.get<ApiEnvelope<PlanListItem[]>>(
      "/plans/main",
      {
        params: {
          page: filters.page,
          pageSize: filters.pageSize,
          year: filters.year ?? undefined,
          month: filters.month ?? undefined,
          status: filters.status || undefined,
        },
      },
    );

    return {
      items: response.data.data,
      meta: response.data.meta as PagedMeta,
    };
  },

  async getDetail(id: string) {
    const response = await apiClient.get<ApiEnvelope<PlanDetail>>(
      `/plans/main/${id}`,
    );
    return response.data.data;
  },

  async create(values: PlanFormValues) {
    const response = await apiClient.post<ApiEnvelope<PlanDetail>>(
      "/plans/main",
      { year: values.year, month: values.month },
    );
    return response.data.data;
  },

  async deleteMainPlan(id: string) {
    const response = await apiClient.delete<ApiEnvelope<boolean>>(
      `/plans/main/${id}`,
    );
    return response.data.data;
  },

  async update(id: string, values: PlanFormValues) {
    const response = await apiClient.put<ApiEnvelope<PlanDetail>>(
      `/plans/main/${id}`,
      {
        year: values.year,
        month: values.month,
      },
    );
    return response.data.data;
  },

  async listApprovalHistory(planId: string) {
    const response = await apiClient.get<ApiEnvelope<ApprovalHistoryItem[]>>(
      `/plans/${planId}/approval-history`,
    );
    const payload = response.data.data as unknown;

    if (Array.isArray(payload)) {
      return payload;
    }

    if (
      payload &&
      typeof payload === "object" &&
      "items" in payload &&
      Array.isArray((payload as { items?: unknown }).items)
    ) {
      return (payload as { items: ApprovalHistoryItem[] }).items;
    }

    return [];
  },

  async submit(planId: string, comment?: string | null) {
    const response = await apiClient.post<ApiEnvelope<PlanDetail>>(
      `/plans/${planId}/submit`,
      { comment: comment ?? null },
    );
    return response.data.data;
  },

  async approve(planId: string, comment?: string | null) {
    const response = await apiClient.post<ApiEnvelope<PlanDetail>>(
      `/plans/${planId}/approve`,
      { comment: comment ?? null },
    );
    return response.data.data;
  },

  async returnPlan(
    planId: string,
    values: { comment: string | null; lineComments: ReturnLineCommentDraft[] },
  ) {
    const response = await apiClient.post<ApiEnvelope<PlanDetail>>(
      `/plans/${planId}/return`,
      values,
    );
    return response.data.data;
  },

  async listDepartments() {
    const response = await apiClient.get<ApiEnvelope<DepartmentLookupDto[]>>(
      "/departments",
    );
    return response.data.data;
  },

  async importMainPlanExcel(planId: string, file: File) {
    const formData = new FormData();
    formData.append("file", file);

    const response = await apiClient.post<ApiEnvelope<ImportMainPlanExcelResult>>(
      `/plans/main/${planId}/import-excel`,
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      },
    );

    return response.data.data;
  },

  async exportMainPlanExcel(planId: string) {
    const response = await apiClient.get(
      `/plans/main/${planId}/export-excel`,
      {
        responseType: "blob",
      },
    );

    const disposition = response.headers["content-disposition"] as string | undefined;
    const filenameMatch = disposition?.match(/filename="?([^"]+)"?/i);

    return {
      blob: response.data as Blob,
      fileName: filenameMatch?.[1] ?? `khct-main-plan-${planId}.xlsx`,
    };
  },

  async listReportingPeriods(planId: string): Promise<ReportingPeriod[]> {
    const res = await apiClient.get<ApiEnvelope<ReportingPeriod[]>>(`/plans/main/${planId}/reporting-periods`);
    return res.data.data;
  },

  async updateReportingPeriodProgress(planId: string, progressText: string | null, completionPercent: number): Promise<ReportingPeriod> {
    const res = await apiClient.put<ApiEnvelope<ReportingPeriod>>(`/plans/main/${planId}/reporting-periods/current`, {
      progressText,
      completionPercent,
    });
    return res.data.data;
  },

  async approveReportingPeriod(planId: string): Promise<ReportingPeriod> {
    const res = await apiClient.post<ApiEnvelope<ReportingPeriod>>(`/plans/main/${planId}/reporting-periods/approve`);
    return res.data.data;
  },
};
