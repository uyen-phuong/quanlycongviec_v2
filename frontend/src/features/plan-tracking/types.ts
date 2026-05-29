import type { DepartmentLookupDto } from "@/shared/api/dtos";

export type WorkType = 0 | 1 | 2;

export type WorkStatus =
  | "not_started"
  | "in_progress"
  | "done"
  | "overdue"
  | "paused";

/** Task-level workflow codes (used in TaskListItem.approvalStatus) */
export type ApprovalStatus =
  | "draft"
  | "pending_team"
  | "approved_team"
  | "approved_department"
  | "approved_final"
  | "returned";

/** Plan-level status codes (used in ResolvedPlan.status and plan list) */
export type PlanStatus =
  | "draft"
  | "pending"
  | "approved_1"
  | "approved_2"
  | "approved_3"
  | "returned";

export interface ResolvedPlan {
  found: boolean;
  planId: string | null;
  scope: "main" | "sub";
  year: number;
  month: number;
  departmentId: string | null;
  departmentCode: string | null;
  departmentName: string | null;
  status: PlanStatus | null;
  createdAt: string | null;
  updatedAt: string | null;
}

export interface TaskListItem {
  id: string;
  planId: string;
  parentTaskId: string | null;
  outlineIndex: string | null;
  displayOrder: number;
  isHeader: boolean;
  title: string;
  workType: number;
  workStatus: WorkStatus;
  deadline: string | null;
  assigneeUserId: string | null;
  assigneeName: string | null;
  ownerDepartmentId: string | null;
  ownerDepartmentCode: string | null;
  ownerDepartmentName: string | null;
  bksMemberText: string | null;
  ktnbLeaderText: string | null;
  noteText: string | null;
  isLocked: boolean;
  hasOpenComment: boolean;
  approvalStatus: ApprovalStatus;
  submittedAt: string | null;
  approvedAt: string | null;
  progressText: string | null;
  reasonNotCompleted: string | null;
  supportingDepartmentIds: string[];
  createdAt: string;
  updatedAt: string;
}

export interface TaskDetailResponse {
  id: string;
  planId: string;
  parentTaskId: string | null;
  outlineIndex: string | null;
  displayOrder: number;
  isHeader: boolean;
  title: string;
  workType: number;
  workStatus: WorkStatus;
  deadline: string | null;
  assigneeUserId: string | null;
  assigneeName: string | null;
  ownerDepartmentId: string | null;
  ownerDepartmentCode: string | null;
  ownerDepartmentName: string | null;
  bksMemberText: string | null;
  ktnbLeaderText: string | null;
  noteText: string | null;
  isLocked: boolean;
  hasOpenComment: boolean;
  approvalStatus: ApprovalStatus;
  submittedAt: string | null;
  approvedAt: string | null;
  progressText: string | null;
  reasonNotCompleted: string | null;
  supportingDepartments: Array<{
    id: string;
    code: string;
    name: string;
  }>;
  createdAt: string;
  updatedAt: string;
}

export interface SaveTaskPayload {
  parentTaskId: string | null;
  outlineIndex: string | null;
  displayOrder: number;
  isHeader: boolean;
  title: string;
  workType: number;
  workStatus: WorkStatus;
  deadline: string | null;
  assigneeUserId: string | null;
  ownerDepartmentId: string | null;
  bksMemberText: string | null;
  ktnbLeaderText: string | null;
  noteText: string | null;
  progressText: string | null;
  reasonNotCompleted: string | null;
  supportingDepartmentIds: string[];
}

export interface LineComment {
  id: string;
  taskId: string;
  taskTitle: string;
  taskOutlineIndex: string | null;
  authorUserId: string;
  authorUserName: string | null;
  authorRole: "controller" | "approver" | "creator";
  content: string;
  isResolved: boolean;
  resolvedAt: string | null;
  resolvedByUserId: string | null;
  resolvedByUserName: string | null;
  createdAt: string;
}

export interface TrackingContext {
  scope: "main" | "sub";
  departmentCode: string | null;
  workType: number | null;
  title: string;
}

export interface TaskRowViewModel {
  task: TaskListItem;
  depth: number;
}

export type SupportingDepartmentOption = DepartmentLookupDto;

export interface CreateTaskPayload {
  planId: string;
  parentTaskId: string | null;
  outlineIndex: string | null;
  displayOrder: number;
  isHeader: boolean;
  title: string;
  workType: number;
  workStatus: WorkStatus;
  deadline: string | null;
  assigneeUserId: string | null;
  ownerDepartmentId: string | null;
  bksMemberText: string | null;
  ktnbLeaderText: string | null;
  noteText: string | null;
  progressText: string | null;
  reasonNotCompleted: string | null;
  supportingDepartmentIds: string[];
}
