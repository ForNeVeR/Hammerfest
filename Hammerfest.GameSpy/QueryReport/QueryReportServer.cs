using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hammerfest.GameSpy.QueryReport;

internal record QueryReportServer(ILogger<QueryReportServer> Logger) : IHostedService
{
    private const int QrPort = 27900;
    private static readonly Encoding Encoding = Encoding.UTF8;

    // Last byte 0x00 = Available
    private static ReadOnlyMemory<byte> AvailableResponse => new byte[] { 0xFE, 0xFD, 0x09, 0x00, 0x00, 0x00, 0x00 };

    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;
    private Task? _listenerTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _udpClient = new UdpClient(QrPort);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Logger.LogInformation("Starting listener on port {Port}.", QrPort);

        _listenerTask = StartListening(_udpClient, _cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Stopping service.");
        _cts?.Cancel();
        if (_listenerTask != null)
            await _listenerTask;
    }

    private async Task StartListening(UdpClient udp, CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            var datagram = await udp.ReceiveAsync(cancellation);
            try
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    Logger.LogTrace("Message received from client {Client}:\n{Message}",
                        datagram.RemoteEndPoint, FormatDatagram(datagram.Buffer));
                }

                await ProcessDatagram(udp, datagram, cancellation);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Exception during datagram processing ({Length}) from client {Client}.",
                    datagram.Buffer.Length, datagram.RemoteEndPoint);
            }
        }
    }

    private async Task ProcessDatagram(UdpClient udp, UdpReceiveResult datagram, CancellationToken cancellation)
    {
        var buffer = datagram.Buffer;

        var messageType = buffer[0];
        if (messageType != 0x09) throw new Exception($"Cannot process message type {messageType}.");

        if (buffer.Length >= 5)
        {
            var padding = BitConverter.ToInt32(buffer.AsSpan()[1..5]);
            if (padding != 0)
                Logger.LogWarning("Message has unexpected padding bytes: {Padding}.", padding);

            var gameName = Encoding.GetString(buffer, 5, buffer.Length - 5).TrimEnd('\0');
            if (gameName != "cc3xp1") throw new Exception($"Unexpected game name: {gameName}.");

            await udp.SendAsync(AvailableResponse, datagram.RemoteEndPoint, cancellation);
            Logger.LogInformation("Sent a response to {Client}.", datagram.RemoteEndPoint);
        }
        else
            throw new Exception("Unexpected message length.");
    }

    private static string FormatDatagram(byte[] datagram) => BitConverter.ToString(datagram);
}
