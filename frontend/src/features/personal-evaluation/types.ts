export type PersonalScorer = "self" | "teamLead" | "manager" | "deputy" | "head" | "none";

export interface PersonalEvaluationItem {
  id: string;
  periodId: string;
  displayOrder: number;
  assignmentSource: string | null;
  taskName: string | null;
  taskDetail: string | null;
  actualResult: string | null;
  note: string | null;
  deadline: string | null;
  completedAt: string | null;
  selfProgressScore: number | null;
  selfQualityScore: number | null;
  teamLeadProgressScore: number | null;
  teamLeadQualityScore: number | null;
  managerProgressScore: number | null;
  managerQualityScore: number | null;
  deputyProgressScore: number | null;
  deputyQualityScore: number | null;
  headProgressScore: number | null;
  headQualityScore: number | null;
}

export interface PersonalEvaluationPeriod {
  id: string;
  userId: string;
  userFullName: string;
  departmentId: string;
  departmentName: string;
  reportYear: number;
  reportMonth: number;
  status: string;
  capacityAttitudeSelfScore: number | null;
  capacityAttitudeTeamLeadScore: number | null;
  capacityAttitudeManagerScore: number | null;
  capacityAttitudeDeputyScore: number | null;
  capacityAttitudeHeadScore: number | null;
  disciplineSelfScore: number | null;
  disciplineTeamLeadScore: number | null;
  disciplineManagerScore: number | null;
  disciplineDeputyScore: number | null;
  disciplineHeadScore: number | null;
  inspectionSelfScore: number | null;
  inspectionTeamLeadScore: number | null;
  inspectionManagerScore: number | null;
  inspectionDeputyScore: number | null;
  inspectionHeadScore: number | null;
}

export interface PersonalEvaluationResponse {
  period: PersonalEvaluationPeriod;
  items: PersonalEvaluationItem[];
}

export interface ScorableUser {
  id: string;
  fullName: string;
  departmentId: string | null;
  departmentCode: string | null;
  departmentName: string | null;
  roleCode: string;
  roleName: string;
}

export type SaveItemPayload = Omit<PersonalEvaluationItem, "id" | "periodId">;

export type SavePeriodPayload = Pick<
  PersonalEvaluationPeriod,
  | "capacityAttitudeSelfScore"
  | "capacityAttitudeTeamLeadScore"
  | "capacityAttitudeManagerScore"
  | "capacityAttitudeDeputyScore"
  | "capacityAttitudeHeadScore"
  | "disciplineSelfScore"
  | "disciplineTeamLeadScore"
  | "disciplineManagerScore"
  | "disciplineDeputyScore"
  | "disciplineHeadScore"
  | "inspectionSelfScore"
  | "inspectionTeamLeadScore"
  | "inspectionManagerScore"
  | "inspectionDeputyScore"
  | "inspectionHeadScore"
>;
