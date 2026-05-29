import { apiClient } from "@/shared/api/client";
import type { ApiEnvelope } from "@/shared/api/dtos";
import type { GetNotificationsResult } from "@/features/notifications/types";

export const notificationsApi = {
  getNotifications: async (): Promise<GetNotificationsResult> => {
    const res = await apiClient.get<ApiEnvelope<GetNotificationsResult>>("/notifications");
    return res.data.data;
  },

  markAllRead: async (): Promise<void> => {
    await apiClient.post("/notifications/read-all");
  },
};
