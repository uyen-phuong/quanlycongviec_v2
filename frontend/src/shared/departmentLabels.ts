export function getDepartmentLabel(
  departmentCode: string | null | undefined,
  departmentName: string | null | undefined,
) {
  return departmentName ?? departmentCode ?? "";
}
