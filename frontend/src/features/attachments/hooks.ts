import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { attachmentApi } from "@/features/attachments/api";
import type { AttachmentDto } from "@/features/attachments/types";

export const attachmentKeys = {
  forPlan: (planId: string | null) => ["attachments", "plan", planId] as const,
  forTask: (taskId: string | null) => ["attachments", "task", taskId] as const,
};

export function usePlanAttachments(planId: string | null) {
  return useQuery({
    queryKey: attachmentKeys.forPlan(planId),
    queryFn: () => attachmentApi.listForPlan(planId!),
    enabled: Boolean(planId),
    staleTime: 30 * 1000,
  });
}

export function useTaskAttachments(taskId: string | null) {
  return useQuery({
    queryKey: attachmentKeys.forTask(taskId),
    queryFn: () => attachmentApi.listForTask(taskId!),
    enabled: Boolean(taskId),
    staleTime: 30 * 1000,
  });
}

export function useUploadPlanAttachment(planId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) => attachmentApi.uploadToPlan(planId!, file),
    onSuccess: (newItem) => {
      queryClient.setQueryData<AttachmentDto[]>(
        attachmentKeys.forPlan(planId),
        (prev) => (prev ? [...prev, newItem] : [newItem]),
      );
    },
  });
}

export function useUploadTaskAttachment(taskId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) => attachmentApi.uploadToTask(taskId!, file),
    onSuccess: (newItem) => {
      queryClient.setQueryData<AttachmentDto[]>(
        attachmentKeys.forTask(taskId),
        (prev) => (prev ? [...prev, newItem] : [newItem]),
      );
    },
  });
}

export function useDeleteAttachment(ownerType: "plan" | "task", ownerId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (attachmentId: string) => attachmentApi.delete(attachmentId),
    onSuccess: (_data, attachmentId) => {
      const key =
        ownerType === "plan"
          ? attachmentKeys.forPlan(ownerId)
          : attachmentKeys.forTask(ownerId);
      queryClient.setQueryData<AttachmentDto[]>(key, (prev) =>
        prev?.filter((a) => a.id !== attachmentId),
      );
    },
  });
}
