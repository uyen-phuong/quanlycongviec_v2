import { apiClient } from "@/shared/api/client";
import type { AttachmentDto } from "@/features/attachments/types";

const ALLOWED_EXTENSIONS = new Set([
  ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
  ".txt", ".csv", ".png", ".jpg", ".jpeg", ".gif",
  ".zip", ".rar", ".7z",
]);
const MAX_SIZE_BYTES = 50 * 1024 * 1024;

export function validateAttachmentFile(file: File): string | null {
  if (file.size > MAX_SIZE_BYTES) {
    return `File vượt quá giới hạn 50MB (hiện tại: ${(file.size / 1024 / 1024).toFixed(1)}MB).`;
  }

  const dot = file.name.lastIndexOf(".");
  const ext = dot >= 0 ? file.name.slice(dot).toLowerCase() : "";
  if (!ALLOWED_EXTENSIONS.has(ext)) {
    return `Định dạng "${ext || "không có extension"}" không được phép. Cho phép: ${[...ALLOWED_EXTENSIONS].join(", ")}.`;
  }

  return null;
}

export const attachmentApi = {
  listForPlan: async (planId: string): Promise<AttachmentDto[]> => {
    const res = await apiClient.get<{ data: AttachmentDto[] }>(`/api/plans/${planId}/attachments`);
    return res.data.data;
  },

  listForTask: async (taskId: string): Promise<AttachmentDto[]> => {
    const res = await apiClient.get<{ data: AttachmentDto[] }>(`/api/tasks/${taskId}/attachments`);
    return res.data.data;
  },

  uploadToPlan: async (planId: string, file: File): Promise<AttachmentDto> => {
    const form = new FormData();
    form.append("file", file);
    const res = await apiClient.post<{ data: AttachmentDto }>(`/api/plans/${planId}/attachments`, form);
    return res.data.data;
  },

  uploadToTask: async (taskId: string, file: File): Promise<AttachmentDto> => {
    const form = new FormData();
    form.append("file", file);
    const res = await apiClient.post<{ data: AttachmentDto }>(`/api/tasks/${taskId}/attachments`, form);
    return res.data.data;
  },

  download: async (attachmentId: string): Promise<Blob> => {
    const res = await apiClient.get<Blob>(`/api/attachments/${attachmentId}/download`, {
      responseType: "blob",
    });
    return res.data;
  },

  delete: async (attachmentId: string): Promise<void> => {
    await apiClient.delete(`/api/attachments/${attachmentId}`);
  },
};
