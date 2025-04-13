using StellarPath.API.Core.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StellarPath.API.Core.DTOs;

namespace StelarPath.API.Infrastructure.Services;

public class UserProvider : IUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;

    public UserProvider(
        IHttpContextAccessor httpContextAccessor,
        IUserService userService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }

    public async Task<UserDto> GetCurrentUserAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("No active request");

        var googleId = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Missing identity claim");

        return await _userService.GetUserByGoogleIdAsync(googleId.Value)
            ?? throw new UnauthorizedAccessException("User not found");
    }

    public string GetCurrentUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new KeyNotFoundException("No active request");

        var googleId = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new KeyNotFoundException("Missing identity claim");

        return googleId.Value;
    }
}