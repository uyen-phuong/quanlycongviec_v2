using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using KHCT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.PersonalEvaluations.Queries;

public record GetPersonalEvaluationQuery(int Year, int Month, Guid? UserId) : IRequest<PersonalEvaluationResponse>;

public class GetPersonalEvaluationHandler : IRequestHandler<GetPersonalEvaluationQuery, PersonalEvaluationResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetPersonalEvaluationHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async System.Threading.Tasks.Task<PersonalEvaluationResponse> Handle(GetPersonalEvaluationQuery request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAccessException();
        if (request.Month < 1 || request.Month > 12)
            throw new ArgumentException("month invalid");

        var targetUserId = request.UserId ?? _currentUser.UserId.Value;
        var targetUser = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == targetUserId, ct)
            ?? throw new KeyNotFoundException("user not found");

        if (!PersonalEvaluationSupport.CanReadPeriodOf(_currentUser, targetUser))
            throw new ForbiddenException(PersonalEvaluationSupport.ForbiddenRole, "Không có quyền xem đánh giá cá nhân này.");

        var period = await _db.PersonalEvaluationPeriods
            .Include(x => x.User)
            .Include(x => x.Department)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.UserId == targetUserId && x.ReportYear == request.Year && x.ReportMonth == request.Month, ct);

        if (period is null)
        {
            period = new PersonalEvaluationPeriod
            {
                UserId = targetUserId,
                DepartmentId = targetUser.DepartmentId ?? throw new InvalidOperationException("user has no department"),
                ReportYear = request.Year,
                ReportMonth = request.Month,
                Status = PersonalEvaluationPeriodStatus.Draft
            };
            _db.PersonalEvaluationPeriods.Add(period);
            await _db.SaveChangesAsync(ct);
            period = await _db.PersonalEvaluationPeriods
                .Include(x => x.User)
                .Include(x => x.Department)
                .Include(x => x.Items)
                .FirstAsync(x => x.Id == period.Id, ct);
        }

        var items = period.Items
            .OrderBy(x => x.DisplayOrder)
            .Select(PersonalEvaluationSupport.ToDto)
            .ToList();

        return new PersonalEvaluationResponse(PersonalEvaluationSupport.ToDto(period), items);
    }
}
