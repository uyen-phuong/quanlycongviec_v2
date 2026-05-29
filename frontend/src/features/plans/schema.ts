import { z } from "zod";

export const planFormSchema = z.object({
  year: z.coerce.number().int().min(2000).max(2100),
  month: z.coerce.number().int().min(1).max(12),
  departmentId: z.string().uuid().nullable(),
});

export const returnLineCommentSchema = z.object({
  taskId: z.string().uuid(),
  content: z.string().trim().min(1, "Can nhap noi dung comment.").max(4000),
});

export const returnPlanSchema = z.object({
  comment: z.string().trim().max(2000).nullable(),
  lineComments: z.array(returnLineCommentSchema).min(1, "Can chon it nhat 1 task."),
});

