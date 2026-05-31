import { useState, useMemo } from "react";
import { useAuth } from "@/shared/auth/useAuth";
import { getDepartmentLabel } from "@/shared/departmentLabels";
import { TaskTable } from "@/features/plan-tracking/components/TaskTable";
import {
  useDepartments,
  useDepartmentTasks,
  useCreateDepartmentTask,
  useSaveDepartmentTask,
  useDeleteDepartmentTask,
  useSubmitDepartmentTaskSingle,
  useAssignDepartmentTaskSingle,
  useApproveDepartmentTaskSingle,
  useReturnDepartmentTaskSingle,
  useDepartmentUsers,
} from "@/features/plan-tracking/hooks";
import type { TaskListItem, TaskRowViewModel } from "@/features/plan-tracking/types";
import "@/features/plan-tracking/PlanTrackingPage.css";

export function DepartmentTasksPage() {
  const auth = useAuth();
  const user = auth.user;
  const userRoles = user?.roles ?? [];
  const departmentCode = user?.departmentCode ?? null;

  const departmentsQuery = useDepartments();
  const ownDepartment = (departmentsQuery.data ?? []).find(
    (d) => d.code === departmentCode,
  );

  const tasksQuery = useDepartmentTasks(departmentCode);
  const createMutation = useCreateDepartmentTask(departmentCode);
  const saveMutation = useSaveDepartmentTask(departmentCode);
  const deleteMutation = useDeleteDepartmentTask(departmentCode);
  
  const submitWfMutation = useSubmitDepartmentTaskSingle(departmentCode);
  const assignWfMutation = useAssignDepartmentTaskSingle(departmentCode);
  const approveWfMutation = useApproveDepartmentTaskSingle(departmentCode);
  const returnWfMutation = useReturnDepartmentTaskSingle(departmentCode);

  const usersQuery = useDepartmentUsers(ownDepartment?.id ?? "");

  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(null);

  const tasks = tasksQuery.data ?? [];
  
  const rows = useMemo<TaskRowViewModel[]>(() => {
    // Standard flat display since department tasks do not have complex nesting or plan headers
    return tasks.map((task) => ({
      task,
      depth: 0,
    }));
  }, [tasks]);

  const selectedTask = tasks.find((t) => t.id === selectedTaskId) ?? null;

  const canAddTask = userRoles.some((r) =>
    ["TRUONG_PHONG", "PHO_TRUONG_KTNB", "TRUONG_NHOM", "ADMIN"].includes(r),
  );

  return (
    <main className="plan-tracking-page">
      <header className="plan-tracking-page__header">
        <div>
          <h1 className="plan-tracking-page__title">Công việc riêng của Phòng</h1>
          <p className="plan-tracking-page__subtitle">
            Quản lý và giao việc riêng độc lập của phòng{" "}
            <span style={{ color: "#ffdd00", fontWeight: 700 }}>
              {getDepartmentLabel(departmentCode, ownDepartment?.name ?? null)}
            </span>
          </p>
        </div>
      </header>

      <div className="plan-tracking-page__content">
        {tasksQuery.isLoading ? (
          <div className="flex h-64 items-center justify-center text-stone-500">
            Đang tải danh sách công việc riêng...
          </div>
        ) : (
          <TaskTable
            rows={rows}
            scope="sub"
            userRoles={userRoles}
            departments={departmentsQuery.data ?? []}
            users={usersQuery.data ?? []}
            minDeadline=""
            planDepartmentId={ownDepartment?.id ?? null}
            planId={null}
            planStatus="draft" // bypasses standard PLAN_EDITABLE check to allow edits
            canAddTask={canAddTask}
            selectedTask={selectedTask}
            selectedComments={[]}
            allComments={[]}
            approvalHistory={[]}
            canResolveComment={false}
            isResolvingComment={false}
            onOpenDetails={(t) => setSelectedTaskId(t.id)}
            onCloseDetails={() => setSelectedTaskId(null)}
            onResolveComment={() => {}}
            onAddComment={async () => {}}
            onCreateTask={async (payload) => {
              await createMutation.mutateAsync(payload);
            }}
            onSave={async (taskId, payload) => {
              await saveMutation.mutateAsync({ taskId, payload });
            }}
            onDeleteTask={async (taskId) => {
              await deleteMutation.mutateAsync(taskId);
            }}
            onSubmitTaskSingle={async (taskId, comment) => {
              await submitWfMutation.mutateAsync({ taskId, comment });
            }}
            onAssignTaskSingle={async (taskId, assigneeUserId, controllerUserId) => {
              await assignWfMutation.mutateAsync({ taskId, assigneeUserId, controllerUserId });
            }}
            onApproveTaskSingle={async (taskId, comment) => {
              await approveWfMutation.mutateAsync({ taskId, comment });
            }}
            onReturnTaskSingle={async (taskId, comment) => {
              await returnWfMutation.mutateAsync({ taskId, comment });
            }}
          />
        )}
      </div>
    </main>
  );
}
