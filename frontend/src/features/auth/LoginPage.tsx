import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useLogin } from "@/features/auth/hooks";
import { loginSchema, type LoginFormValues } from "@/features/auth/schema";
import { toApiError } from "@/shared/api/client";
import "@/features/auth/LoginPage.css";

export function LoginPage() {
  const loginMutation = useLogin();
  const [showPassword, setShowPassword] = useState(false);
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
    <main className="login-page">
      {/* 1. Glowing Bottom Wave SVG */}
      <svg className="absolute bottom-0 left-0 w-full pointer-events-none" viewBox="0 0 1440 220" fill="none" xmlns="http://www.w3.org/2000/svg" style={{ opacity: 0.85, zIndex: 1 }}>
        <path d="M0 120C150 140 320 180 480 150C640 120 780 70 960 90C1140 110 1280 180 1440 160V220H0V120Z" fill="url(#goldGrad)" opacity="0.15" />
        <path d="M0 150C200 130 400 110 600 140C800 170 1000 180 1200 150C1300 135 1380 145 1440 160V220H0V150Z" fill="url(#goldGrad2)" opacity="0.1" />
        <path d="M0 160C300 180 600 120 900 150C1200 180 1350 160 1440 150" stroke="url(#lineGrad)" strokeWidth="1.5" opacity="0.3" />
        <path d="M0 130C350 100 700 190 1050 140C1250 110 1380 130 1440 145" stroke="url(#lineGrad)" strokeWidth="1" opacity="0.25" />
        <defs>
          <linearGradient id="goldGrad" x1="0" y1="0" x2="1440" y2="220" gradientUnits="userSpaceOnUse">
            <stop offset="0%" stopColor="#ae1c3f" />
            <stop offset="50%" stopColor="#ffdd00" stopOpacity="0.2" />
            <stop offset="100%" stopColor="#ae1c3f" />
          </linearGradient>
          <linearGradient id="goldGrad2" x1="0" y1="0" x2="1440" y2="220" gradientUnits="userSpaceOnUse">
            <stop offset="0%" stopColor="#ffdd00" stopOpacity="0" />
            <stop offset="70%" stopColor="#ffdd00" stopOpacity="0.15" />
            <stop offset="100%" stopColor="#ffdd00" stopOpacity="0" />
          </linearGradient>
          <linearGradient id="lineGrad" x1="0" y1="0" x2="1440" y2="0" gradientUnits="userSpaceOnUse">
            <stop offset="0%" stopColor="#ae1c3f" stopOpacity="0" />
            <stop offset="50%" stopColor="#ffdd00" stopOpacity="0.8" />
            <stop offset="100%" stopColor="#ffdd00" stopOpacity="0" />
          </linearGradient>
        </defs>
      </svg>

      {/* 2. Abstract building outline in the background */}
      <div className="absolute right-[40%] top-[10%] opacity-[0.06] pointer-events-none select-none hidden lg:block" style={{ zIndex: 1 }}>
        <svg width="450" height="700" viewBox="0 0 450 700" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M200 100 L350 170 L350 650 L200 580 Z" stroke="#ffffff" strokeWidth="1.5" />
          <path d="M50 170 L200 100 L200 580 L50 650 Z" stroke="#ffffff" strokeWidth="1.5" />
          <path d="M200 100 L200 580" stroke="#ffffff" strokeWidth="1.5" />
          <path d="M50 250 L200 180 L350 250" stroke="#ffffff" strokeWidth="0.8" strokeDasharray="3 3" />
          <path d="M50 330 L200 260 L350 330" stroke="#ffffff" strokeWidth="0.8" strokeDasharray="3 3" />
          <path d="M50 410 L200 340 L350 410" stroke="#ffffff" strokeWidth="0.8" strokeDasharray="3 3" />
          <path d="M50 490 L200 420 L350 490" stroke="#ffffff" strokeWidth="0.8" strokeDasharray="3 3" />
          <path d="M50 570 L200 500 L350 570" stroke="#ffffff" strokeWidth="0.8" strokeDasharray="3 3" />
          <text x="230" y="240" fill="#ffffff" fillOpacity="0.5" fontSize="18" fontWeight="bold" transform="rotate(25, 230, 240)" fontFamily="sans-serif">AGRIBANK</text>
        </svg>
      </div>

      <div className="login-container">
        {/* Left Side: Brand & Introductions */}
        <section className="login-brand-side">
          <div className="brand-logo-wrap">
            <svg viewBox="0 0 32 32" width="36" height="36" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect width="32" height="32" rx="6" fill="#053e2b" />
              <rect x="1.5" y="1.5" width="29" height="29" rx="4.5" stroke="#ffdd00" strokeWidth="1.5" />
              <path d="M16 6V26" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
              <path d="M16 8C14 10 11 13 11 17" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
              <path d="M16 8C18 10 21 13 21 17" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
              <path d="M16 14C14 16 12 18 12 21" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
              <path d="M16 14C18 16 20 18 20 21" stroke="#ffdd00" strokeWidth="1.8" strokeLinecap="round" />
              <circle cx="16" cy="7" r="1.2" fill="#fff" />
              <circle cx="12" cy="12" r="1" fill="#fff" />
              <circle cx="20" cy="12" r="1" fill="#fff" />
              <circle cx="13" cy="17" r="1" fill="#fff" />
              <circle cx="19" cy="17" r="1" fill="#fff" />
            </svg>
            <div className="brand-logo-text">
              <span className="brand-logo-title">AGRIBANK</span>
              <span className="brand-logo-slogan">Đồng hành cùng phát triển</span>
            </div>
          </div>

          <h1 className="brand-main-title">
            HỆ THỐNG QUẢN LÝ<br />CÔNG VIỆC KTNB AGRIBANK
          </h1>
          <p className="brand-desc">
            Giải pháp quản trị công việc toàn diện, nâng cao hiệu suất và tối ưu quy trình vận hành trong toàn hệ thống.
          </p>

          {/* Features horizontal grid */}
          <div className="login-features-grid">
            <div className="login-feature-card">
              <div className="feature-card-icon">
                <svg fill="none" height="18" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="18">
                  <path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2" />
                  <rect x="9" y="3" width="6" height="4" rx="2" />
                  <path d="M9 14h6" />
                  <path d="M9 10h6" />
                  <path d="M9 18h6" />
                </svg>
              </div>
              <span className="feature-card-title">Quản lý công việc</span>
              <p className="feature-card-desc">
                Theo dõi tiến độ và phân công công việc hiệu quả.
              </p>
            </div>

            <div className="login-feature-card">
              <div className="feature-card-icon">
                <svg fill="none" height="18" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="18">
                  <path d="M18 20V10" />
                  <path d="M12 20V4" />
                  <path d="M6 20v-6" />
                </svg>
              </div>
              <span className="feature-card-title">Báo cáo trực quan</span>
              <p className="feature-card-desc">
                Hệ thống báo cáo thông minh, chính xác theo thời gian thực.
              </p>
            </div>

            <div className="login-feature-card">
              <div className="feature-card-icon">
                <svg fill="none" height="18" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="18">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
                  <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                </svg>
              </div>
              <span className="feature-card-title">Bảo mật an toàn</span>
              <p className="feature-card-desc">
                Đảm bảo an toàn thông tin theo tiêu chuẩn Agribank.
              </p>
            </div>
          </div>

          <div className="brand-footer">
            <svg fill="none" height="12" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="12">
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
            </svg>
            <span>Bảo mật bởi Agribank. Mọi quyền được bảo lưu.</span>
          </div>
        </section>

        {/* Right Side: Elevated White Login Form Card */}
        <section className="login-form-card">
          <div className="form-header">
            <h2 className="form-title">Đăng nhập hệ thống</h2>
            <p className="form-subtitle">Vui lòng đăng nhập để tiếp tục</p>
          </div>

          <div className="form-divider-wrap">
            <div className="form-divider-line" />
            <div className="form-divider-icon">
              <svg fill="none" height="14" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="14">
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
                <path d="M12 8v4" />
                <path d="M12 16h.01" />
              </svg>
            </div>
            <div className="form-divider-line" />
          </div>

          <form className="flex flex-col" onSubmit={submit}>
            <div className="form-group-wrap">
              {/* Username Input Box */}
              <div>
                <label className="input-label" htmlFor="username">Tên đăng nhập</label>
                <div className="input-box-container">
                  <span className="input-icon-left">
                    <svg fill="none" height="16" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24" width="16">
                      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                      <circle cx="12" cy="7" r="4" />
                    </svg>
                  </span>
                  <input
                    id="username"
                    autoComplete="username"
                    className="login-form-input"
                    placeholder="Nhập tên đăng nhập"
                    {...form.register("username")}
                  />
                </div>
                {form.formState.errors.username?.message ? (
                  <span className="text-[11px] text-red-600 mt-1 block font-medium">
                    {form.formState.errors.username.message}
                  </span>
                ) : null}
              </div>

              {/* Password Input Box */}
              <div>
                <label className="input-label" htmlFor="password">Mật khẩu</label>
                <div className="input-box-container">
                  <span className="input-icon-left">
                    <svg fill="none" height="16" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24" width="16">
                      <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
                      <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                    </svg>
                  </span>
                  <input
                    id="password"
                    autoComplete="current-password"
                    className="login-form-input"
                    placeholder="Nhập mật khẩu"
                    type={showPassword ? "text" : "password"}
                    {...form.register("password")}
                  />
                  <button
                    type="button"
                    className="input-icon-right"
                    onClick={() => setShowPassword((prev) => !prev)}
                    title={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                  >
                    {showPassword ? (
                      <svg fill="none" height="16" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="16">
                        <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" />
                        <line x1="1" x2="23" y1="1" y2="23" />
                      </svg>
                    ) : (
                      <svg fill="none" height="16" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="16">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                        <circle cx="12" cy="12" r="3" />
                      </svg>
                    )}
                  </button>
                </div>
                {form.formState.errors.password?.message ? (
                  <span className="text-[11px] text-red-600 mt-1 block font-medium">
                    {form.formState.errors.password.message}
                  </span>
                ) : null}
              </div>
            </div>

            {/* Error Message */}
            {form.formState.errors.root?.message ? (
              <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-2.5 text-xs text-red-600 font-medium mb-4">
                {form.formState.errors.root.message}
              </div>
            ) : null}

            {/* Form options: remember & forgot pwd */}
            <div className="form-options-bar">
              <label className="checkbox-label">
                <input type="checkbox" />
                <span>Ghi nhớ đăng nhập</span>
              </label>
              <a href="#" className="forgot-pwd-link" onClick={(e) => e.preventDefault()}>
                Quên mật khẩu?
              </a>
            </div>

            {/* Submit Button */}
            <button
              className="btn-agri-submit"
              disabled={loginMutation.isPending}
              type="submit"
            >
              <span>{loginMutation.isPending ? "ĐANG ĐĂNG NHẬP..." : "ĐĂNG NHẬP"}</span>
              <span className="btn-agri-submit-arrow">
                <svg fill="none" height="15" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24" width="15">
                  <line x1="5" x2="19" y1="12" y2="12" />
                  <polyline points="12 5 19 12 12 19" />
                </svg>
              </span>
            </button>
          </form>

          {/* Social / SSO / OTP login dividers */}
          <div className="or-divider">HOẶC ĐĂNG NHẬP QUA</div>

          <div className="social-login-grid">
            <button className="btn-social-login" type="button" onClick={() => window.alert("Hệ thống liên kết SSO đang bảo trì.")}>
              <svg viewBox="0 0 24 24" width="14" height="14" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2s8 3 8 8c0 5-8 11-8 11S4 15 4 10c0-5 8-8 8-8z" fill="#ae1c3f" />
                <path d="M9 7l6 6M11 5l6 6" stroke="#ffffff" strokeWidth="2" strokeLinecap="round" />
              </svg>
              <span>SSO Agribank</span>
            </button>
            <button className="btn-social-login" type="button" onClick={() => window.alert("Đang liên kết thiết bị bảo mật OTP.")}>
              <svg viewBox="0 0 24 24" width="13" height="13" fill="none" xmlns="http://www.w3.org/2000/svg" style={{ color: "var(--ink)" }}>
                <rect x="3" y="10" width="18" height="11" rx="2" ry="2" stroke="currentColor" strokeWidth="2.2" />
                <path d="M7 10V7a5 5 0 0 1 10 0v3" stroke="currentColor" strokeWidth="2.2" />
                <circle cx="12" cy="15" r="1" fill="currentColor" />
              </svg>
              <span>OTP Token</span>
            </button>
          </div>

          {/* Call support line */}
          <div className="form-footer-support">
            <svg fill="none" height="14" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24" width="14">
              <path d="M3 18v-6a9 9 0 0 1 18 0v6" />
              <path d="M21 19a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3zM3 19a2 2 0 0 0 2 2h1a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H3z" />
            </svg>
            <span>Cần hỗ trợ? Liên hệ Trung tâm hỗ trợ Agribank</span>
          </div>
        </section>
      </div>
    </main>
  );
}
