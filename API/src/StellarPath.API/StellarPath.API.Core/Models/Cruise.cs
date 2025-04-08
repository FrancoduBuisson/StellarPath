namespace StellarPath.API.Core.Models
{
    public class Cruise
    {
        public int CruiseId { get; set; }
        public int SpaceshipId { get; set; }
        public int DepartureDestinationId { get; set; }
        public int ArrivalDestinationId { get; set; }
        public required DateTime LocalDepartureTime { get; set; }
        public required int DurationMinutes { get; set; }
        public required decimal CruiseSeatPrice { get; set; }
        public int CruiseStatusId { get; set; }
        public string CreatedByGoogleId { get; set; } = null!;

        public Spaceship? Spaceship { get; set; }
        public Destination? DepartureDestination { get; set; }
        public Destination? ArrivalDestination { get; set; }
        public CruiseStatus? CruiseStatus { get; set; }
        public User? CreatedBy { get; set; }
    }
}
