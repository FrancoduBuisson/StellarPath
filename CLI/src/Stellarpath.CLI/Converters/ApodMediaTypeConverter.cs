using Stellarpath.CLI.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ApodMediaTypeConverter : JsonConverter<ApodMediaType>
{
  public override ApodMediaType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var value = reader.GetString();
    return new ApodMediaType(value ?? string.Empty);
  }

  public override void Write(Utf8JsonWriter writer, ApodMediaType value, JsonSerializerOptions options)
  {
    writer.WriteStringValue(value.Value);
  }
}