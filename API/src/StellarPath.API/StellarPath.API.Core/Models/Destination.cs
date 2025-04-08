namespace StellarPath.API.Core.Models
{
    public class Destination
    {
        public int DestinationId { get; set; }
        public required string Name { get; set; }
        public int SystemId { get; set; }
        public required long DistanceFromEarth { get; set; }
        public required bool IsActive { get; set; }

    }
}
