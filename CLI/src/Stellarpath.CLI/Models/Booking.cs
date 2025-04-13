namespace Stellarpath.CLI.Models;

public class Booking
{
    public int BookingId { get; set; }
    public string GoogleId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int CruiseId { get; set; }
    public string DepartureDestination { get; set; } = string.Empty;
    public string ArrivalDestination { get; set; } = string.Empty;
    public DateTime LocalDepartureTime { get; set; }
    public int SeatNumber { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime BookingExpiration { get; set; }
    public int BookingStatusId { get; set; }
    public string BookingStatusName { get; set; } = string.Empty;
}