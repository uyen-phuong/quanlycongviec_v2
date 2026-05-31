/* eslint-disable react-refresh/only-export-components */
import type { ReactNode } from "react";
import {
  Navigate,
  Outlet,
  createBrowserRouter,
} from "react-router-dom";
import { AdminApprovalConfigsPage } from "@/features/admin/AdminApprovalConfigsPage";
import { AdminUsersPage } from "@/features/admin/AdminUsersPage";
import { AdminConfigPage } from "@/features/admin/AdminConfigPage";
import { AdminPermissionMatrixPage } from "@/features/admin/AdminPermissionMatrixPage";
import { LoginPage } from "@/features/auth/LoginPage";
import { PlanTrackingPage } from "@/features/plan-tracking/PlanTrackingPage";
import { MainPlanDetailPage } from "@/features/plans/MainPlanDetailPage";
import { MainPlansPage } from "@/features/plans/MainPlansPage";
import { DepartmentTasksPage } from "@/features/department-tasks/DepartmentTasksPage";
import { PersonalTasksPage } from "@/features/personal-tasks/PersonalTasksPage";
import { AppShell } from "@/layouts/AppShell";
import { useAuth } from "@/shared/auth/useAuth";
import { ROLE_ADMIN } from "@/shared/auth/roles";

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
        path: "department-tasks",
        element: <DepartmentTasksPage />,
      },
      {
        path: "personal-tasks",
        element: <PersonalTasksPage />,
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
        path: "projects",
        element: <FullscreenMessage title="Module Dự án Triển khai giải pháp đang được xây dựng" />,
      },
      {
        element: <RequireRole roles={[ROLE_ADMIN]} />,
        children: [
          {
            path: "admin/users",
            element: <AdminUsersPage />,
          },
          {
            path: "admin/config",
            element: <AdminConfigPage />,
          },
          {
            path: "admin/permissions",
            element: <AdminPermissionMatrixPage />,
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
