using System.Net.Http.Json;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StellarPath.API.Infrastructure.Services
{
  public class NasaApodService : INasaApodService
  {
    private readonly HttpClient _httpClient;

    public NasaApodService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<NasaApodResponse?> GetPictureOfTheDayAsync(string apiKey)
    {
      var url = $"https://api.nasa.gov/planetary/apod?api_key={apiKey}";
      return await _httpClient.GetFromJsonAsync<NasaApodResponse>(url);
    }
  }
}

