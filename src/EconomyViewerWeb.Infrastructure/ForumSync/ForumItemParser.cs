using System.Text.RegularExpressions;
using EconomyViewerWeb.Domain.Entities;

namespace EconomyViewerWeb.Infrastructure.ForumSync;

public static class ForumItemParser
{
    private static readonly Regex FullItemRegex = new(
        @"^(?<header>.+?)\s+(?<count>\d+)\s*шт\.\s*-\s*(?<price>\d+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ShortItemRegex = new(
        @"^(?<header>.+?)\s+(?<price>\d+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static Item? TryParseItem(string line, string? currentMod)
    {
        line = line.Trim();

        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        var fullMatch = FullItemRegex.Match(line);

        if (fullMatch.Success)
        {
            var header = fullMatch.Groups["header"].Value;
            if (!int.TryParse(fullMatch.Groups["count"].Value, out var count) ||
                !int.TryParse(fullMatch.Groups["price"].Value, out var price))
            {
                return null;
            }

            var item = new Item
            {
                Header = header,
                Count = count,
                Price = price,
                Mod = currentMod
            };

            return item;

        }

        var shortMatch = ShortItemRegex.Match(line);

        if (shortMatch.Success)
        {
            var header = shortMatch.Groups["header"].Value;
            if (!int.TryParse(shortMatch.Groups["price"].Value, out var price))
            {
                return null;
            }

            var item = new Item
            {
                Header = header,
                Count = 1,
                Price = price,
                Mod = currentMod
            };

            return item;
        }


        return null;
    }

    public static string? TryParseModHeader(string line)
    {
        line = line.Trim();

        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        if (line.EndsWith(':'))
        {
            return line.TrimEnd(':').Trim();
        }

        if (line.StartsWith('*') && line.EndsWith('*'))
        {
            return line.Trim('*').Trim();
        }

        return null;
    }
}
