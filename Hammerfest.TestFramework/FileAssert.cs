using Xunit;

namespace Hammerfest.TestFramework;

public static class FileAssert
{
    public static void ContentsEqual(string expected, string actual)
    {
        string Normalize(string s)
        {
            if (!s.EndsWith('\n')) s += '\n';
            return s.ReplaceLineEndings();
        }

        Assert.Equal(Normalize(expected), Normalize(actual));
    }
}
