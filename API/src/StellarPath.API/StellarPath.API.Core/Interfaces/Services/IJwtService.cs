using System.Security.Claims;
using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;
public interface IJwtService
{
    string GenerateToken(UserDto user, string googleIdToken);
    ClaimsPrincipal? ValidateToken(string token);
    string? ExtractGoogleTokenFromJwt(string token);
}

