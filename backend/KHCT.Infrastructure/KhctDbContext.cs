using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Infrastructure.Audit;
using KHCT.Infrastructure.Persistence;
using KHCT.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KHCT.Domain.Entities.Task;

namespace KHCT.Infrastructure;

public sealed class KhctDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUser? _currentUser;

    public KhctDbContext(DbContextOptions<KhctDbContext> options, ICurrentUser? currentUser = null) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ApprovalConfig> ApprovalConfigs => Set<ApprovalConfig>();
    public DbSet<BksMember> BksMembers => Set<BksMember>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public DbSet<TaskSupportingDept> TaskSupportingDepts => Set<TaskSupportingDept>();
    public DbSet<TaskApprovalHistory> TaskApprovalHistories => Set<TaskApprovalHistory>();
    public DbSet<ApprovalHistory> ApprovalHistories => Set<ApprovalHistory>();
    public DbSet<LineComment> LineComments => Set<LineComment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PersonalEvaluationPeriod> PersonalEvaluationPeriods => Set<PersonalEvaluationPeriod>();
    public DbSet<PersonalEvaluationItem> PersonalEvaluationItems => Set<PersonalEvaluationItem>();
    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade IApplicationDbContext.Database => base.Database;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KhctDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entity.SetTableName(SnakeCaseHelper.ToSnakeCase(tableName));
            }

            foreach (var prop in entity.GetProperties())
            {
                var columnName = prop.GetColumnName();
                prop.SetColumnName(SnakeCaseHelper.ToSnakeCase(columnName));
            }

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName))
                    key.SetName(SnakeCaseHelper.ToSnakeCase(keyName));
            }

            foreach (var fk in entity.GetForeignKeys())
            {
                var fkName = fk.GetConstraintName();
                if (!string.IsNullOrEmpty(fkName))
                    fk.SetConstraintName(SnakeCaseHelper.ToSnakeCase(fkName));
            }

            foreach (var idx in entity.GetIndexes())
            {
                var idxName = idx.GetDatabaseName();
                if (!string.IsNullOrEmpty(idxName))
                    idx.SetDatabaseName(SnakeCaseHelper.ToSnakeCase(idxName));
            }
        }

        ApplyEnumStringConversions(modelBuilder);

        SeedData.Apply(modelBuilder);
    }

    private static void ApplyEnumStringConversions(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var prop in entity.GetProperties())
            {
                var type = prop.ClrType;
                var underlying = Nullable.GetUnderlyingType(type) ?? type;
                if (!underlying.IsEnum) continue;
                if (underlying == typeof(Domain.Enums.WorkType)) continue;

                var converterType = typeof(Microsoft.EntityFrameworkCore.Storage.ValueConversion.EnumToStringConverter<>)
                    .MakeGenericType(underlying);
                var converter = (Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter)
                    Activator.CreateInstance(converterType)!;
                prop.SetValueConverter(converter);
                prop.SetMaxLength(32);
            }
        }
    }

    public override System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var autoAuditLogs = PrepareAutomaticAuditLogs();
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
        foreach (var entry in ChangeTracker.Entries<UserRole>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
        }
        foreach (var entry in ChangeTracker.Entries<TaskSupportingDept>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
        }
        if (autoAuditLogs.Count != 0)
        {
            AddRange(autoAuditLogs);
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    private List<AuditLog> PrepareAutomaticAuditLogs()
    {
        if (ChangeTracker.Entries<AuditLog>().Any(x => x.State == EntityState.Added))
        {
            return [];
        }

        var logs = new List<AuditLog>();
        foreach (var entry in ChangeTracker.Entries().Where(AutomaticAuditSupport.ShouldAutoAudit))
        {
            var entity = (Entity)entry.Entity;
            var before = AutomaticAuditSupport.BuildBeforeSnapshot(entry);
            var after = AutomaticAuditSupport.BuildAfterSnapshot(entry);
            if (!AutomaticAuditSupport.HasMeaningfulDifference(before, after))
            {
                continue;
            }

            logs.Add(new AuditLog
            {
                EntityName = AutomaticAuditSupport.EntityName(entity),
                EntityId = entity.Id,
                Action = AutomaticAuditSupport.ActionName(entry.State),
                ActorUserId = _currentUser?.UserId,
                BeforeJson = before is null ? null : System.Text.Json.JsonSerializer.Serialize(before),
                AfterJson = after is null ? null : System.Text.Json.JsonSerializer.Serialize(after)
            });
        }

        return logs;
    }
}
