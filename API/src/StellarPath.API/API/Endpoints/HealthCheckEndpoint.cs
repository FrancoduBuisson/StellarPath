namespace API.Endpoints
{
    public static class HealthCheckEndpoint
    {
       public static WebApplication RegisterHealthCheckEndpoint(this WebApplication app)
        {
            app.MapGet("/api/health", async context =>
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Healthy");
            });
                return app;
        }
    }
}
