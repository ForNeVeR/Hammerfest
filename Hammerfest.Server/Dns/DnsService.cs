using System.Collections.Immutable;
using Hammerfest.Server.Env;
using Microsoft.Extensions.Options;

namespace Hammerfest.Server.Dns;

public record DnsService(ILogger<DnsService> Logger, IOptions<DnsOptions> Options, ISystemEnvironment Environment) : IHostedService
{
    public static ImmutableArray<string> Domains = ImmutableArray.Create(
        "servserv.generals.ea.com",
        "cc3xp1.available.gamespy.com"
    );

    private const string ServerIp = "127.0.0.1";

    private readonly object _stateLock = new();
    private string? _previousIpAddress;
    private bool _hostsFileChanged;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Options.Value.Enabled) return Task.CompletedTask;

        foreach (var domain in Domains)
        {
            var entry = HostsFile.FindIpAddress(Environment, domain);
            if (entry != ServerIp)
            {
                if (entry != null)
                    HostsFile.ReplaceEntry(Environment, ServerIp, domain);
                else
                    HostsFile.AddEntry(Environment, ServerIp, domain);

                Logger.LogInformation(
                    """Temporarily wrote entry for host "{Host}" in file "{HostFile}" with address "{IpAddress}".""",
                    domain, Environment.HostsFilePath, ServerIp);

                lock (_stateLock)
                {
                    _previousIpAddress = entry;
                    _hostsFileChanged = true;
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        string? previousIpAddress;
        lock (_stateLock)
        {
            if (!_hostsFileChanged) return Task.CompletedTask;
            previousIpAddress = _previousIpAddress;
        }

        var status = previousIpAddress != null
            ? HostsFile.ReplaceEntry(Environment, ServerIp, previousIpAddress)
            : true; //HostsFile.RemoveEntry(Environment, ServServ);
            // TODO: Proper multi-rollback
        if (!status)
        {
            Logger.LogWarning(
                """Cannot restore the file "{HostsFile}": previous IP address "{IpAddress}", host "{Host}".""",
                Environment.HostsFilePath, previousIpAddress, ServerIp);
        }

        return Task.CompletedTask;
    }
}
