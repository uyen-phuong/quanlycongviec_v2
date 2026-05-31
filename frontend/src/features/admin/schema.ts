import { z } from "zod";

export const createUserSchema = z.object({
  username: z.string().min(1, "Bắt buộc").max(100),
  password: z.string().min(6, "Mật khẩu tối thiểu 6 ký tự").max(100),
  fullName: z.string().min(1, "Bắt buộc").max(200),
  email: z.string().email("Email không hợp lệ").max(200).or(z.literal("")).optional(),
  departmentId: z.string().uuid().or(z.literal("")).optional(),
  positionId: z.string().uuid().or(z.literal("")).optional(),
  roleId: z.string().uuid("Bắt buộc chọn vai trò").min(1, "Bắt buộc chọn vai trò"),
  isActive: z.boolean(),
});

export const updateUserSchema = z.object({
  fullName: z.string().min(1, "Bắt buộc").max(200),
  email: z.string().email("Email không hợp lệ").max(200).or(z.literal("")).optional(),
  departmentId: z.string().uuid().or(z.literal("")).optional(),
  positionId: z.string().uuid().or(z.literal("")).optional(),
  isActive: z.boolean(),
});

export const changeRoleSchema = z.object({
  roleId: z.string().uuid("Bắt buộc chọn vai trò"),
});

export const resetPasswordSchema = z.object({
  password: z.string().min(6, "Mật khẩu tối thiểu 6 ký tự").max(100),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
  message: "Mật khẩu xác nhận không khớp",
  path: ["confirmPassword"],
});

export type CreateUserValues = z.infer<typeof createUserSchema>;
export type UpdateUserValues = z.infer<typeof updateUserSchema>;
export type ChangeRoleValues = z.infer<typeof changeRoleSchema>;
export type ResetPasswordValues = z.infer<typeof resetPasswordSchema>;
