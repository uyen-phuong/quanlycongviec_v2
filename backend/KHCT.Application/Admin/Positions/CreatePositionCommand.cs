using FluentValidation;
using KHCT.Application.Common.Interfaces;
using KHCT.Domain.Common;
using KHCT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Admin.Positions;

public record CreatePositionCommand(string Code, string Name, int SortOrder) : IRequest<PositionDto>;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}

public class CreatePositionHandler : IRequestHandler<CreatePositionCommand, PositionDto>
{
    private readonly IApplicationDbContext _db;

    public CreatePositionHandler(IApplicationDbContext db) => _db = db;

    public async Task<PositionDto> Handle(CreatePositionCommand request, CancellationToken ct)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Positions.AnyAsync(x => x.Code == code, ct))
            throw new DomainException("position_code_taken", "Position code already exists.");

        var position = new Position
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            SortOrder = request.SortOrder,
            IsActive = true
        };

        _db.Positions.Add(position);
        await _db.SaveChangesAsync(ct);
        return AdminSupport.ToDto(position);
    }
}
