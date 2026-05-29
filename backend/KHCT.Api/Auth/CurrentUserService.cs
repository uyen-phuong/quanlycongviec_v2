using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KHCT.Application.Common.Interfaces;

namespace KHCT.Api.Auth;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Username => Principal?.FindFirst("username")?.Value;

    public Guid? DepartmentId
    {
        get
        {
            var d = Principal?.FindFirst("dept")?.Value;
            return Guid.TryParse(d, out var id) ? id : null;
        }
    }

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
}
