namespace StellarPath.API.Core.Models
{
    public class ShipModel
    {
        public int ModelId { get; set; }
        public required string ModelName { get; set; }
        public required int Capacity { get; set; }
        public required int CruiseSpeedKmph { get; set; }
    }
}
