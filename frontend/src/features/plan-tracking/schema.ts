import { z } from "zod";

export const monthPickerSchema = z.object({
  month: z.string().regex(/^\d{4}-\d{2}$/, "Thang khong hop le."),
});
