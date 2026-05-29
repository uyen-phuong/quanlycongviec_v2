export interface AttachmentDto {
  id: string;
  ownerType: "plan" | "task";
  ownerId: string;
  fileName: string;
  sizeBytes: number;
  contentType: string | null;
  uploadedByUserId: string;
  uploadedByName: string | null;
  createdAt: string;
}
