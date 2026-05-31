export const ROLE_ADMIN = "ADMIN";
export const ROLE_VAN_THU = "VAN_THU";
export const ROLE_TRUONG_KTNB = "TRUONG_KTNB";
export const ROLE_PHO_TRUONG_KTNB = "PHO_TRUONG_KTNB";
export const ROLE_TRUONG_PHONG = "TRUONG_PHONG";
export const ROLE_PHO_PHONG = "PHO_PHONG";
export const ROLE_NHAN_VIEN = "NHAN_VIEN";
export const ROLE_GUEST = "GUEST";

export type RoleType =
  | typeof ROLE_ADMIN
  | typeof ROLE_VAN_THU
  | typeof ROLE_TRUONG_KTNB
  | typeof ROLE_PHO_TRUONG_KTNB
  | typeof ROLE_TRUONG_PHONG
  | typeof ROLE_PHO_PHONG
  | typeof ROLE_NHAN_VIEN
  | typeof ROLE_GUEST;

export const GLOBAL_TRACKING_ROLES = [
  ROLE_ADMIN,
  ROLE_VAN_THU,
  ROLE_TRUONG_KTNB,
  ROLE_PHO_TRUONG_KTNB,
  ROLE_GUEST,
];

