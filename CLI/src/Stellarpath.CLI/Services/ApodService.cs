using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Stellarpath.CLI.Models;
using Stellarpath.CLI.Core;
using Spectre.Console.Cli;
using CommandContext = Stellarpath.CLI.Core.CommandContext;

namespace Stellarpath.CLI.Services
{
  public class ApodService : ApiServiceBase<NasaApodResponse>
  {
    public ApodService(CommandContext context)
        : base(context, "/api/nasa/apod")
    {
    }
  }
}
