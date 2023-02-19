using Hammerfest.Server.Dns;
using Hammerfest.TestFramework;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hammerfest.Server.Tests.Dns;

public sealed class DnsServiceTests : IDisposable
{
    private readonly MockEnvironment _environment = new();
    private readonly DnsService _service;

    public DnsServiceTests()
    {
        _environment.HostsFilePath = Path.GetTempFileName();
        _service = new DnsService(
            NullLogger<DnsService>.Instance,
            Options.Create(new DnsOptions { Enabled = true  }),
            _environment);
    }

    public void Dispose()
    {
        File.Delete(_environment.HostsFilePath);
    }

    [Fact]
    public async Task StartServiceRewritesHostsFile()
    {
        await _service.StartAsync(CancellationToken.None);

        var content = await File.ReadAllTextAsync(_environment.HostsFilePath);
        FileAssert.ContentsEqual($"""
        127.0.0.1 {DnsService.ServServ}
        """, content);
    }

    [Fact]
    public async Task StopServiceRollsFileBack()
    {
        await _service.StartAsync(CancellationToken.None);

        var content = await File.ReadAllTextAsync(_environment.HostsFilePath);
        FileAssert.ContentsEqual($"""
        127.0.0.1 {DnsService.ServServ}
        """, content);

        await _service.StopAsync(CancellationToken.None);

        content = await File.ReadAllTextAsync(_environment.HostsFilePath);
        Assert.Empty(content);
    }

    [Fact]
    public async Task StopServiceOnlyRemovesTheNewEntry()
    {
        await File.WriteAllTextAsync(_environment.HostsFilePath, """
        127.0.0.1 entry1
        """);

        await _service.StartAsync(CancellationToken.None);

        var content = await File.ReadAllTextAsync(_environment.HostsFilePath);
        FileAssert.ContentsEqual($"""
        127.0.0.1 entry1
        127.0.0.1 {DnsService.ServServ}
        """, content);

        await File.WriteAllTextAsync(_environment.HostsFilePath, $"""
        127.0.0.1 entry1
        127.0.0.1 {DnsService.ServServ}
        127.0.0.1 something_new
        """);

        await _service.StopAsync(CancellationToken.None);

        content = await File.ReadAllTextAsync(_environment.HostsFilePath);
        FileAssert.ContentsEqual("""
        127.0.0.1 entry1
        127.0.0.1 something_new
        """, content);
    }

    [Fact]
    public async Task StopServiceDoesNothingIfTheEntryWasAlreadyThere()
    {
        await File.WriteAllTextAsync(_environment.HostsFilePath, $"""
        127.0.0.1 {DnsService.ServServ}
        """);

        await _service.StartAsync(CancellationToken.None);

        var content = await File.ReadAllTextAsync(_environment.HostsFilePath);
        Assert.Equal($"""
        127.0.0.1 {DnsService.ServServ}
        """, content);

        await _service.StopAsync(CancellationToken.None);

        content = await File.ReadAllTextAsync(_environment.HostsFilePath);
        Assert.Equal($"""
        127.0.0.1 {DnsService.ServServ}
        """, content);
    }
}
