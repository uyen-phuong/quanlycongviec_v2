import type { ReactNode } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useLogin } from "@/features/auth/hooks";
import { loginSchema, type LoginFormValues } from "@/features/auth/schema";
import { toApiError } from "@/shared/api/client";
import "@/features/auth/LoginPage.css";

export function LoginPage() {
  const loginMutation = useLogin();
  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      username: "",
      password: "",
    },
  });

  const submit = form.handleSubmit(async (values) => {
    try {
      await loginMutation.mutateAsync(values);
    } catch (error) {
      const apiError = toApiError(error);
      form.setError("root", {
        message: apiError.message,
      });
    }
  });

  return (
    <main className="login-page flex min-h-screen items-center justify-center px-6 py-16">
      <div className="grid w-full max-w-6xl gap-10 lg:grid-cols-[1.15fr_0.85fr]">
        <section className="flex flex-col justify-center rounded-[36px] border border-white/60 bg-white/70 p-10 shadow-[0_30px_80px_rgba(94,58,29,0.12)] backdrop-blur">
          <p className="text-xs font-semibold uppercase tracking-[0.34em] text-maroon-soft">
            Hệ thống KHCT
          </p>
          <h1 className="mt-4 max-w-3xl text-5xl font-bold leading-tight text-ink">
            Theo dõi tiến độ thực hiện kế hoạch công tác - Kiểm toán nội bộ Agribank
          </h1>
          <p className="mt-6 max-w-2xl text-base leading-8 text-stone-600">
            Hệ thống quản lý và theo dõi kế hoạch công tác nội bộ, hỗ trợ quy trình
            phê duyệt nhiều cấp và đồng bộ dữ liệu giữa kế hoạch tổng hợp và đơn vị.
          </p>
          <div className="mt-10 grid gap-4 sm:grid-cols-3">
            <FeatureTile
              title="Phân quyền theo vai trò"
              description="Mỗi vai trò có quyền truy cập và thao tác riêng biệt."
            />
            <FeatureTile
              title="Quy trình phê duyệt"
              description="Kế hoạch trải qua nhiều bước kiểm soát trước khi được duyệt."
            />
            <FeatureTile
              title="Đồng bộ tiến độ"
              description="Tiến độ từ đơn vị tự động cập nhật lên kế hoạch tổng hợp."
            />
          </div>
        </section>

        <section className="login-card rounded-[32px] border border-stone-200 bg-white p-8 shadow-[0_24px_64px_rgba(52,24,24,0.12)]">
          <p className="text-xs font-semibold uppercase tracking-[0.3em] text-stone-500">
            Đăng nhập
          </p>
          <h2 className="mt-3 text-3xl font-bold text-ink">
            Vào hệ thống quản trị KHCT
          </h2>
          <p className="mt-3 text-sm leading-7 text-stone-600">
            Dùng tài khoản được cấp. Phiên làm việc được duy trì bằng
            access token trong memory và refresh cookie HttpOnly.
          </p>

          <form className="mt-8 space-y-5" onSubmit={submit}>
            <Field
              error={form.formState.errors.username?.message}
              label="Tên đăng nhập"
            >
              <input
                autoComplete="username"
                className="w-full rounded-2xl border border-stone-300 bg-stone-50 px-4 py-3 text-sm text-stone-800 outline-none transition focus:border-maroon focus:bg-white"
                placeholder="vd: admin"
                {...form.register("username")}
              />
            </Field>

            <Field
              error={form.formState.errors.password?.message}
              label="Mật khẩu"
            >
              <input
                autoComplete="current-password"
                className="w-full rounded-2xl border border-stone-300 bg-stone-50 px-4 py-3 text-sm text-stone-800 outline-none transition focus:border-maroon focus:bg-white"
                placeholder="Nhập mật khẩu"
                type="password"
                {...form.register("password")}
              />
            </Field>

            {form.formState.errors.root?.message ? (
              <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                {form.formState.errors.root.message}
              </div>
            ) : null}

            <button
              className="w-full rounded-2xl bg-maroon px-4 py-3 text-sm font-semibold text-white transition hover:bg-maroon-deep disabled:cursor-not-allowed disabled:opacity-70"
              disabled={loginMutation.isPending}
              type="submit"
            >
              {loginMutation.isPending ? "Đang đăng nhập..." : "Đăng nhập"}
            </button>
          </form>
        </section>
      </div>
    </main>
  );
}

function Field({
  label,
  error,
  children,
}: {
  label: string;
  error?: string;
  children: ReactNode;
}) {
  return (
    <label className="block">
      <span className="mb-2 block text-sm font-medium text-stone-700">{label}</span>
      {children}
      {error ? <span className="mt-2 block text-sm text-red-700">{error}</span> : null}
    </label>
  );
}

function FeatureTile({
  title,
  description,
}: {
  title: string;
  description: string;
}) {
  return (
    <div className="rounded-2xl border border-stone-200 bg-stone-50 p-4">
      <p className="text-sm font-semibold text-ink">{title}</p>
      <p className="mt-2 text-sm leading-7 text-stone-600">{description}</p>
    </div>
  );
}
