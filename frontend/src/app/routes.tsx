/* eslint-disable react-refresh/only-export-components */
import type { ReactNode } from "react";
import {
  Navigate,
  Outlet,
  createBrowserRouter,
} from "react-router-dom";
import { AdminApprovalConfigsPage } from "@/features/admin/AdminApprovalConfigsPage";
import { AdminDepartmentsPage } from "@/features/admin/AdminDepartmentsPage";
import { AdminRolesPage } from "@/features/admin/AdminRolesPage";
import { AdminUsersPage } from "@/features/admin/AdminUsersPage";
import { LoginPage } from "@/features/auth/LoginPage";
import { PersonalEvaluationPage } from "@/features/personal-evaluation/PersonalEvaluationPage";
import { PlanTrackingPage } from "@/features/plan-tracking/PlanTrackingPage";
import { MainPlanDetailPage } from "@/features/plans/MainPlanDetailPage";
import { MainPlansPage } from "@/features/plans/MainPlansPage";
import { AppShell } from "@/layouts/AppShell";
import { useAuth } from "@/shared/auth/useAuth";

function RequireAuth({ children }: { children: ReactNode }) {
  const auth = useAuth();

  if (auth.isBootstrapping) {
    return <FullscreenMessage title="Đang tải phiên làm việc..." />;
  }

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}

function PublicOnly({ children }: { children: ReactNode }) {
  const auth = useAuth();

  if (auth.isBootstrapping) {
    return <FullscreenMessage title="Đang tải phiên làm việc..." />;
  }

  if (auth.isAuthenticated) {
    return <Navigate to="/plan-tracking" replace />;
  }

  return <>{children}</>;
}

function RequireRole({ roles }: { roles: string[] }) {
  const auth = useAuth();

  if (!auth.isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  const allowed = auth.user?.roles.some((role) => roles.includes(role)) ?? false;
  if (!allowed) {
    return (
      <FullscreenMessage
        title="Không có quyền truy cập"
        description="Tài khoản hiện tại không được phép vào khu vực này."
      />
    );
  }

  return <Outlet />;
}


function FullscreenMessage({
  title,
  description,
}: {
  title: string;
  description?: string;
}) {
  return (
    <main className="flex min-h-screen items-center justify-center bg-paper px-6">
      <div className="max-w-md rounded-3xl border border-stone-200 bg-white p-8 text-center shadow-sm">
        <h1 className="font-serif text-2xl text-ink">{title}</h1>
        {description ? (
          <p className="mt-3 text-sm leading-7 text-stone-600">{description}</p>
        ) : null}
      </div>
    </main>
  );
}

export const router = createBrowserRouter([
  {
    path: "/login",
    element: (
      <PublicOnly>
        <LoginPage />
      </PublicOnly>
    ),
  },
  {
    path: "/",
    element: (
      <RequireAuth>
        <AppShell />
      </RequireAuth>
    ),
    children: [
      {
        index: true,
        element: <Navigate to="/plan-tracking" replace />,
      },
      {
        path: "plan-tracking",
        element: <PlanTrackingPage />,
      },
      {
        path: "plan-tracking/dept/:code",
        element: <PlanTrackingPage />,
      },
      {
        path: "plan-tracking/personal",
        element: <PersonalEvaluationPage />,
      },
      {
        path: "plans/main",
        element: <MainPlansPage />,
      },
      {
        path: "plans/main/:id",
        element: <MainPlanDetailPage />,
      },
      {
        element: <RequireRole roles={["ADMIN"]} />,
        children: [
          {
            path: "admin/users",
            element: <AdminUsersPage />,
          },
          {
            path: "admin/departments",
            element: <AdminDepartmentsPage />,
          },
          {
            path: "admin/roles",
            element: <AdminRolesPage />,
          },
          {
            path: "admin/approval-configs",
            element: <AdminApprovalConfigsPage />,
          },
        ],
      },
    ],
  },
]);
