using System.Text;
using Hammerfest.Server.Env;

namespace Hammerfest.Server.Dns;

/// <remarks>
/// The IO operations in this class are specifically made synchronous to not allow cancellation during read or write
/// and ensure transactional write when possible.
/// </remarks>
public static class HostsFile
{
    public static string? FindIpAddress(ISystemEnvironment env, string host)
    {
        foreach (var line in File.ReadAllLines(env.HostsFilePath))
        {
            if (ParseLine(line) is (var ipAddress, var currentHost) && currentHost == host)
                return ipAddress;
        }

        return null;
    }

    public static void AddEntry(ISystemEnvironment env, string ipAddress, string host)
    {
        var content = File.ReadAllText(env.HostsFilePath);
        var result = new StringBuilder(content);
        if (content.Length > 0 && !content.EndsWith('\n'))
            result.Append('\n');
        result.Append($"{ipAddress} {host}\r\n");

        File.WriteAllText(env.HostsFilePath, result.ToString());
    }

    public static bool RemoveEntry(ISystemEnvironment env, string host)
    {
        var originalLines = File.ReadAllLines(env.HostsFilePath);
        var resultLines = new List<string>();
        var removed = false;
        foreach (var line in originalLines)
        {
            if (ParseLine(line) is var (_, currentHost) && currentHost == host)
            {
                removed = true;
            }
            else
                resultLines.Add(line);
        }

        if (removed)
            File.WriteAllLines(env.HostsFilePath, resultLines);

        return removed;
    }

    public static bool ReplaceEntry(ISystemEnvironment env, string ipAddress, string host)
    {
        var originalLines = File.ReadAllLines(env.HostsFilePath);
        var resultLines = new List<string>();
        var replaced = false;
        foreach (var line in originalLines)
        {
            if (ParseLine(line) is var (_, currentHost) && currentHost == host)
            {
                resultLines.Add($"{ipAddress} {host}");
                replaced = true;
            }
            else
                resultLines.Add(line);
        }

        if (replaced)
            File.WriteAllLines(env.HostsFilePath, resultLines);

        return replaced;
    }

    private static ValueTuple<string, string>? ParseLine(string line)
    {
        var commentStartMarker = line.IndexOf('#');
        if (commentStartMarker != -1)
            line = line.Substring(0, commentStartMarker);

        line = line.Trim();

        if (line == "")
            return null;

        var components = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (components.Length < 2)
            return null;

        return (components[0], components[1]);
    }
}
