using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellarpath.CLI.Models
{
  public readonly struct ApodMediaType
  {
    public string Value { get; }

    public ApodMediaType(string value)
    {
      Value = value?.ToLowerInvariant() ?? string.Empty;
    }

    public bool IsImage => Value == "image";
    public bool IsVideo => Value == "video";

    public override string ToString() => Value;
  }
}
