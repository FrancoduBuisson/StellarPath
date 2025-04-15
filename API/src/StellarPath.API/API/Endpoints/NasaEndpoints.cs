using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StellarPath.API.Core.Configuration;
using StellarPath.API.Core.Interfaces.Services;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace API.Endpoints;

public static class NasaEndpoints
{
  public static void RegisterNasaEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet("/api/nasa/apod", async (
        INasaApodService nasaService,
        IOptions<NasaSettings> nasaSettings
    ) =>
    {
      var apiKey = nasaSettings.Value.ApiKey;

      if (string.IsNullOrWhiteSpace(apiKey))
      {
        return Results.BadRequest("API key is required.");
      }

      var apod = await nasaService.GetPictureOfTheDayAsync(apiKey);
      return apod != null ? Results.Ok(apod) : Results.NotFound("No APOD data returned.");
    })
    .WithTags("NASA");
  }
}
