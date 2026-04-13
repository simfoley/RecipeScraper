using System.Globalization;
using System.Text;

namespace RecipeScraper.Extensions;

internal static class StringExtensions
{
    // Collapses all Unicode whitespace variants (e.g. non-breaking spaces) into single regular spaces, then trims.
    public static string? NormalizeWhitespace(this string? value)
    {
        if (value is null) return null;
        var sb = new StringBuilder();
        bool lastWasSpace = false;
        foreach (char c in value)
        {
            if (char.IsWhiteSpace(c) || char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator)
            {
                if (!lastWasSpace) sb.Append(' ');
                lastWasSpace = true;
            }
            else
            {
                sb.Append(c);
                lastWasSpace = false;
            }
        }
        var result = sb.ToString().Trim();
        return result.Length > 0 ? result : null;
    }
}
