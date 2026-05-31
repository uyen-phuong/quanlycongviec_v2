using FluentValidation;
using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Positions;

public record UpdatePositionCommand(Guid Id, string Name, bool IsActive, int SortOrder) : IRequest<PositionDto>;

public class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}

public class UpdatePositionHandler : IRequestHandler<UpdatePositionCommand, PositionDto>
{
    private readonly IApplicationDbContext _db;

    public UpdatePositionHandler(IApplicationDbContext db) => _db = db;

    public async Task<PositionDto> Handle(UpdatePositionCommand request, CancellationToken ct)
    {
        var position = await _db.Positions.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Position not found.");

        position.Name = request.Name.Trim();
        position.IsActive = request.IsActive;
        position.SortOrder = request.SortOrder;
        await _db.SaveChangesAsync(ct);
        return AdminSupport.ToDto(position);
    }
}
