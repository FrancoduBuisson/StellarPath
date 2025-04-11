namespace Stellarpath.CLI.Models;

public class StarSystem
{
    public int SystemId { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public int GalaxyId { get; set; }
    public string? GalaxyName { get; set; }
    public bool IsActive { get; set; }
}
