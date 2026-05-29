using KHCT.Application.Common.Interfaces;
using KHCT.Application.Common.Support;
using KHCT.Application.Plans;
using KHCT.Application.Tasks;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Attachments;

public static class AttachmentSupport
{
    public const string OwnerTypePlan = "plan";
    public const string OwnerTypeTask = "task";
    public const long MaxSizeBytes = 50L * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv",
        ".png", ".jpg", ".jpeg", ".gif",
        ".zip", ".rar", ".7z"
    };

    public static AttachmentListItemDto ToListItem(Attachment attachment) =>
        new(
            attachment.Id,
            attachment.OwnerType,
            attachment.OwnerId,
            attachment.FileName,
            attachment.SizeBytes,
            attachment.ContentType,
            attachment.UploadedByUserId,
            attachment.UploadedByUser?.FullName,
            attachment.CreatedAt);

    public static object Snapshot(Attachment attachment) =>
        new
        {
            attachment.Id,
            attachment.OwnerType,
            attachment.OwnerId,
            attachment.FileName,
            attachment.StoredPath,
            attachment.SizeBytes,
            attachment.ContentType,
            attachment.UploadedByUserId
        };

    public static string NormalizeFileName(string fileName)
    {
        var normalized = Path.GetFileName(fileName).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainException("invalid_file_name", "File name is invalid.");
        }

        return normalized;
    }

    public static string ValidateAndGetExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new DomainException("invalid_file_format", "File extension is not supported.");
        }

        return extension.ToLowerInvariant();
    }

    public static void ValidateFileSize(long sizeBytes)
    {
        if (sizeBytes <= 0 || sizeBytes > MaxSizeBytes)
        {
            throw new DomainException("file_size_invalid", "File size exceeds the allowed limit.");
        }
    }

    public static void ValidateSignature(string extension, byte[] content)
    {
        if (content.Length == 0)
        {
            throw new DomainException("invalid_file", "File is empty.");
        }

        if (extension == ".pdf" && !StartsWith(content, "%PDF-"u8))
        {
            throw new DomainException("invalid_file_signature", "PDF signature is invalid.");
        }

        if ((extension is ".docx" or ".xlsx" or ".pptx" or ".zip") && !StartsWith(content, [0x50, 0x4B, 0x03, 0x04]))
        {
            throw new DomainException("invalid_file_signature", "ZIP-based file signature is invalid.");
        }

        if ((extension is ".doc" or ".xls" or ".ppt") && !StartsWith(content, [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]))
        {
            throw new DomainException("invalid_file_signature", "Office binary file signature is invalid.");
        }

        if (extension == ".png" && !StartsWith(content, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            throw new DomainException("invalid_file_signature", "PNG signature is invalid.");
        }

        if ((extension is ".jpg" or ".jpeg") && !StartsWith(content, [0xFF, 0xD8, 0xFF]))
        {
            throw new DomainException("invalid_file_signature", "JPEG signature is invalid.");
        }

        if (extension == ".gif" &&
            !StartsWith(content, "GIF87a"u8) &&
            !StartsWith(content, "GIF89a"u8))
        {
            throw new DomainException("invalid_file_signature", "GIF signature is invalid.");
        }

        if (extension == ".rar" &&
            !StartsWith(content, [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00]) &&
            !StartsWith(content, [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00]))
        {
            throw new DomainException("invalid_file_signature", "RAR signature is invalid.");
        }

        if (extension == ".7z" && !StartsWith(content, [0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C]))
        {
            throw new DomainException("invalid_file_signature", "7Z signature is invalid.");
        }
    }

    public static async Task<Plan> LoadReadablePlanAsync(
        IApplicationDbContext db,
        Guid planId,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var plan = await db.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == planId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        if (plan.Scope == PlanScope.Sub && !PlanSupport.CanReadSubPlan(plan, currentUser))
        {
            throw new KeyNotFoundException("Plan not found.");
        }

        return plan;
    }

    public static async Task<Plan> LoadMutablePlanAsync(
        IApplicationDbContext db,
        Guid planId,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var plan = await db.Plans
            .FirstOrDefaultAsync(x => x.Id == planId, ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        EnsureCanMutatePlanAttachment(plan, currentUser);
        return plan;
    }

    public static async Task<TaskEntity> LoadReadableTaskAsync(
        IApplicationDbContext db,
        Guid taskId,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var task = await db.Tasks
            .AsNoTracking()
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.Id == taskId, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        TaskSupport.EnsureCanReadPlanTasks(task.Plan!, currentUser);
        return task;
    }

    public static async Task<TaskEntity> LoadMutableTaskAsync(
        IApplicationDbContext db,
        Guid taskId,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var task = await db.Tasks
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.Id == taskId, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        EnsureCanMutateTaskAttachment(task.Plan!, currentUser);
        return task;
    }

    public static async Task<Attachment> LoadReadableAttachmentAsync(
        IApplicationDbContext db,
        Guid attachmentId,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var attachment = await db.Attachments
            .AsNoTracking()
            .Include(x => x.UploadedByUser)
            .FirstOrDefaultAsync(x => x.Id == attachmentId, ct)
            ?? throw new KeyNotFoundException("Attachment not found.");

        await EnsureCanReadOwnerAsync(db, attachment.OwnerType, attachment.OwnerId, currentUser, ct);
        return attachment;
    }

    public static async Task<Attachment> LoadMutableAttachmentAsync(
        IApplicationDbContext db,
        Guid attachmentId,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var attachment = await db.Attachments
            .Include(x => x.UploadedByUser)
            .FirstOrDefaultAsync(x => x.Id == attachmentId, ct)
            ?? throw new KeyNotFoundException("Attachment not found.");

        await EnsureCanMutateOwnerAsync(db, attachment.OwnerType, attachment.OwnerId, currentUser, ct);
        return attachment;
    }

    public static string ResolveContentType(string extension, string? providedContentType) =>
        string.IsNullOrWhiteSpace(providedContentType)
            ? extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                ".7z" => "application/x-7z-compressed",
                _ => "application/octet-stream"
            }
            : providedContentType;

    public static void EnsureCanMutatePlanAttachment(Plan plan, ICurrentUser currentUser)
    {
        PlanSupport.EnsureEditable(plan);

        if (plan.Scope == PlanScope.Main)
        {
            if (PlanSupport.HasRole(currentUser, PlanSupport.RoleVanThu) ||
                PlanSupport.HasRole(currentUser, PlanSupport.RoleAdmin))
            {
                return;
            }

            throw new ForbiddenException("forbidden_role", "Current role cannot mutate main plan attachments.");
        }

        if (!plan.DepartmentId.HasValue)
        {
            throw new DomainException("plan_department_missing", "Sub plan department is missing.");
        }

        if (!PlanSupport.HasRole(currentUser, PlanSupport.RolePhoTruongKtnb) &&
            !PlanSupport.HasRole(currentUser, PlanSupport.RoleTruongPhong))
        {
            throw new ForbiddenException("forbidden_role", "Current role cannot mutate sub plan attachments.");
        }

        PlanSupport.EnsureCanMutateSubDepartment(currentUser, plan.DepartmentId.Value);
    }

    public static void EnsureCanMutateTaskAttachment(Plan plan, ICurrentUser currentUser)
    {
        PlanSupport.EnsureEditable(plan);

        if (TaskSupport.CanFullUpdate(plan, currentUser))
        {
            return;
        }

        if (plan.Scope == PlanScope.Sub &&
            PlanSupport.HasRole(currentUser, PlanSupport.RoleNhanVien) &&
            currentUser.DepartmentId == plan.DepartmentId)
        {
            return;
        }

        throw new ForbiddenException("forbidden_role", "Current role cannot mutate task attachments.");
    }

    private static async System.Threading.Tasks.Task EnsureCanReadOwnerAsync(
        IApplicationDbContext db,
        string ownerType,
        Guid ownerId,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        switch (ownerType)
        {
            case OwnerTypePlan:
                await LoadReadablePlanAsync(db, ownerId, currentUser, ct);
                return;
            case OwnerTypeTask:
                await LoadReadableTaskAsync(db, ownerId, currentUser, ct);
                return;
            default:
                throw new DomainException("attachment_owner_invalid", "Attachment owner type is invalid.");
        }
    }

    private static async System.Threading.Tasks.Task EnsureCanMutateOwnerAsync(
        IApplicationDbContext db,
        string ownerType,
        Guid ownerId,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        switch (ownerType)
        {
            case OwnerTypePlan:
                await LoadMutablePlanAsync(db, ownerId, currentUser, ct);
                return;
            case OwnerTypeTask:
                await LoadMutableTaskAsync(db, ownerId, currentUser, ct);
                return;
            default:
                throw new DomainException("attachment_owner_invalid", "Attachment owner type is invalid.");
        }
    }

    private static bool StartsWith(byte[] content, ReadOnlySpan<byte> signature) =>
        content.AsSpan().StartsWith(signature);
}
