using Hammerfest.Server.Dns;
using Hammerfest.Server.Env;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
        services
            .AddSingleton<ISystemEnvironment, WindowsEnvironment>()
            .AddHostedService<DnsService>())
    .Build();

host.Run();
