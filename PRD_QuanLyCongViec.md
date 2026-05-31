# PRD – HỆ THỐNG QUẢN LÝ CÔNG VIỆC NỘI BỘ

**Phiên bản:** 1.0  
**Ngày:** 31/05/2026

---

## 1. GIỚI THIỆU CHUNG

Hệ thống web nội bộ nhằm quản lý toàn bộ công việc của công ty (100+ nhân viên, 10 phòng ban). Hỗ trợ lập kế hoạch, giao việc, theo dõi tiến độ, báo cáo và phân quyền chi tiết theo từng vai trò.

---

## 2. VAI TRÒ NGƯỜI DÙNG

| Vai trò | Quyền hạn chính |
|---------|-----------------|
| **Admin** | Truy cập mọi menu, chỉ thao tác trên menu Quản trị hệ thống (không thêm/sửa/xóa công việc). |
| **Lãnh đạo KTNB** (Trưởng KTNB, Phó KTNB) | Xem toàn bộ công việc toàn công ty (trừ menu Quản trị). Phê duyệt Kế hoạch/Dự án, duyệt kỳ báo cáo. |
| **Trưởng phòng (TP)** | Xem công việc của phòng mình, menu KH KTNB & Dự án. Phân công, kiểm soát, giao việc, phê duyệt. |
| **Phó phòng / Trưởng nhóm** | Tương tự TP nhưng quyền "Kiểm soát" công việc chỉ khi được TP phân công cụ thể trên từng Task. |
| **Nhân viên (NV)** | Xem việc phòng mình, task được giao, việc cá nhân. Tạo việc phòng & cá nhân, cập nhật tiến độ, trình duyệt. |
| **Văn thư** | Như NV trong phòng, nhưng có quyền tạo mới Kế hoạch công tác KTNB và Dự án. |
| **Guest / Người giám sát** | Chỉ xem menu KH KTNB và Dự án (không thêm/sửa/xóa). |

**Ghi chú:** Một người có thể thuộc nhiều vai trò (vd: TP phòng A, nhưng là thành viên Dự án của phòng B).

---

## 3. PHÂN LOẠI CÔNG VIỆC (4 LOẠI CHÍNH)

1. **Kế hoạch công tác KTNB (KH KTNB)**
   - Toàn bộ công việc trọng tâm của công ty cần báo cáo cấp trên.
   - Do Văn thư (thuộc Phòng Kế hoạch) tạo, chọn phòng đầu mối, phòng phối hợp, kỳ báo cáo.
   - Quy trình duyệt: Văn thư → TP Văn thư → Lãnh đạo KTNB.

2. **Dự án Triển khai giải pháp**
   - Dự án liên quan nhiều phòng ban. Văn thư tạo, phân công Tổ trưởng/Tổ phó/Thành viên.
   - Sau khi duyệt, Tổ trưởng tạo Task và giao cho thành viên thực hiện.

3. **Công việc riêng của Phòng**
   - Công việc ngoài KH và Dự án do phòng tự tổ chức.
   - Do NV trong phòng tạo, trình TP duyệt theo workflow cấp phòng.

4. **Công việc cá nhân**
   - Việc cá nhân tự tạo để ghi chú, dự định.
   - Chỉ có 2 trạng thái: Tạo mới → Đang thực hiện → Hoàn thành. Không cần duyệt. Chỉ người tạo xem được.

---

## 4. QUY TRÌNH NGHIỆP VỤ CHI TIẾT

### 4.1. Workflow cấp phòng (Áp dụng cho Công việc riêng của Phòng, Task từ Dự án, Task từ KH KTNB)

**Các trạng thái:**  
`Tạo mới` → `Chờ Giao việc` → `Đang thực hiện` → `Chờ kiểm soát` → `Chờ phê duyệt` → `Hoàn thành`

