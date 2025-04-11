
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using StelarPath.API.Infrastructure.Data;
using StelarPath.API.Infrastructure.Data.Repositories;
using StelarPath.API.Infrastructure.Services;
using StellarPath.API.Core.Configuration;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;

namespace API.Configuration;
public static class DependencyInjection
{
    public static IServiceCollection RegisterApplicationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();

        services.Configure<GoogleAuthSettings>(configuration.GetSection("GoogleAuth"));
        services.Configure<JWTSettings>(configuration.GetSection("Jwt"));

        services.AddSingleton<IGoogleAuthService, GoogleAuthService>();
        services.AddSingleton<IJwtService, JwtService>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("Jwt").Get<JWTSettings>();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };

            // this is done on each req... (basically extract our google id token and reverify it :<))
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
                    var googleAuthService = context.HttpContext.RequestServices.GetRequiredService<IGoogleAuthService>();

                    var googleToken = context.Principal?.Claims
                        .FirstOrDefault(c => c.Type == "google_token")?.Value;

                    if (string.IsNullOrEmpty(googleToken))
                    {
                        context.Fail("Missing Google token");
                        return;
                    }

                    var payload = await googleAuthService.VerifyGoogleTokenAsync(googleToken);
                    if (payload == null)
                    {
                        context.Fail("Invalid Google token");
                        return;
                    }
                }
            };
        });

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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStarSystemRepository, StarSystemRepository>();
        services.AddScoped<IDestinationRepository, DestinationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IShipModelRepository, ShipModelRepository>();
        services.AddScoped<ISpaceshipRepository, SpaceshipRepository>();
        services.AddScoped<ICruiseRepository, CruiseRepository>();
        services.AddScoped<ICruiseStatusRepository, CruiseStatusRepository>();
    }

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IGalaxyService, GalaxyService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStarSystemService, StarSystemService>();
        services.AddScoped<IDestinationService, DestinationService>();
        services.AddScoped<IShipModelService, ShipModelService>();
        services.AddScoped<ISpaceshipService, SpaceshipService>();
        services.AddScoped<ICruiseService, CruiseService>();
        services.AddScoped<ICruiseStatusService, CruiseStatusService>();
    }

}

