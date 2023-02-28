using Hammerfest.Server.Env;

namespace Hammerfest.Server.Dns;

/// <remarks>
/// The IO operations in this class are specifically made synchronous to not allow cancellation during read or write
/// and ensure transactional write when possible.
/// </remarks>
public static class HostsFile
{
    /// <summary>
    ///     Overrides the local hosts file.
    /// </summary>
    /// <param name="env">The <see cref="ISystemEnvironment" /> to modify hosts file in</param>
    /// <param name="valuesToSet">
    ///     The <see cref="IReadOnlyDictionary{TKey,TValue}" /> of new DNS entries, where the key is considered the host
    ///     and the value is considered its IP address. Null as the entry value indicates that entry is to be deleted
    /// </param>
    /// <returns>
    ///     The <see cref="Dictionary{TKey,TValue}" /> of initial DNS entries, where the key is considered the host
    ///     and the value is considered its IP address.
    /// </returns>
    /// <remarks>
    ///     Null as the entries' value indicates that the value is to be deleted.
    ///     When new entry is added (not replaced) its resulting value in the returned dictionary is null. It is needed
    ///     to reuse the method on the rollback.
    /// </remarks>
    public static Dictionary<string, string?> ModifyEntries(ISystemEnvironment env, IReadOnlyDictionary<string, string?>
        valuesToSet)
    {
        if (valuesToSet.Count == 0) return new Dictionary<string, string?>();

        var newValues = new Dictionary<string, string?>(valuesToSet);
        var originalLines = File.ReadAllLines(env.HostsFilePath);
        var resultLines = new List<string>();
        var originalEntries = new Dictionary<string, string?>();

        foreach (var line in originalLines)
        {
            var parsedLine = ParseLine(line);
            switch (parsedLine)
            {
                case var (oldIp, host) when IsAlreadyPresent(host, oldIp):
                    resultLines.Add($"{newValues[host]} {host}");
                    newValues.Remove(host);
                    break;
                case var (oldIp, host) when IsReplacement(host):
                    resultLines.Add($"{newValues[host]} {host}");
                    originalEntries.Add(host, oldIp);
                    newValues.Remove(host);
                    break;
                case var (oldIp, host) when IsDeletion(host):
                    originalEntries.Add(host, oldIp);
                    newValues.Remove(host);
                    break;
                default:
                    resultLines.Add(line);
                    break;
            }
        }

        foreach (var (host, ip) in newValues)
        {
            resultLines.Add($"{ip} {host}");
            originalEntries.Add(host, null);
        }

        if (originalEntries.Count != 0)
            File.WriteAllLines(env.HostsFilePath, resultLines);

        return originalEntries;

        bool IsReplacement(string host)
        {
            return newValues.ContainsKey(host) && newValues[host] is not null;
        }

        bool IsDeletion(string host)
        {
            return newValues.ContainsKey(host) && newValues[host] is null;
        }

        bool IsAlreadyPresent(string host, string ip)
        {
            return newValues.TryGetValue(host, out var newIp) && newIp == ip;
        }
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
