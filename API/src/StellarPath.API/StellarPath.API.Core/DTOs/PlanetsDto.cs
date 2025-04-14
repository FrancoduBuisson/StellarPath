using Newtonsoft.Json;

namespace StellarPath.API.Core.DTOs;


public class PlanetDto
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("mass")]
    public double Mass { get; set; }

    [JsonProperty("radius")]
    public double Radius { get; set; }

    [JsonProperty("period")]
    public double Period { get; set; }

    [JsonProperty("semi_major_axis")]
    public double SemiMajorAxis { get; set; }

    [JsonProperty("temperature")]
    public double Temperature { get; set; }

    [JsonProperty("distance_light_year")]
    public double DistanceLightYear { get; set; }

    [JsonProperty("host_star_mass")]
    public double HostStarMass { get; set; }

    [JsonProperty("host_star_temperature")]
    public double HostStarTemperature { get; set; }
}

