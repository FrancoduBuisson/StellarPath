using System.Text.Json.Serialization;

namespace Stellarpath.CLI.Models;

public class DeactivationResult
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("cancelledCruises")]
    public int CancelledCruises { get; set; }
}