export function TaskHeaderRow() {
  return (
    <thead>
      <tr>
        <th style={{ width: "50px" }}>TT</th>
        <th className="th-red" style={{ minWidth: "460px", textAlign: "left" }}>Nội dung văn bản chỉ đạo</th>
        <th className="th-red" style={{ minWidth: "160px" }}>Thành viên BKS chỉ đạo</th>
        <th className="th-red" style={{ minWidth: "160px" }}>Lãnh đạo KTNB chỉ đạo</th>
        <th style={{ minWidth: "140px" }}>Loại công việc</th>
        <th className="th-blue" style={{ minWidth: "220px" }}>Phòng đầu mối và phòng phối hợp</th>
        <th className="th-org" style={{ minWidth: "160px" }}>Hạn hoàn thành</th>
        <th className="th-grn" style={{ minWidth: "300px", textAlign: "left" }}>Tiến độ thực hiện</th>
        <th style={{ minWidth: "160px" }}>Trạng thái</th>
        <th style={{ minWidth: "130px" }}>Người nhập</th>
        <th style={{ minWidth: "130px" }}>Người kiểm soát</th>
        <th style={{ minWidth: "130px" }}>Người phê duyệt</th>
        <th style={{ minWidth: "180px" }}>Nguyên nhân chưa hoàn thành</th>
        <th style={{ minWidth: "160px" }}>Ghi chú</th>
        <th style={{ width: "48px" }}>Xóa</th>
      </tr>
    </thead>
  );
}
