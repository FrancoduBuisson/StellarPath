using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using StellarPath.API.Core.DTOs;
using Microsoft.Extensions.Configuration;

namespace API.Endpoints;

public static class PlanetEndpoints
{
    public static WebApplication RegisterPlanetEndpoints(this WebApplication app)
    {
        var planetGroup = app.MapGroup("/api/planets")
            .WithTags("Planets");

        planetGroup.MapGet("/details", async (
            [FromQuery] string name,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration) =>
        {
            return await GetPlanetDetails(name, httpClientFactory, configuration);
        })
        .WithName("GetPlanetDetails")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> GetPlanetDetails(
        string name,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Results.BadRequest("Planet name is required");
        }

        try
        {
            var apiKey = configuration["PlanetsAPI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return Results.Problem("API key is not configured", statusCode: 500);
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
}