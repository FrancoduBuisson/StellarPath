namespace StellarPath.API.Core.DTOs;
public class SpaceshipDto
{
    public int SpaceshipId { get; set; }
    public int ModelId { get; set; }
    public string? ModelName { get; set; }
    public int? Capacity { get; set; }
    public int? CruiseSpeedKmph { get; set; }
    public bool IsActive { get; set; }
}