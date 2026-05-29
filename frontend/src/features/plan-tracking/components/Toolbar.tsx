import type { PlanStatus } from "@/features/plan-tracking/types";

function hasRole(userRoles: string[], targetRoles: string[]) {
  return userRoles.some((role) => targetRoles.includes(role));
}

function canSubmit(status: PlanStatus | null, scope: "main" | "sub", userRoles: string[]) {
  if (!status || !["draft", "returned"].includes(status)) return false;
  return scope === "main"
    ? hasRole(userRoles, ["VAN_THU"])
    : hasRole(userRoles, ["TRUONG_PHONG", "PHO_TRUONG_KTNB", "TRUONG_NHOM", "NHAN_VIEN"]);
}

function canReview(status: PlanStatus | null, scope: "main" | "sub", userRoles: string[]) {
  if (!status) return false;
  if (scope === "main") {
    return status === "pending" && hasRole(userRoles, ["TRUONG_KH"]);
  }

  return status === "pending" && hasRole(userRoles, ["TRUONG_PHONG"]);
}

function canApprove(status: PlanStatus | null, scope: "main" | "sub", userRoles: string[]) {
  if (!status) return false;
  if (scope === "main") {
    return status === "approved_1" && hasRole(userRoles, ["TRUONG_KTNB"]);
  }

  return status === "approved_2" && hasRole(userRoles, ["PHO_TRUONG_KTNB"]);
}

function canReturn(status: PlanStatus | null, scope: "main" | "sub", userRoles: string[]) {
  return canReview(status, scope, userRoles) || canApprove(status, scope, userRoles);
}

function getStatusLabel(status: PlanStatus | null, scope: "main" | "sub") {
  const fallback = status ?? "draft";
  if (scope === "main") {
    const mainLabels: Record<string, string> = {
      draft: "Chưa phê duyệt",
      pending: "Chờ kiểm soát",
      approved_1: "Đã kiểm soát",
      approved_2: "Đã phê duyệt",
      returned: "Đã chuyển trả",
    };

    return mainLabels[fallback] ?? fallback;
  }

  const subLabels: Record<string, string> = {
    draft: "Chưa phê duyệt",
    pending: "Chờ kiểm soát",
    approved_1: "Đã kiểm soát",
    approved_2: "Đã kiểm soát",
    approved_3: "Đã phê duyệt",
    returned: "Đã chuyển trả",
  };

  return subLabels[fallback] ?? fallback;
}

const roleLabel: Record<string, string> = {
  ADMIN: "Admin thực hiện",
  VAN_THU: "Văn thư",
  TRUONG_KH: "Trưởng KH",
  TRUONG_KTNB: "Trưởng KTNB",
  PHO_TRUONG_KTNB: "Phó trưởng KTNB",
  TRUONG_PHONG: "Trưởng phòng",
  TRUONG_NHOM: "Trưởng nhóm",
  NHAN_VIEN: "Nhân viên",
};

function getReviewLabel(_scope: "main" | "sub") {
  return "Kiểm soát";
}

function getApproveLabel(_scope: "main" | "sub") {
  return "Phê duyệt";
}

function getReviewTitle(scope: "main" | "sub") {
  return scope === "main"
    ? "Trưởng phòng Kế hoạch kiểm soát hoặc chuyển trả"
    : "Trưởng phòng kiểm soát (chờ → đã kiểm soát)";
}

function getApproveTitle(scope: "main" | "sub") {
  return scope === "main"
    ? "Trưởng KTNB phê duyệt hoặc chuyển trả"
    : "Bước phê duyệt cuối";
}

function getSubmitTitle(scope: "main" | "sub") {
  return scope === "main"
    ? "Văn thư chuyển dữ liệu sang trạng thái chờ duyệt"
    : "Gửi công việc của đơn vị sang trạng thái chờ kiểm soát";
}


export function Toolbar({
  scope,
  status,
  userRoles,
  hasPlan,
  onSubmit,
  onApprove,
  onJumpToDetail,
}: {
  scope: "main" | "sub";
  status: PlanStatus | null;
  userRoles: string[];
  hasPlan: boolean;
  onSubmit: () => void;
  onApprove: () => void;
  onJumpToDetail: (action: "import" | "export" | "return") => void;
}) {
  const submitEnabled = hasPlan && canSubmit(status, scope, userRoles);
  const reviewEnabled = hasPlan && canReview(status, scope, userRoles);
  const approveEnabled = hasPlan && canApprove(status, scope, userRoles);
  const importEnabled =
    hasPlan &&
    scope === "main" &&
    ["draft", "returned"].includes(status ?? "") &&
    hasRole(userRoles, ["VAN_THU", "ADMIN"]);
  const exportEnabled = hasPlan && scope === "main" && hasRole(userRoles, ["VAN_THU", "ADMIN"]);
  const returnEnabled = hasPlan && canReturn(status, scope, userRoles);

  const pillClass = `status-pill plan-${status ?? "draft"}`;
  const roleText = userRoles.map((r) => roleLabel[r] ?? r).join(", ");

  return (
    <>
      <div className="ctb">
        <div style={{ display: "flex", gap: "7px" }}>
          <button
            className="btn-g"
            disabled={!importEnabled}
            onClick={() => onJumpToDetail("import")}
            title={importEnabled ? undefined : "Chỉ khả dụng khi kế hoạch draft/returned và có quyền VAN_THU/ADMIN"}
            type="button"
          >
            <svg fill="none" height="14" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24" width="14">
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
              <polyline points="7 10 12 15 17 10" />
              <line x1="12" x2="12" y1="15" y2="3" />
            </svg>
            Upload Excel
          </button>
          <button
            className="btn-g"
            disabled={!exportEnabled}
            onClick={() => onJumpToDetail("export")}
            type="button"
          >
            Xuất Excel
          </button>
          <button
            className="btn-b"
            disabled={!submitEnabled}
            onClick={onSubmit}
            title={submitEnabled ? getSubmitTitle(scope) : "Dữ liệu được lưu tự động"}
            type="button"
          >
            Lưu dữ liệu
          </button>
          <button
            className="btn-o"
            disabled={!returnEnabled}
            onClick={() => onJumpToDetail("return")}
            type="button"
          >
            Chuyển trả
          </button>
          <button
            className="btn-r"
            disabled={!reviewEnabled}
            onClick={onApprove}
            title={reviewEnabled ? getReviewTitle(scope) : undefined}
            type="button"
          >
            {getReviewLabel(scope)}
          </button>
          <button
            className="btn-g"
            disabled={!approveEnabled}
            onClick={onApprove}
            title={approveEnabled ? getApproveTitle(scope) : undefined}
            type="button"
          >
            {getApproveLabel(scope)}
          </button>
        </div>
      </div>

      <div className="status-row">
        <span className="status-lbl">Trạng thái dữ liệu:</span>
        <span className={pillClass}>{getStatusLabel(status, scope)}</span>
        <span className="status-role">Vai trò: {roleText}</span>
      </div>
    </>
  );
}
