using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellarpath.CLI.Models
{
  public class NasaApodResponse
  {
    public string Title { get; set; } = "";
    public string Explanation { get; set; } = "";
    public string Url { get; set; } = "";
    public string HdUrl { get; set; } = "";
    public string MediaType { get; set; } = "";
  }
}
