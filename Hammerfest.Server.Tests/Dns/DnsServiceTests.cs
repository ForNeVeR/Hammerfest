using Hammerfest.Server.Dns;
using Hammerfest.TestFramework;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Hammerfest.Server.Tests.Dns;

public sealed class DnsServiceTests : IDisposable
{
    private readonly MockEnvironment _environment = new();
    private readonly DnsService _service;
    private readonly string _expected;

    public DnsServiceTests()
    {
        _environment.HostsFilePath = Path.GetTempFileName();
        _service = new DnsService(
            NullLogger<DnsService>.Instance,
            Options.Create(new DnsOptions {Enabled = true}),
            _environment);
        _expected = string.Join("\r\n", DnsService.Domains.Select(domain => "127.0.0.1 " + domain));
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
        FileAssert.ContentsEqual(_expected, content);
    }

    [Fact]
    public async Task StopServiceRollsFileBack()
    {
        await _service.StartAsync(CancellationToken.None);

        var content = await File.ReadAllTextAsync(_environment.HostsFilePath);
        FileAssert.ContentsEqual(_expected, content);

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
        {_expected}
        """, content);

        await File.WriteAllTextAsync(_environment.HostsFilePath, $"""
        127.0.0.1 entry1
        {_expected}
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
    public async Task StopServiceDoesNothingIfTheEntriesWereAlreadyThere()
    {
        await File.WriteAllTextAsync(_environment.HostsFilePath, _expected);

        await _service.StartAsync(CancellationToken.None);

        var content = await File.ReadAllTextAsync(_environment.HostsFilePath);

        Assert.Equal(_expected, content);

        await _service.StopAsync(CancellationToken.None);

        content = await File.ReadAllTextAsync(_environment.HostsFilePath);
        Assert.Equal(_expected, content);
    }
}
