using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Services
{
  public class ApodService
  {
    private readonly HttpClient _httpClient;

    public ApodService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<NasaApodResponse?> GetApodAsync()
    {
      try
      {
        var response = await _httpClient.GetAsync("https://localhost:7029/api/nasa/apod");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<NasaApodResponse>();
      }
      catch (Exception ex)
      {
        AnsiConsole.MarkupLine($"[red]Failed to retrieve APOD: {ex.Message}[/]");
        return null;
      }
    }
  }
}
