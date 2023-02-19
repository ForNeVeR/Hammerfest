using Hammerfest.Server.Dns;
using Hammerfest.Server.Env;
using Hammerfest.Server.ServServ;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder => builder.UseStartup<ServServStartup>())
    .ConfigureServices((builder, services) =>
    {
        services
            .Configure<DnsOptions>(builder.Configuration.GetSection("Dns"));

        services
            .AddSingleton<ISystemEnvironment, WindowsEnvironment>()
            .AddHostedService<DnsService>();
    })
    .Build();

host.Run();
