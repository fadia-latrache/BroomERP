using System.Security.Claims;
using BroomErp.Application.Common.Interfaces;
using BroomErp.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace BroomErp.Api.Common;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User?.FindFirst("sub")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

    public Role? Role
    {
        get
        {
            var roleStr = User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<Role>(roleStr, true, out var r) ? r : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
