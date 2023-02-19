namespace Hammerfest.Server.Dns;

public record DnsOptions
{
    public bool Enabled { get; init; } = false;
}
