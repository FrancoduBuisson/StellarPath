namespace Stellarpath.CLI.Models;

public class SpaceshipAvailability
{
    public int SpaceshipId { get; set; }
    public int ModelId { get; set; }
    public string? ModelName { get; set; }
    public int Capacity { get; set; }
    public int CruiseSpeedKmph { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<TimeSlot> AvailableTimeSlots { get; set; } = new List<TimeSlot>();
}

public class TimeSlot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}