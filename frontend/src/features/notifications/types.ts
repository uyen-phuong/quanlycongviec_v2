export interface NotificationDto {
  id: string;
  title: string;
  body: string | null;
  eventType: string;
  planId: string | null;
  taskId: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface GetNotificationsResult {
  items: NotificationDto[];
  unreadCount: number;
}
