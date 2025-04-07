using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using StellarPath.API.Core.Configuration;
using StellarPath.API.Core.Interfaces.Services;

namespace StelarPath.API.Infrastructure.Services;
public class GoogleAuthService(IOptions<GoogleAuthSettings> options) : IGoogleAuthService
{
    public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { options.Value.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return payload;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}