**Luồng chính:**
1. **NV** tạo task (Tạo mới) và trình lên cấp trên → `Chờ Giao việc`.
2. **TP** thực hiện:
   - Chọn người thực hiện (nếu chưa có).
   - **Phân quyền Kiểm soát** (nếu muốn): chọn Trưởng nhóm hoặc Phó phòng làm Người kiểm soát cho task này.
   - Nhấn **"Giao việc"** → `Đang thực hiện`.
3. **NV** thực hiện xong, nhấn "Trình hoàn thành" → `Chờ kiểm soát`.
4. **Người kiểm soát** (TP, hoặc Trưởng nhóm/Phó phòng được phân quyền) kiểm tra:
   - Nếu đạt, nhấn "Kiểm soát đạt" → `Chờ phê duyệt`.
   - Nếu không đạt, nhấn "Chuyển trả" (kèm comment) → quay về `Đang thực hiện`.
5. **TP** phê duyệt cuối cùng:
   - Nếu đạt, nhấn "Phê duyệt" → `Hoàn thành`.
   - Nếu không đạt, "Chuyển trả" → quay về `Đang thực hiện`.

**Mỗi bước chuyển trả đều kèm comment và gửi thông báo cho người liên quan.**

---

### 4.2. Module "Kế hoạch Công tác KTNB"

#### 4.2.1. Tạo mới và phê duyệt KH
- **Văn thư** vào menu KH KTNB, chọn "Tạo mới", nhập:
  - Loại: `Kế hoạch công tác KTNB`.
  - Tên KH: (vd: "Kế hoạch công tác KTNB năm 2026").
  - Lãnh đạo KTNB phụ trách: Chọn 1 (TKT hoặc PKT).
  - Phòng đầu mối: Chọn 1 phòng.
  - Phòng phối hợp: Chọn nhiều phòng.
  - Kỳ báo cáo: `Tháng`, `Quý`, `6 tháng`, `Năm`.
- Trình tự duyệt: Văn thư → **TP của Văn thư** kiểm soát → **Lãnh đạo KTNB đã chọn** phê duyệt.
- Sau khi Lãnh đạo KTNB phê duyệt, KH xuất hiện trong danh sách công việc của **tất cả các phòng được chọn** với trạng thái `Chờ Giao việc`.

#### 4.2.2. Phân công trong phòng
- Tại mỗi phòng, TP thấy task KH với trạng thái `Chờ Giao việc`.
- TP thực hiện:
  - Chọn **Cán bộ đầu mối** (1 người) – chịu trách nhiệm tổng hợp và nhập báo cáo chính.
  - Chọn **Cán bộ phối hợp** (nhiều người) – hỗ trợ, nhập nội dung phối hợp.
  - Chọn **Người kiểm soát** cho task này (nếu muốn ủy quyền).
  - Nhấn **"Giao việc"** → trạng thái chuyển `Đang thực hiện`.

#### 4.2.3. Báo cáo tiến độ trong phòng
- **Cán bộ đầu mối**:
  - Nhập **nội dung tiến độ**, **tỷ lệ % hoàn thành**, chọn **kỳ báo cáo cụ thể** (vd: Tháng 1/2026).
  - Có thể nhập nhiều kỳ. Mỗi lần nhập là một bản ghi lịch sử.
  - Khi hoàn tất nội dung cho một kỳ, nhấn "Trình hoàn thành" để gửi duyệt cấp phòng (theo workflow cấp phòng: `Chờ kiểm soát` → `Chờ phê duyệt` → `Hoàn thành`).
- **Cán bộ phối hợp**:
  - Nhập **nội dung phối hợp** (trường riêng) để đầu mối tham khảo. Không cần duyệt.

#### 4.2.4. Cập nhật lên KH tổng
- Khi task tại phòng đầu mối được phê duyệt hoàn thành (ở một kỳ), nội dung tiến độ và tỷ lệ % **tự động cập nhật** vào bản ghi KH tổng ở cột tương ứng.
- Khi xem chi tiết task KH, người dùng có thể thấy **lịch sử báo cáo** của tất cả các phòng, kèm thời gian nhập.

