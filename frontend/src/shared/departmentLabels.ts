const departmentLabelOverrides: Record<string, string> = {
  TKTH: "Bo phan Thu ky tong hop",
  VPTNB: "Van phong Tay Nam Bo",
};

export function getDepartmentLabel(
  departmentCode: string | null | undefined,
  departmentName: string | null | undefined,
) {
  if (departmentCode && departmentLabelOverrides[departmentCode]) {
    return departmentLabelOverrides[departmentCode];
  }

  return departmentName ?? departmentCode ?? "";
}
