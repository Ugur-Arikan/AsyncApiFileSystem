using System.Globalization;

namespace AsyncFileSystem;

internal static class Singletons
{
    internal const string FmtTime = "yyyy-MM-dd HH:mm:ss";
    internal static readonly CultureInfo Culture = new("en-US");
}
