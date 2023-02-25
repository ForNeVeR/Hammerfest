using System.Net;
using System.Net.Sockets;
using Hammerfest.GameSpy.QueryReport;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hammerfest.GameSpy.Tests;

public class QueryReportServerTests
{
    [Fact]
    public async Task QrServerResponse()
    {
        using var server = new QueryReportServer(NullLogger<QueryReportServer>.Instance);
        await server.StartAsync(CancellationToken.None);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10.0));
        try
        {
            using var client = new UdpClient();
            var message = "\x09\0\0\0\0cc3xp1\0"u8.ToArray();
            await client.SendAsync(message, IPEndPoint.Parse($"127.0.0.1:{QueryReportServer.Port}"), cts.Token);
            var response = await client.ReceiveAsync(cts.Token);
            Assert.Equal(new byte[] { 0xFE, 0xFD, 0x09, 0x00, 0x00, 0x00, 0x00 }, response.Buffer);
        }
        finally
        {
            await server.StopAsync(CancellationToken.None);
        }
    }
}
