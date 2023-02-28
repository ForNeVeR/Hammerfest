using System.Collections.Immutable;
using Hammerfest.Server.Env;
using Microsoft.Extensions.Options;

namespace Hammerfest.Server.Dns;

public record DnsService
    (ILogger<DnsService> Logger, IOptions<DnsOptions> Options, ISystemEnvironment Environment) : IHostedService
{
    public static ImmutableArray<string> Domains = ImmutableArray.Create(
        "servserv.generals.ea.com",
        "cc3xp1.available.gamespy.com"
    );

    private Dictionary<string, string?> _modifications = new();

    private const string? ServerIp = "127.0.0.1";

    private readonly object _stateLock = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Options.Value.Enabled) return Task.CompletedTask;

        var entries = Domains.ToDictionary(domain => domain, ip => ServerIp);
        lock (_stateLock)
        {
            _modifications = HostsFile.ModifyEntries(Environment, entries);
        }

        Logger.LogInformation("Successfully overwritten original DNS entries with the server entries.");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        lock (_stateLock)
        {
            _modifications = HostsFile.ModifyEntries(Environment, _modifications);
        }

        Logger.LogInformation("Successfully restored overwritten DNS entries with the original entries.");

        return Task.CompletedTask;
    }
}
