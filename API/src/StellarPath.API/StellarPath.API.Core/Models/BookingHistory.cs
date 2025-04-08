namespace StellarPath.API.Core.Models
{
    public class BookingHistory
    {
        public int HistoryId { get; set; }
        public int BookingId { get; set; }
        public int PreviousBookingStatusId { get; set; }
        public int NewBookingStatusId { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
