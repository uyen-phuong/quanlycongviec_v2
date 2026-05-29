using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Plans;

public record ResolvePlanQuery(string Scope, string? DepartmentCode, int? Year, int? Month) : IRequest<ResolvedPlanDto>;

public record ResolvedPlanDto(
    bool Found,
    Guid? PlanId,
    string Scope,
    int Year,
    int Month,
    Guid? DepartmentId,
    string? DepartmentCode,
    string? DepartmentName,
    string? Status,
    DateTime? CreatedAt,
    DateTime? UpdatedAt);

public sealed class ResolvePlanQueryHandler : IRequestHandler<ResolvePlanQuery, ResolvedPlanDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ResolvePlanQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ResolvedPlanDto> Handle(ResolvePlanQuery request, CancellationToken ct)
    {
        var scope = ParseScope(request.Scope);
        var (year, month) = ResolvePeriod(request.Year, request.Month);

        if (scope == PlanScope.Main)
        {
            var resolvedMainPlan = await _db.Plans
                .AsNoTracking()
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x =>
                    x.Scope == PlanScope.Main &&
                    x.Year == year &&
                    x.Month == month, ct);

            if (resolvedMainPlan is null)
            {
                return new ResolvedPlanDto(false, null, PlanSupport.ScopeCode(scope), year, month, null, null, null, null, null, null);
            }

            return new ResolvedPlanDto(
                true,
                resolvedMainPlan.Id,
                PlanSupport.ScopeCode(resolvedMainPlan.Scope),
                resolvedMainPlan.Year,
                resolvedMainPlan.Month,
                resolvedMainPlan.DepartmentId,
                resolvedMainPlan.Department?.Code,
                resolvedMainPlan.Department?.Name,
                PlanSupport.StatusCode(resolvedMainPlan.Status),
                resolvedMainPlan.CreatedAt,
                resolvedMainPlan.UpdatedAt);
        }

        var targetDepartment = await ResolveDepartmentAsync(request.DepartmentCode, ct);
        if (targetDepartment is null)
        {
            return new ResolvedPlanDto(false, null, PlanSupport.ScopeCode(scope), year, month, null, request.DepartmentCode?.Trim().ToUpperInvariant(), null, null, null, null);
        }

        var subPlan = await _db.Plans
            .AsNoTracking()
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x =>
                x.Scope == PlanScope.Sub &&
                x.DepartmentId == targetDepartment.Id &&
                x.Year == year &&
                x.Month == month, ct);

        if (subPlan is null)
        {
            return new ResolvedPlanDto(
                false,
                null,
                PlanSupport.ScopeCode(scope),
                year,
                month,
                targetDepartment.Id,
                targetDepartment.Code,
                targetDepartment.Name,
                null,
                null,
                null);
        }

        return new ResolvedPlanDto(
            true,
            subPlan.Id,
            PlanSupport.ScopeCode(subPlan.Scope),
            subPlan.Year,
            subPlan.Month,
            subPlan.DepartmentId,
            subPlan.Department?.Code,
            subPlan.Department?.Name,
            PlanSupport.StatusCode(subPlan.Status),
            subPlan.CreatedAt,
            subPlan.UpdatedAt);
    }

    private async Task<DepartmentLookup?> ResolveDepartmentAsync(string? departmentCode, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(departmentCode))
        {
            var normalizedCode = departmentCode.Trim().ToUpperInvariant();
            return await _db.Departments
                .AsNoTracking()
                .Where(x => x.Code == normalizedCode)
                .Select(x => new DepartmentLookup(x.Id, x.Code, x.Name))
                .FirstOrDefaultAsync(ct);
        }

        if (!_currentUser.DepartmentId.HasValue)
        {
            return null;
        }

        return await _db.Departments
            .AsNoTracking()
            .Where(x => x.Id == _currentUser.DepartmentId.Value)
            .Select(x => new DepartmentLookup(x.Id, x.Code, x.Name))
            .FirstOrDefaultAsync(ct);
    }

    private static PlanScope ParseScope(string scope) =>
        scope.Trim().ToLowerInvariant() switch
        {
            "main" => PlanScope.Main,
            "sub" => PlanScope.Sub,
            _ => throw new FluentValidation.ValidationException("Invalid scope.")
        };

    private static (int Year, int Month) ResolvePeriod(int? year, int? month)
    {
        var now = GetVietnamNow();
        var resolvedYear = year ?? now.Year;
        var resolvedMonth = month ?? now.Month;

        if (resolvedMonth is < 1 or > 12)
        {
            throw new FluentValidation.ValidationException("Month must be between 1 and 12.");
        }

        return (resolvedYear, resolvedMonth);
    }

    private static DateTime GetVietnamNow()
    {
        var utcNow = DateTime.UtcNow;
        foreach (var timezoneId in new[] { "SE Asia Standard Time", "Asia/Ho_Chi_Minh", "Asia/Bangkok" })
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.FindSystemTimeZoneById(timezoneId));
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return utcNow.AddHours(7);
    }

    private sealed record DepartmentLookup(Guid Id, string Code, string Name);
}
