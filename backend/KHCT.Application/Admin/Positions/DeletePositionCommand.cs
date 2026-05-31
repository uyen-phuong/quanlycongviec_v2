using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Positions;

public record DeletePositionCommand(Guid Id) : IRequest<Unit>;

public class DeletePositionHandler : IRequestHandler<DeletePositionCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public DeletePositionHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(DeletePositionCommand request, CancellationToken ct)
    {
        var position = await _db.Positions.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Position not found.");

        var inUse = await _db.Users.AnyAsync(x => x.PositionId == request.Id, ct);
        if (inUse)
            throw new DomainException("position_in_use", "Cannot delete a position that is assigned to users.");

        _db.Positions.Remove(position);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
