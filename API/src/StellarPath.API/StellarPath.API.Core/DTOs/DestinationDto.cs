namespace StellarPath.API.Core.DTOs;
public class DestinationDto
{
    public int DestinationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SystemId { get; set; }
    public string? SystemName { get; set; }
    public long DistanceFromEarth { get; set; }
    public bool IsActive { get; set; }
}