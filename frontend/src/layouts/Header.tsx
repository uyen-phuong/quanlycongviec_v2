import { useEffect, useRef, useState } from "react";
import { useAuth } from "@/shared/auth/useAuth";
import { useMarkAllNotificationsRead, useNotifications } from "@/features/notifications/hooks";
import type { NotificationDto } from "@/features/notifications/types";

const roleLabel: Record<string, string> = {
  ADMIN: "Quản trị viên",
  VAN_THU: "Văn thư",
  TRUONG_KH: "Trưởng KH",
  TRUONG_KTNB: "Trưởng KTNB",
  PHO_TRUONG_KTNB: "Phó trưởng KTNB",
  TRUONG_PHONG: "Trưởng phòng",
  TRUONG_NHOM: "Trưởng nhóm",
  NHAN_VIEN: "Nhân viên",
};

const eventLabel: Record<string, string> = {
  plan_submitted: "Gửi kiểm soát",
  plan_approved: "Phê duyệt",
  plan_returned: "Chuyển trả",
  task_created: "Thêm mới",
  task_deleted: "Xóa",
};

function timeAgo(iso: string) {
  const diff = Math.floor((Date.now() - new Date(iso).getTime()) / 1000);
  if (diff < 60) return "Vừa xong";
  if (diff < 3600) return `${Math.floor(diff / 60)} phút trước`;
  if (diff < 86400) return `${Math.floor(diff / 3600)} giờ trước`;
  return `${Math.floor(diff / 86400)} ngày trước`;
}

function NotificationItem({ item }: { item: NotificationDto }) {
  const tag = eventLabel[item.eventType] ?? item.eventType;
  return (
    <div style={{
      padding: "10px 14px",
      borderBottom: "1px solid #f0f0f0",
      background: item.isRead ? "#fff" : "#fef9f0",
      cursor: "default",
    }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: "8px" }}>
        <span style={{
          fontSize: "10px", fontWeight: 600, color: "#fff",
          background: item.isRead ? "#aaa" : "var(--red)",
          borderRadius: "3px", padding: "1px 5px", whiteSpace: "nowrap",
        }}>
          {tag}
        </span>
        <span style={{ fontSize: "10px", color: "#999", whiteSpace: "nowrap" }}>
          {timeAgo(item.createdAt)}
        </span>
      </div>
      <p style={{ margin: "5px 0 0", fontSize: "12px", color: "#333", lineHeight: 1.4 }}>
        {item.title}
      </p>
    </div>
  );
}

export function Header() {
  const auth = useAuth();
  const { data } = useNotifications();
  const markAllRead = useMarkAllNotificationsRead();
  const [open, setOpen] = useState(false);
  const dropRef = useRef<HTMLDivElement>(null);

  const unreadCount = data?.unreadCount ?? 0;
  const items = data?.items ?? [];

  useEffect(() => {
    if (!open) return;
    function handleClick(e: MouseEvent) {
      if (dropRef.current && !dropRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, [open]);

  function handleBellClick() {
    if (!open && unreadCount > 0) {
      markAllRead.mutate();
    }
    setOpen((v) => !v);
  }

  const initials = (auth.user?.fullName ?? "?")
    .split(" ")
    .slice(-2)
    .map((w) => w[0])
    .join("")
    .toUpperCase();

  const roleText = auth.user?.roles.map((r) => roleLabel[r] ?? r).join(", ");

  return (
    <header style={{
      background: "var(--red-d)",
      height: "54px",
      flexShrink: 0,
      padding: "0 22px",
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      boxShadow: "0 2px 10px rgba(0,0,0,.2)",
    }}>
      <span style={{ fontSize: "15px", fontWeight: 700, color: "#fff", letterSpacing: "-.2px" }}>
        Theo Dõi Tiến Độ Thực Hiện Kế Hoạch Công Tác
      </span>

      <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
        {/* Bell */}
        <div ref={dropRef} style={{ position: "relative" }}>
          <div
            onClick={handleBellClick}
            style={{
              width: "34px", height: "34px", borderRadius: "50%",
              background: "rgba(255,255,255,.14)", display: "flex",
              alignItems: "center", justifyContent: "center",
              cursor: "pointer", position: "relative",
            }}
          >
            <svg fill="none" height="15" stroke="#fff" strokeWidth="2" viewBox="0 0 24 24" width="15">
              <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
              <path d="M13.73 21a2 2 0 0 1-3.46 0" />
            </svg>
            {unreadCount > 0 && (
              <span style={{
                position: "absolute", top: "-3px", right: "-3px",
                background: "#e53935", color: "#fff",
                borderRadius: "9px", fontSize: "9px", fontWeight: 700,
                minWidth: "16px", height: "16px", padding: "0 4px",
                display: "flex", alignItems: "center", justifyContent: "center",
                lineHeight: 1,
              }}>
                {unreadCount > 99 ? "99+" : unreadCount}
              </span>
            )}
          </div>

          {/* Dropdown */}
          {open && (
            <div style={{
              position: "absolute", top: "42px", right: 0,
              width: "320px", background: "#fff",
              borderRadius: "8px", boxShadow: "0 4px 20px rgba(0,0,0,.18)",
              zIndex: 1000, overflow: "hidden",
              border: "1px solid #e8e8e8",
            }}>
              <div style={{
                padding: "10px 14px", borderBottom: "1px solid #f0f0f0",
                display: "flex", justifyContent: "space-between", alignItems: "center",
                background: "#fafafa",
              }}>
                <span style={{ fontSize: "12px", fontWeight: 700, color: "#333" }}>
                  Thông báo
                </span>
                {items.length > 0 && (
                  <span style={{ fontSize: "11px", color: "#888" }}>
                    {unreadCount > 0 ? `${unreadCount} chưa đọc` : "Đã đọc hết"}
                  </span>
                )}
              </div>

              <div style={{ maxHeight: "360px", overflowY: "auto" }}>
                {items.length === 0 ? (
                  <div style={{ padding: "24px", textAlign: "center", color: "#aaa", fontSize: "12px" }}>
                    Chưa có thông báo
                  </div>
                ) : (
                  items.map((item) => <NotificationItem key={item.id} item={item} />)
                )}
              </div>
            </div>
          )}
        </div>

        {/* User info */}
        <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
          <div style={{
            width: "34px", height: "34px",
            background: "linear-gradient(135deg, rgba(255,255,255,.24) 0%, rgba(255,255,255,.08) 100%)",
            border: "1.5px solid var(--gold)",
            borderRadius: "50%", display: "flex", alignItems: "center",
            justifyContent: "center", fontWeight: 700, fontSize: "11.5px", color: "#fff",
            boxShadow: "0 2px 6px rgba(0,0,0,.15)",
          }}>
            {initials}
          </div>
          <div>
            <div style={{ fontSize: "12.5px", fontWeight: 600, color: "#fff" }}>
              {auth.user?.fullName ?? "Guest"}
            </div>
            <div style={{ fontSize: "10px", color: "rgba(255,255,255,.7)", fontWeight: 500 }}>
              {roleText}
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}
