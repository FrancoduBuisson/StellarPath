using Google.Apis.Auth;

namespace StellarPath.API.Core.Interfaces.Services;
public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken);
}

