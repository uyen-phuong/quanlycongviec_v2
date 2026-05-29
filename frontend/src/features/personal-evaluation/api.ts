import { apiClient } from "@/shared/api/client";
import type { ApiEnvelope } from "@/shared/api/dtos";
import type {
  PersonalEvaluationItem,
  PersonalEvaluationPeriod,
  PersonalEvaluationResponse,
  SaveItemPayload,
  SavePeriodPayload,
  ScorableUser,
} from "@/features/personal-evaluation/types";

export const personalEvaluationApi = {
  async get(year: number, month: number, userId?: string | null) {
    const response = await apiClient.get<ApiEnvelope<PersonalEvaluationResponse>>(
      "/personal-evaluations",
      { params: { year, month, userId: userId ?? undefined } },
    );
    return response.data.data;
  },

  async listScorableUsers() {
    const response = await apiClient.get<ApiEnvelope<ScorableUser[]>>(
      "/personal-evaluations/scorable-users",
    );
    return response.data.data;
  },

  async createItem(periodId: string) {
    const response = await apiClient.post<ApiEnvelope<PersonalEvaluationItem>>(
      "/personal-evaluations/items",
      { periodId },
    );
    return response.data.data;
  },

  async saveItem(id: string, payload: SaveItemPayload) {
    const response = await apiClient.put<ApiEnvelope<PersonalEvaluationItem>>(
      `/personal-evaluations/items/${id}`,
      payload,
    );
    return response.data.data;
  },

  async deleteItem(id: string) {
    const response = await apiClient.delete<ApiEnvelope<boolean>>(
      `/personal-evaluations/items/${id}`,
    );
    return response.data.data;
  },

  async savePeriod(id: string, payload: SavePeriodPayload) {
    const response = await apiClient.put<ApiEnvelope<PersonalEvaluationPeriod>>(
      `/personal-evaluations/period/${id}`,
      payload,
    );
    return response.data.data;
  },

  async downloadExport(periodId: string, variant: "01" | "01a"): Promise<{ blob: Blob; fileName: string }> {
    const response = await apiClient.get<Blob>(
      `/personal-evaluations/${periodId}/export/phu-luc-${variant}`,
      { responseType: "blob" },
    );
    const disp = response.headers["content-disposition"] || "";
    const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(disp);
    const fileName = match ? decodeURIComponent(match[1]) : `PhuLuc${variant.toUpperCase()}.xlsx`;
    return { blob: response.data, fileName };
  },
};
