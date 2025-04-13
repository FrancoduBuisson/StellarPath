using StellarPath.API.Core.Models;
using System;

namespace StellarPath.API.Core.DTOs
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public string UserGoogleId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;

        // Cruise Information
        public int CruiseId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime => DepartureTime.AddMinutes(DurationMinutes);
        public int DurationMinutes { get; set; }
        public decimal SeatPrice { get; set; }

        // Ship Information
        public string SpaceshipModel { get; set; } = string.Empty;
        public int SpaceshipCapacity { get; set; }

        // Destination Information
        public string DepartureDestination { get; set; } = string.Empty;
        public string DepartureStarSystem { get; set; } = string.Empty;
        public string DepartureGalaxy { get; set; } = string.Empty;
        public string ArrivalDestination { get; set; } = string.Empty;
        public string ArrivalStarSystem { get; set; } = string.Empty;
        public string ArrivalGalaxy { get; set; } = string.Empty;

        // Booking Details
        public int SeatNumber { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime BookingExpiration { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public int BookingStatusId { get; set; }        
        public bool IsActive => BookingStatus != "Cancelled" && BookingStatus != "Expired";
        public bool IsUpcoming => DepartureTime > DateTime.UtcNow && IsActive;
        public bool IsCompleted => ArrivalTime < DateTime.UtcNow && IsActive;
    }
}