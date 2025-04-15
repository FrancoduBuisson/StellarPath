using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using StellarPath.API.Core.DTOs;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace API.Endpoints;

public static class PlanetEndpoints
{
    public static WebApplication RegisterPlanetEndpoints(this WebApplication app)
    {
        var planetGroup = app.MapGroup("/api/planets")
            .WithTags("Planets");

        planetGroup.MapGet("/details", GetPlanetDetails)
            .WithName("GetPlanetDetails")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> GetPlanetDetails(
        [FromQuery] string name,
        IHttpClientFactory httpClientFactory)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Results.BadRequest("Planet name is required");
        }

        try
        {
            var apiKey = await GetSecretAsync("stellar-path-api/api-key-2-v2", "af-south-1");
            if (string.IsNullOrEmpty(apiKey))
            {
                return Results.Problem("API key could not be retrieved from AWS Secrets Manager", statusCode: 500);
            }

            var httpClient = httpClientFactory.CreateClient();

            string baseUrl = "https://api.api-ninjas.com/v1/planets";
            string requestUrl = $"{baseUrl}?name={Uri.EscapeDataString(name)}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("X-Api-Key", apiKey);

            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Results.Problem($"Error from external API: {content}", statusCode: (int)response.StatusCode);
            }

            var planets = JsonConvert.DeserializeObject<List<PlanetDto>>(content);

            if (planets == null || planets.Count == 0)
            {
                return Results.NotFound($"No planet found with name: {name}");
            }

            return Results.Ok(planets);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to fetch planet data: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<string> GetSecretAsync(string secretName, string region)
    {
        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT",
        };

        try
        {
            GetSecretValueResponse response = await client.GetSecretValueAsync(request);
            return response.SecretString;
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving secret from AWS Secrets Manager", ex);
        }
    }
}
