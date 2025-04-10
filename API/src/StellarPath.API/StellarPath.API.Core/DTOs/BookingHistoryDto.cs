namespace StellarPath.API.Core.DTOs
{
    public class BookingHistoryDto
    {
        public int BookingHistoryId { get; set; }
        public DateTime ChangedAt { get; set; }
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
    }
}
