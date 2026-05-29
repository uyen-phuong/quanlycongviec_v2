import { z } from "zod";

export const loginSchema = z.object({
  username: z.string().trim().min(1, "Nhập tên đăng nhập."),
  password: z.string().min(1, "Nhập mật khẩu."),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
