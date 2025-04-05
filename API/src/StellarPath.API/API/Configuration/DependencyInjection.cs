
using StelarPath.API.Infrastructure.Data;
using StelarPath.API.Infrastructure.Data.Repositories;
using StelarPath.API.Infrastructure.Services;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Configuration;
public static class DependencyInjection
{
    public static IServiceCollection RegisterApplicationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionFactory>(provider =>
         new ConnectionFactory(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        RegisterRepositories(services);

        RegisterServices(services);

        return services; 
    }

    public static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IGalaxyRepository, GalaxyRepository>();
    }

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IGalaxyService, GalaxyService>();
    }

}