#### 4.2.5. Duyệt kỳ báo cáo bởi Lãnh đạo KTNB
- Sau khi kỳ báo cáo kết thúc (vd: hết Quý I), Lãnh đạo KTNB vào chi tiết KH, thấy nút **"Duyệt kỳ báo cáo"**.
- Khi duyệt:
  - Kỳ báo cáo đó được đóng lại, không thể sửa thêm.
  - Công việc KH tự động chuyển sang **kỳ báo cáo tiếp theo** (nếu có) và trạng thái trở về `Đang thực hiện` ở các phòng để tiếp tục nhập báo cáo kỳ mới.
  - TP có thể thay đổi cán bộ đầu mối/phối hợp/kiểm soát cho kỳ mới.
- **Riêng kỳ báo cáo năm**: Khi duyệt kỳ Năm, công việc KH chính thức chuyển trạng thái `Hoàn thành`.

#### 4.2.6. Subtask trong phòng (tùy chọn)
- Trong task KH, TP có thể tạo các **Subtask** nhỏ để giao cho cán bộ.
- Subtask **không có workflow duyệt riêng**. Chỉ có trạng thái: `Đang thực hiện` / `Hoàn thành`.
- Khi task KH chính được duyệt hoàn thành, tất cả subtask con cũng tự động coi là hoàn thành.

---

### 4.3. Module "Dự án Triển khai giải pháp"

#### 4.3.1. Tạo mới và phê duyệt Dự án
- **Văn thư** vào menu Dự án, chọn "Tạo mới", nhập:
  - Loại: `Dự án`.
  - Tên dự án.
  - Tổ trưởng, Tổ phó, Thành viên (có thể đa phòng ban).
- Trình tự duyệt: Văn thư → **TP của Văn thư** kiểm soát → **Lãnh đạo KTNB** phê duyệt.
- Sau khi Lãnh đạo KTNB phê duyệt, Dự án chuyển trạng thái `Đang thực hiện`.

#### 4.3.2. Quản lý Task trong Dự án
- **Tổ trưởng** vào Dự án, tạo các Task con.
  - Mỗi Task giao cho **một thành viên** (thuộc danh sách thành viên dự án).
  - Task tuân theo **workflow cấp phòng** (Tạo mới → Chờ Giao việc → … → Hoàn thành), trong đó Tổ trưởng đóng vai trò Người kiểm soát/phê duyệt.
- Khi một Task được phê duyệt `Hoàn thành`, nó đóng góp vào % hoàn thành của Dự án.
- Khi **100% Task** được duyệt hoàn thành, Dự án tự động chuyển trạng thái `Hoàn thành`.

---

## 5. PHÂN QUYỀN TRUY CẬP DỮ LIỆU

| Vai trò | Công việc riêng phòng | Công việc cá nhân | KH KTNB | Dự án | Quản trị hệ thống |
|---------|----------------------|-------------------|---------|-------|-------------------|
| Admin | Xem (all) | Xem (all) | Xem | Xem | **Toàn quyền** |
| Lãnh đạo KTNB | Xem (all) | Xem (all) | Xem + Duyệt | Xem + Duyệt | Không |
| TP, P.Phòng, T.Nhóm | **Phòng mình** | Không (trừ của mình) | Xem (nếu phòng tham gia) | Xem | Không |
| Nhân viên | **Phòng mình** | **Của mình** | Xem (nếu phòng tham gia) | Xem (nếu là thành viên) | Không |
| Văn thư | Như NV | Như NV | **Tạo mới** | **Tạo mới** | Không |
| Guest / Giám sát | Không | Không | Xem | Xem | Không |

---

## 6. YÊU CẦU GIAO DIỆN & CHỨC NĂNG CHUNG

