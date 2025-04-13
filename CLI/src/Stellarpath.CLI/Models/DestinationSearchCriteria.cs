namespace Stellarpath.CLI.Models;

public class DestinationSearchCriteria
{
    public string? Name { get; set; }
    public int? SystemId { get; set; }
    public string? SystemName { get; set; }
    public long? MinDistance { get; set; }
    public long? MaxDistance { get; set; }
    public bool? IsActive { get; set; }
}