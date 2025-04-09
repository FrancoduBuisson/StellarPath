public class CreateCruiseDto
{
    public int SpaceshipId { get; set; }
    public int DepartureDestinationId { get; set; }
    public int ArrivalDestinationId { get; set; }
    public DateTime LocalDepartureTime { get; set; }
    public decimal CruiseSeatPrice { get; set; }
}
