namespace Hammerfest.Server.ServServ;

public class ServServStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting().UseEndpoints(e => e.MapControllers());
    }
}
