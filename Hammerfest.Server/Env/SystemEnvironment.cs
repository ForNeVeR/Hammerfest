namespace Hammerfest.Server.Env;

public interface ISystemEnvironment
{
    public string HostsFilePath { get; }
}

public class WindowsEnvironment : ISystemEnvironment
{
    public string HostsFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.System),
        "drivers/etc/hosts");
}
