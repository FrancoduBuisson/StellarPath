using System.Text.Json.Serialization;

namespace StellarPath.API.Core.Models
{
  public class NasaApodResponse
  {
    public string Title { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("hdurl")]
    public string HdUrl { get; set; } = string.Empty;

    [JsonPropertyName("media_type")]
    public string? MediaType { get; set; }
  }
}
