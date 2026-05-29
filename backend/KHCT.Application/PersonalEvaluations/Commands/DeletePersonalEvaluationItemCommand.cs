using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.PersonalEvaluations.Commands;

public record DeletePersonalEvaluationItemCommand(Guid Id) : IRequest<bool>;

public class DeletePersonalEvaluationItemHandler : IRequestHandler<DeletePersonalEvaluationItemCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeletePersonalEvaluationItemHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async System.Threading.Tasks.Task<bool> Handle(DeletePersonalEvaluationItemCommand request, CancellationToken ct)
    {
        var item = await _db.PersonalEvaluationItems
            .Include(x => x.Period)
                .ThenInclude(p => p!.User)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("item not found");

        var user = item.Period?.User ?? throw new InvalidOperationException();
        if (!PersonalEvaluationSupport.CanCreateOrDeleteItem(_currentUser, user))
            throw new ForbiddenException(PersonalEvaluationSupport.ForbiddenRole, "Không có quyền xóa dòng đánh giá.");

        _db.PersonalEvaluationItems.Remove(item);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
