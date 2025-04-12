namespace Stellarpath.CLI.Models;

public class ShipModel
{
    public int ModelId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CruiseSpeedKmph { get; set; }
}