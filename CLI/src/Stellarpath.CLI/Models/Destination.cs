namespace Stellarpath.CLI.Models;

public class Destination
{
    public int DestinationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SystemId { get; set; }
    public string? SystemName { get; set; }
    public long DistanceFromEarth { get; set; }
    public bool IsActive { get; set; }
}