### 6.1. Giao diện chính: Dạng Table List
- Tất cả danh sách công việc hiển thị dạng bảng (table).
- **Tùy chỉnh cột**: Người dùng được ẩn/hiện, sắp xếp thứ tự cột.
- **Lưu bộ lọc cá nhân**: Lưu lại bộ lọc đã dùng để truy cập nhanh.
- **Bộ lọc hỗ trợ**: Người thực hiện, Khoảng thời gian (ngày tạo, hạn), Trạng thái, Mức độ (Bình thường, Khẩn, Hỏa tốc), Mức độ phức tạp (Thấp, Trung bình, Cao).
- **Xuất Excel**: Mọi danh sách đều có nút xuất Excel.

### 6.2. Thông báo (Notification Bell)
- Thông báo đẩy trong ứng dụng khi:
  - Được giao việc mới.
  - Task chuyển trạng thái (Chờ kiểm soát, Chờ phê duyệt, Hoàn thành, Chuyển trả).
  - Có comment mới trong task mình tham gia.
  - Sắp đến hạn (cấu hình nhắc trước).

### 6.3. Bình luận (Comment)
- Mỗi task có khu vực bình luận.
- Hỗ trợ **đính kèm file** trong bình luận.

### 6.4. Nhật ký hoạt động (Audit Log)
- Ghi lại mọi hành động: Login, Thêm/Sửa/Xóa, Thay đổi trạng thái, Phân công, Phê duyệt, Xuất báo cáo, v.v.
- Thông tin lưu: User, Thời gian, Hành động, Đối tượng, Giá trị cũ – mới.

---

## 7. BÁO CÁO

Hệ thống cung cấp các báo cáo sau, tất cả đều hỗ trợ lọc theo thời gian và xuất Excel:

1. **Báo cáo tổng hợp công việc theo Nhân viên**
   - Số lượng: Tổng, Đang thực hiện, Hoàn thành, Quá hạn, Sắp đến hạn.
   - Phân theo mức độ phức tạp, mức độ khẩn.

2. **Báo cáo tổng hợp công việc theo Phòng**
   - Tương tự như trên, gom nhóm theo phòng ban.

3. **Báo cáo chi tiết công việc của từng Nhân viên**
   - Liệt kê tất cả task với đầy đủ trường thông tin, tỷ lệ hoàn thành, trạng thái hiện tại.

4. **Báo cáo Nhật ký hoạt động**
   - Lọc theo User, Khoảng thời gian, Loại hành động.

---

## 8. YÊU CẦU KỸ THUẬT

- **Xác thực**: Email + Password. Hỗ trợ thêm Active Directory nếu cần.
- **Lưu trữ**: On-premise (máy chủ nội bộ).
- **Bảo mật**:
  - Mã hóa dữ liệu nhạy cảm (password).
  - Phân quyền chặt chẽ theo vai trò ở cả frontend và backend.
  - Audit log không thể chỉnh sửa/xóa.
- **Giao diện**: Responsive, ưu tiên desktop. Hỗ trợ tiếng Việt đầy đủ.
- **Hiệu năng**: Tối ưu cho 100+ người dùng đồng thời.

---

## 9. MỘT SỐ LƯU Ý KHI TRIỂN KHAI CODE

- Trạng thái `Chờ Giao việc` (trước đây là Chờ Apply) và nút `Giao việc` (trước đây là Apply) cần được sử dụng nhất quán.
- Cần cơ chế tự động chuyển kỳ báo cáo và cập nhật trạng thái khi Lãnh đạo duyệt kỳ.
- Subtask trong KH không có workflow riêng, việc hoàn thành task cha tự động đóng subtask.
- Phân quyền "Người kiểm soát" là động, lưu theo từng task, không phải mặc định toàn hệ thống.

---

**Tài liệu này đã sẵn sàng để bàn giao cho AI Coding.** Bạn có thể bắt đầu nhập vào hệ thống AI dưới dạng prompt mô tả hệ thống.