import { PersonalEvaluationRow } from "@/features/personal-evaluation/components/PersonalEvaluationRow";
import type { PersonalEvaluationItem, SaveItemPayload } from "@/features/personal-evaluation/types";

interface Access {
  canText: boolean;
  canSelf: boolean;
  canTeam: boolean;
  canManager: boolean;
  canDeputy: boolean;
  canHead: boolean;
  canDelete: boolean;
}

interface Props {
  items: PersonalEvaluationItem[];
  access: Access;
  onSave: (id: string, payload: SaveItemPayload) => void;
  onDelete: (id: string) => void;
}

export function PersonalEvaluationTable({ items, access, onSave, onDelete }: Props) {
  return (
    <div className="twrap" style={{ maxHeight: 600 }}>
      <table className="personal-table">
        <thead>
          <tr className="personal-head-top">
            <th rowSpan={3} style={{ width: 50 }}>STT</th>
            <th rowSpan={3} style={{ minWidth: 220 }}>Văn bản/Phiếu giao việc/Bút phê của Trưởng KTNB/Trường phòng</th>
            <th rowSpan={3} style={{ minWidth: 180 }}>Tên công việc được giao</th>
            <th rowSpan={3} style={{ minWidth: 300 }}>Chi tiết công việc được giao</th>
            <th rowSpan={3} style={{ minWidth: 130 }}>Thời hạn, yêu cầu hoàn thành</th>
            <th colSpan={4} style={{ minWidth: 590 }}>Kết quả thực hiện của Người lao động</th>
            <th colSpan={2} style={{ minWidth: 160 }}>Đánh giá của Trưởng đoàn KTNB/Tổ trưởng</th>
            <th colSpan={2} style={{ minWidth: 160 }}>Đánh giá của lãnh đạo phòng</th>
            <th colSpan={4} style={{ minWidth: 320 }}>Đánh giá của lãnh đạo KTNB</th>
            <th colSpan={2} style={{ minWidth: 160 }}>Điểm bình quân</th>
            <th rowSpan={3} style={{ minWidth: 180 }}>Ghi chú</th>
            <th rowSpan={3} style={{ width: 60 }}>Xóa</th>
          </tr>
          <tr className="personal-head-sub">
            <th rowSpan={2} style={{ minWidth: 300 }}>Kết quả công việc (số văn bản/báo cáo…)</th>
            <th rowSpan={2} style={{ minWidth: 130 }}>Thời gian hoàn thành</th>
            <th colSpan={2} style={{ minWidth: 160 }}>Người lao động (tự đánh giá)</th>
            <th rowSpan={2} style={{ minWidth: 80 }}>Điểm tiến độ</th>
            <th rowSpan={2} style={{ minWidth: 80 }}>Điểm chất lượng</th>
            <th rowSpan={2} style={{ minWidth: 80 }}>Điểm tiến độ</th>
            <th rowSpan={2} style={{ minWidth: 80 }}>Điểm chất lượng</th>
            <th colSpan={2} style={{ minWidth: 160 }}>Phó trưởng KTNB phụ trách</th>
            <th colSpan={2} style={{ minWidth: 160 }}>Trưởng KTNB</th>
            <th rowSpan={2} style={{ minWidth: 80 }}>Điểm tiến độ</th>
            <th rowSpan={2} style={{ minWidth: 80 }}>Điểm chất lượng</th>
          </tr>
          <tr className="personal-head-sub">
            <th style={{ minWidth: 80 }}>Điểm tiến độ</th>
            <th style={{ minWidth: 80 }}>Điểm chất lượng</th>
            <th style={{ minWidth: 80 }}>Điểm tiến độ</th>
            <th style={{ minWidth: 80 }}>Điểm chất lượng</th>
            <th style={{ minWidth: 80 }}>Điểm tiến độ</th>
            <th style={{ minWidth: 80 }}>Điểm chất lượng</th>
          </tr>
        </thead>
        <tbody>
          {items.map((it, idx) => (
            <PersonalEvaluationRow
              key={it.id}
              index={idx}
              item={it}
              access={access}
              onSave={onSave}
              onDelete={onDelete}
            />
          ))}
          {items.length === 0 && (
            <tr>
              <td colSpan={22} style={{ textAlign: "center", color: "#9ca3af", padding: 20 }}>
                Chưa có dòng công việc nào.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
