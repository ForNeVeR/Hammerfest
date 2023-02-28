using Hammerfest.Server.Dns;
using Hammerfest.Server.Env;
using Hammerfest.TestFramework;

namespace Hammerfest.Server.Tests.Dns;

public class HostsFileTests
{
    [Fact]
    public void ModifyEntriesAddsEntries()
    {
        DoTest("", env =>
        {
            HostsFile.ModifyEntries(env, new Dictionary<string, string?>
                {
                    ["testy"] = "::1"
                }
            );
        }, """
        ::1 testy
        """);
    }

    [Fact]
    public void ModifyEntriesReplacesEntries()
    {
        DoTest("""
        ::1 address1
        ::1 address2
        ::1 address3
        """, env =>
        {
            HostsFile.ModifyEntries(env, new Dictionary<string, string?>
            {
                ["address2"] = "::2"
            });
        }, """
        ::1 address1
        ::2 address2
        ::1 address3
        """);
    }

    [Fact]
    public void ModifyEntriesRemovesCommentsOnReplace()
    {
        DoTest("""
        ::1 address1
        ::1 address2 # aaa
        ::1 address3
        """, env =>
        {
            HostsFile.ModifyEntries(env, new Dictionary<string, string?>
            {
                ["address2"] = "::2"
            });
        }, """
        ::1 address1
        ::2 address2
        ::1 address3
        """);
    }

    [Fact]
    public void ModifyEntriesRemovesEntries()
    {
        DoTest("""
        ::1 address1
        ::1 address2 # aaa
        ::1 address3
        """, env =>
        {
            Assert.Equal(new Dictionary<string, string?>
            {
                ["address2"] = "::1"
            }, HostsFile.ModifyEntries(env, new Dictionary<string, string?>
            {
                ["address2"] = null
            }));
        }, """
        ::1 address1
        ::1 address3
        """);
    }

    private static void DoTest(string initialContent, Action<ISystemEnvironment> test, string? expectedContent = null)
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, initialContent);

            var env = new MockEnvironment
            {
                HostsFilePath = file
            };

            test(env);

            var content = File.ReadAllText(file);

            if (expectedContent != null)
                FileAssert.ContentsEqual(expectedContent, content);
        }
        finally
        {
            File.Delete(file);
        }
    }
}
