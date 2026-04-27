using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                          ?? User?.FindFirst(ClaimTypes.Email)?.Value;

    public UserRole? Role
    {
        get
        {
            var role = User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(role, out var r) ? r : null;
        }
    }
}
