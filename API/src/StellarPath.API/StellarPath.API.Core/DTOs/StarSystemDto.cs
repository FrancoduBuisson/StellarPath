namespace StellarPath.API.Core.DTOs;
public class StarSystemDto
{
    public int SystemId { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public int GalaxyId { get; set; }
    public string? GalaxyName { get; set; }
    public bool IsActive { get; set; }
}