using KHCT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KHCT.Application.Tasks;

public record GetTaskByIdQuery(Guid Id) : IRequest<TaskDetailDto>;

public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, TaskDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTaskByIdHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskDetailDto> Handle(GetTaskByIdQuery request, CancellationToken ct)
    {
        var task = await _db.Tasks
            .AsNoTracking()
            .Include(x => x.Plan)
            .Include(x => x.AssigneeUser)
            .Include(x => x.OwnerDepartment)
            .Include(x => x.SupportingDepts)
                .ThenInclude(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        TaskSupport.EnsureCanReadPlanTasks(task.Plan!, _currentUser);
        return TaskSupport.ToDetail(task);
    }
}
