using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.PersonalEvaluations.Commands;

public record CreatePersonalEvaluationItemCommand(Guid PeriodId) : IRequest<PersonalEvaluationItemDto>;

public class CreatePersonalEvaluationItemValidator : AbstractValidator<CreatePersonalEvaluationItemCommand>
{
    public CreatePersonalEvaluationItemValidator()
    {
        RuleFor(x => x.PeriodId).NotEmpty();
    }
}

public class CreatePersonalEvaluationItemHandler : IRequestHandler<CreatePersonalEvaluationItemCommand, PersonalEvaluationItemDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreatePersonalEvaluationItemHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async System.Threading.Tasks.Task<PersonalEvaluationItemDto> Handle(CreatePersonalEvaluationItemCommand request, CancellationToken ct)
    {
        var period = await _db.PersonalEvaluationPeriods
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.PeriodId, ct)
            ?? throw new KeyNotFoundException("period not found");

        var user = period.User ?? throw new InvalidOperationException("user missing");
        if (!PersonalEvaluationSupport.CanCreateOrDeleteItem(_currentUser, user))
            throw new ForbiddenException(PersonalEvaluationSupport.ForbiddenRole, "Không có quyền tạo dòng đánh giá.");

        var nextOrder = await _db.PersonalEvaluationItems
            .Where(x => x.PeriodId == period.Id)
            .Select(x => (int?)x.DisplayOrder)
            .MaxAsync(ct) ?? -1;

        var item = new PersonalEvaluationItem
        {
            PeriodId = period.Id,
            DisplayOrder = nextOrder + 1
        };
        _db.PersonalEvaluationItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return PersonalEvaluationSupport.ToDto(item);
    }
}
