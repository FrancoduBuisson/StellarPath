namespace StellarPath.API.Core.Models;
public class StarSystem
{
    public int SystemId { get; set; }
    public required string SystemName { get; set; }
    public int GalaxyId { get; set; }
    public bool IsActive { get; set; }
}

