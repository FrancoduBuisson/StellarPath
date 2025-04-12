namespace Stellarpath.CLI.Models;

public class CruiseSearchCriteria
{
    public int? SpaceshipId { get; set; }
    public string? SpaceshipName { get; set; }
    public int? DepartureDestinationId { get; set; }
    public int? ArrivalDestinationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}