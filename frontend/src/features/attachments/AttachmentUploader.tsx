import { useRef, useState } from "react";
import { validateAttachmentFile } from "@/features/attachments/api";

export function AttachmentUploader({
  isUploading,
  onUpload,
}: {
  isUploading: boolean;
  onUpload: (file: File) => void;
}) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [error, setError] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);

  function handleFile(file: File) {
    const validationError = validateAttachmentFile(file);
    if (validationError) {
      setError(validationError);
      return;
    }

    setError(null);
    onUpload(file);
  }

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    if (file) {
      handleFile(file);
    }

    if (inputRef.current) {
      inputRef.current.value = "";
    }
  }

  function handleDrop(event: React.DragEvent) {
    event.preventDefault();
    setIsDragging(false);
    const file = event.dataTransfer.files[0];
    if (file) {
      handleFile(file);
    }
  }

  return (
    <div className="attachment-uploader">
      <div
        className={`attachment-dropzone${isDragging ? " attachment-dropzone--active" : ""}`}
        onDragLeave={() => setIsDragging(false)}
        onDragOver={(e) => {
          e.preventDefault();
          setIsDragging(true);
        }}
        onDrop={handleDrop}
      >
        <input
          accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.csv,.png,.jpg,.jpeg,.gif,.zip,.rar,.7z"
          className="attachment-dropzone__input"
          disabled={isUploading}
          onChange={handleChange}
          ref={inputRef}
          type="file"
        />
        <span className="attachment-dropzone__label">
          {isUploading ? "Dang tai len..." : "Keo thu hoac bam de chon file (toi da 50MB)"}
        </span>
      </div>
      {error ? <p className="attachment-uploader__error">{error}</p> : null}
    </div>
  );
}
