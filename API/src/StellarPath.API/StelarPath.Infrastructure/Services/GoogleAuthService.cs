using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using StellarPath.API.Core.Configuration;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Services;

namespace StelarPath.API.Infrastructure.Services;
public class GoogleAuthService(IOptions<GoogleAuthSettings> options, HttpClient httpClient) : IGoogleAuthService
{
    public async Task<GoogleUserInfoDto> GetGoogleUserInfoAsync(string accessToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<GoogleUserInfoDto>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            return null;
        }
    }

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

