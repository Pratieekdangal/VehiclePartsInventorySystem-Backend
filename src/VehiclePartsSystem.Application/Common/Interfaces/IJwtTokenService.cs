using VehiclePartsSystem.Domain.Entities;

namespace VehiclePartsSystem.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
