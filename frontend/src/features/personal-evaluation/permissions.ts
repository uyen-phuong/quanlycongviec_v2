import type { PersonalScorer } from "@/features/personal-evaluation/types";

export interface CurrentUserCtx {
  userId: string | null;
  departmentId: string | null;
  roles: string[];
}

export interface TargetCtx {
  userId: string;
  departmentId: string;
}

export function getMyScorer(roles: string[]): PersonalScorer {
  if (roles.includes("TRUONG_KTNB")) return "head";
  if (roles.includes("PHO_TRUONG_KTNB")) return "deputy";
  if (roles.includes("TRUONG_PHONG") || roles.includes("TRUONG_KH")) return "manager";
  if (roles.includes("TRUONG_NHOM")) return "teamLead";
  if (roles.includes("NHAN_VIEN")) return "self";
  return "none";
}

export function canScoreColumn(viewer: CurrentUserCtx, target: TargetCtx, column: PersonalScorer): boolean {
  if (column === "none") return false;
  if (getMyScorer(viewer.roles) !== column) return false;
  switch (column) {
    case "self": return viewer.userId === target.userId;
    case "teamLead": return !!viewer.departmentId && viewer.departmentId === target.departmentId;
    case "manager": return !!viewer.departmentId && viewer.departmentId === target.departmentId;
    case "deputy":
    case "head": return true;
    default: return false;
  }
}

export function canEditItemText(viewer: CurrentUserCtx, target: TargetCtx): boolean {
  return viewer.userId === target.userId && viewer.roles.includes("NHAN_VIEN");
}

export function canCreateOrDeleteItem(viewer: CurrentUserCtx, target: TargetCtx): boolean {
  return canEditItemText(viewer, target);
}
