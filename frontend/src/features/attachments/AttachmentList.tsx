import { attachmentApi } from "@/features/attachments/api";
import type { AttachmentDto } from "@/features/attachments/types";

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

function formatDate(iso: string) {
  return new Intl.DateTimeFormat("vi-VN", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(iso));
}

async function triggerDownload(attachment: AttachmentDto) {
  const blob = await attachmentApi.download(attachment.id);
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = attachment.fileName;
  document.body.append(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}

export function AttachmentList({
  attachments,
  canDelete,
  isDeleting,
  onDelete,
}: {
  attachments: AttachmentDto[];
  canDelete: boolean;
  isDeleting: boolean;
  onDelete: (id: string) => void;
}) {
  if (attachments.length === 0) {
    return <p className="attachment-list__empty">Chua co file dinh kem.</p>;
  }

  return (
    <ul className="attachment-list">
      {attachments.map((item) => (
        <li className="attachment-item" key={item.id}>
          <div className="attachment-item__info">
            <span className="attachment-item__name" title={item.fileName}>
              {item.fileName}
            </span>
            <span className="attachment-item__meta">
              {formatBytes(item.sizeBytes)} · {item.uploadedByName ?? "Khong ro"} · {formatDate(item.createdAt)}
            </span>
          </div>
          <div className="attachment-item__actions">
            <button
              className="attachment-item__btn"
              onClick={() => {
                void triggerDownload(item);
              }}
              type="button"
            >
              Tai ve
            </button>
            {canDelete ? (
              <button
                className="attachment-item__btn attachment-item__btn--danger"
                disabled={isDeleting}
                onClick={() => onDelete(item.id)}
                type="button"
              >
                Xoa
              </button>
            ) : null}
          </div>
        </li>
      ))}
    </ul>
  );
}
