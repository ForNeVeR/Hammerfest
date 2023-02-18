using Hammerfest.Server.Env;

namespace Hammerfest.TestFramework;

public class MockEnvironment : ISystemEnvironment
{
    public string HostsFilePath { get; set; } = "";
};
