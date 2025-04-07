namespace StellarPath.API.Core.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public string GoogleId { get; set; } = null!;
        public int CruiseId { get; set; }
        public required int SeatNumber { get; set; }
        public DateTime BookingDate { get; set; } 
        public DateTime BookingExpiration { get; set; }
        public int BookingStatusId { get; set; }
    }
}
