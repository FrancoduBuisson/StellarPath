using Google.Apis.Auth;
using StellarPath.API.Core.DTOs;

namespace StellarPath.API.Core.Interfaces.Services;
public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken);
    Task<GoogleUserInfoDto> GetGoogleUserInfoAsync(string idToken);
}

