namespace StellarPath.API.Core.DTOs;
public class CruiseDto
{
    public int CruiseId { get; set; }
    public int SpaceshipId { get; set; }
    public string? SpaceshipName { get; set; }
    public int? Capacity { get; set; }
    public int? CruiseSpeedKmph { get; set; }
    public int DepartureDestinationId { get; set; }
    public string? DepartureDestinationName { get; set; }
    public int ArrivalDestinationId { get; set; }
    public string? ArrivalDestinationName { get; set; }
    public DateTime LocalDepartureTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal CruiseSeatPrice { get; set; }
    public int CruiseStatusId { get; set; }
    public string? CruiseStatusName { get; set; }
    public string CreatedByGoogleId { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public DateTime? EstimatedArrivalTime => LocalDepartureTime.AddMinutes(DurationMinutes);
}