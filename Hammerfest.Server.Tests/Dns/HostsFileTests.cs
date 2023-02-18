using Hammerfest.Server.Dns;
using Hammerfest.Server.Env;
using Hammerfest.TestFramework;

namespace Hammerfest.Server.Tests.Dns;

public class HostsFileTests
{
    [Fact]
    public void FindIpAddressWorks()
    {
        DoTest("""
        ::1 address
        """, env =>
        {
            Assert.Equal("::1", HostsFile.FindIpAddress(env, "address"));
        });
    }

    [Fact]
    public void FindIpAddressSkipsComments()
    {
        DoTest("""
        #::1 address
        """, env =>
        {
            Assert.Null(HostsFile.FindIpAddress(env, "address"));
        });
    }

    [Fact]
    public void FindIpAddressReturnsNull()
    {
        DoTest("""
        ::1 address
        """, env =>
        {
            Assert.Null(HostsFile.FindIpAddress(env, "address2"));
        });
    }

    [Fact]
    public void AddEntryWorks()
    {
        DoTest("", env =>
        {
            HostsFile.AddEntry(env, "::1", "testy");
        }, """
        ::1 testy
        """);
    }

    [Fact]
    public void ReplaceEntryWorks()
    {
        DoTest("""
        ::1 address1
        ::1 address2
        ::1 address3
        """, env =>
        {
            HostsFile.ReplaceEntry(env, "::2", "address2");
        }, """
        ::1 address1
        ::2 address2
        ::1 address3
        """);
    }

    [Fact]
    public void ReplaceEntryRemovesComments()
    {
        DoTest("""
        ::1 address1
        ::1 address2 # aaa
        ::1 address3
        """, env =>
        {
            HostsFile.ReplaceEntry(env, "::2", "address2");
        }, """
        ::1 address1
        ::2 address2
        ::1 address3
        """);
    }

    [Fact]
    public void RemoveEntryWorks()
    {
        DoTest("""
        ::1 address1
        ::1 address2 # aaa
        ::1 address3
        """, env =>
        {
            Assert.True(HostsFile.RemoveEntry(env, "address2"));
        }, """
        ::1 address1
        ::1 address3
        """);
    }

    [Fact]
    public void RemoveEntryReturnsFalse()
    {
        DoTest("""
        ::1 address1
        ::1 address3
        """, env =>
        {
            Assert.False(HostsFile.RemoveEntry(env, "address2"));
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
