using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Stellarpath.CLI.Models
{
  public class NasaApodResponse
  {
    public string Title { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string HdUrl { get; set; } = string.Empty;

    [JsonPropertyName("media_type")]
    [JsonConverter(typeof(ApodMediaTypeConverter))]
    public ApodMediaType MediaType { get; set; }
  }
}
