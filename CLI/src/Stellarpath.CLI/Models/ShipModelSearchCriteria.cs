namespace Stellarpath.CLI.Models;

public class ShipModelSearchCriteria
{
    public string? Name { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public int? MinSpeed { get; set; }
    public int? MaxSpeed { get; set; }
}