using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CLI.Services
{
    internal interface IApplication
    {
        void Run();
    }

    internal class Application : IApplication
    {
        private readonly ILogger _logger;
        public Application(ILogger<Application> logger)
        {
            _logger = logger;
            
        }
        public void Run()
        {
            _logger.LogInformation("Logging from Application");
            Console.WriteLine("StellarPath CLI Application running...");
        }
    }
}
