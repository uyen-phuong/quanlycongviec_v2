export type PlanScope = "main";

export type ApprovalStatus =
  | "draft"
  | "pending"
  | "approved_1"
  | "approved_2"
  | "approved_3"
  | "returned";

export interface PlanListItem {
  id: string;
  scope: PlanScope;
  year: number;
  month: number;
  status: ApprovalStatus;
  departmentId: string | null;
  departmentCode: string | null;
  departmentName: string | null;
  createdById: string;
  createdByName: string | null;
  submittedAt: string | null;
  approvedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PlanDetail {
  id: string;
  scope: PlanScope;
  year: number;
  month: number;
  status: ApprovalStatus;
  departmentId: string | null;
  departmentCode: string | null;
  departmentName: string | null;
  createdById: string;
  createdByName: string | null;
  submittedAt: string | null;
  approvedAt: string | null;
  taskCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface PlanListFilters {
  page: number;
  pageSize: number;
  year: number | null;
  month: number | null;
  status: ApprovalStatus | "";
  departmentId?: string | null;
}

export interface PlanFormValues {
  year: number;
  month: number;
  departmentId: string | null;
}

export interface ApprovalHistoryItem {
  id: string;
  planId: string;
  action: string;
  fromStatus: string;
  toStatus: string;
  actorUserId: string;
  actorUserName: string | null;
  comment: string | null;
  createdAt: string;
}

export interface ReturnLineCommentDraft {
  taskId: string;
  content: string;
}

export interface ImportMainPlanExcelResult {
  success: boolean;
  fileName: string;
  sheetName: string;
  headerRowNumber: number;
  totalRows: number;
  headerRows: number;
  taskRows: number;
  replacedTasks: number;
}
