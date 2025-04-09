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
            Console.WriteLine("StellarPath CLI Application running...");
        }
    }
}
