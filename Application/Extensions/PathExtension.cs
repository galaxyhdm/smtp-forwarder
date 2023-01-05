using System.Text.RegularExpressions;

namespace SmtpForwarder.Application.Extensions;

public static class PathExtensions
{
    private static readonly string RegexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
    private static readonly Regex EscapeRegex = new($"[{Regex.Escape(RegexSearch)}]");

    public static string EscapePath(this string path)
    {
       return EscapeRegex.Replace(path, "");
    }
}