namespace StellarPath.API.Core.DTOs;

public class BookingHistoryDto
{
    public int HistoryId { get; set; }
    public int BookingId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}