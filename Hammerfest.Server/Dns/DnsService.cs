using Hammerfest.Server.Env;

namespace Hammerfest.Server.Dns;

public record DnsService(ILogger<DnsService> Logger, ISystemEnvironment Environment) : IHostedService
{
    public const string ServServ = "servserv.generals.ea.com";
    private const string ServerIp = "127.0.0.1";

    private readonly object _stateLock = new();
    private string? _previousIpAddress;
    private bool _hostsFileChanged;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var entry = HostsFile.FindIpAddress(Environment, ServServ);
        if (entry != ServerIp)
        {
            if (entry != null)
                HostsFile.ReplaceEntry(Environment, ServerIp, ServServ);
            else
                HostsFile.AddEntry(Environment, ServerIp, ServServ);

            Logger.LogInformation(
                """Temporarily wrote entry for host "{Host}" in file "{HostFile}" with address "{IpAddress}".""",
                ServServ, Environment.HostsFilePath, ServerIp);

            lock (_stateLock)
            {
                _previousIpAddress = entry;
                _hostsFileChanged = true;
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
            : HostsFile.RemoveEntry(Environment, ServServ);
        if (!status)
        {
            Logger.LogWarning(
                """Cannot restore the file "{HostsFile}": previous IP address "{IpAddress}", host "{Host}".""",
                Environment.HostsFilePath, previousIpAddress, ServerIp);
        }

        return Task.CompletedTask;
    }
}
