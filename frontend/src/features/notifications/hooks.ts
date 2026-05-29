import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notificationsApi } from "@/features/notifications/api";

export const notificationKeys = {
  list: ["notifications"] as const,
};

export function useNotifications() {
  return useQuery({
    queryKey: notificationKeys.list,
    queryFn: notificationsApi.getNotifications,
    refetchInterval: 30_000,
    staleTime: 15_000,
  });
}

export function useMarkAllNotificationsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: notificationsApi.markAllRead,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: notificationKeys.list });
    },
  });
}
