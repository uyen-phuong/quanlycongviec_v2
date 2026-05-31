using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Department> Departments { get; }
    DbSet<Position> Positions { get; }
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ApprovalConfig> ApprovalConfigs { get; }
    DbSet<BksMember> BksMembers { get; }
    DbSet<Plan> Plans { get; }
    DbSet<PlanReportingPeriod> PlanReportingPeriods { get; }
    DbSet<TaskEntity> Tasks { get; }
    DbSet<TaskSupportingDept> TaskSupportingDepts { get; }
    DbSet<TaskCollaborator> TaskCollaborators { get; }
    DbSet<TaskApprovalHistory> TaskApprovalHistories { get; }
    DbSet<ApprovalHistory> ApprovalHistories { get; }
    DbSet<LineComment> LineComments { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<PersonalEvaluationPeriod> PersonalEvaluationPeriods { get; }
    DbSet<PersonalEvaluationItem> PersonalEvaluationItems { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DatabaseFacade Database { get; }

    System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
