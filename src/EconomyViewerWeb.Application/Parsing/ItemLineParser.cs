using System.Text.RegularExpressions;

namespace EconomyViewerWeb.Application.Parsing;

public static class ItemLineParser
{
    private static readonly Regex FullItemRegex = new(
        @"^(?<name>.+?)\s+(?<count>\d+)\s*шт\.\s*-\s*(?<price>\d+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ShortItemRegex = new(
        @"^(?<name>.+?)\s+(?<price>\d+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static ParsedItem? TryParse(string line)
    {
        line = line.Trim();

        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        var fullMatch = FullItemRegex.Match(line);

        if (fullMatch.Success)
        {
            var name = fullMatch.Groups["name"].Value;

            if (!int.TryParse(fullMatch.Groups["count"].Value, out var count) ||
                !int.TryParse(fullMatch.Groups["price"].Value, out var price))
            {
                return null;
            }

            return new ParsedItem(name, count, price);
        }

        var shortMatch = ShortItemRegex.Match(line);

        if (shortMatch.Success)
        {
            var name = shortMatch.Groups["name"].Value;

            if (!int.TryParse(shortMatch.Groups["price"].Value, out var price))
            {
                return null;
            }

            return new ParsedItem(name, 1, price);
        }

        return null;
    }
}
