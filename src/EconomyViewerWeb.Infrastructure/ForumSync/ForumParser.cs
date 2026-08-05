using EconomyViewerWeb.Application.Common.Normalization;
using EconomyViewerWeb.Application.ForumSync;
using EconomyViewerWeb.Application.ForumSync.Models;
using EconomyViewerWeb.Application.Parsing;
using EconomyViewerWeb.Application.Common.Keys;
using HtmlAgilityPack;

namespace EconomyViewerWeb.Infrastructure.ForumSync;

public sealed class ForumParser : IForumParser
{
    public IReadOnlyCollection<ForumServerReference> ParseServerReferences(
        string html)
    {
        var document = new HtmlDocument();

        document.LoadHtml(html);

        var linkNodes = document.DocumentNode.SelectNodes("//a");
        var serverReferences = new List<ForumServerReference>();

        if (linkNodes is null)
        {
            return serverReferences;
        }

        foreach (var linkNode in linkNodes)
        {
            var title = linkNode.InnerText.Trim();

            if (!title.StartsWith(
                    "Экономика ",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var href = linkNode.GetAttributeValue(
                "href",
                string.Empty);

            if (string.IsNullOrWhiteSpace(href))
            {
                continue;
            }

            if (!href.Contains(
                    "/topic/",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            serverReferences.Add(
                new ForumServerReference(title, href));
        }

        return serverReferences;
    }

    public ForumServerData ParseServer(
        ForumServerReference serverReference,
        string html)
    {
        var document = new HtmlDocument();

        document.LoadHtml(html);

        var contentNode = document.DocumentNode
            .SelectSingleNode("//div[@data-role='commentContent']");

        var serverName = NormalizeServerName(serverReference.Name);

        var itemsByKey =
            new Dictionary<string, ForumItemData>(
                StringComparer.OrdinalIgnoreCase);

        if (contentNode is null)
        {
            return new ForumServerData(
                serverName,
                Array.Empty<ForumItemData>());
        }

        string? currentMod = null;

        foreach (var childNode in contentNode.ChildNodes)
        {
            var mod = TryGetModName(childNode);

            if (mod is not null)
            {
                currentMod = ItemTextNormalizer.NormalizeRequired(
                    mod,
                    nameof(mod));

                continue;
            }

            var lines = childNode.InnerText.Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

            foreach (var line in lines)
            {
                var parsedItem = ItemLineParser.TryParse(line);

                if (parsedItem is null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(currentMod))
                {
                    continue;
                }

                var normalizedName = ItemTextNormalizer.NormalizeRequired(
                    parsedItem.Name,
                    nameof(parsedItem.Name));

                var itemData = new ForumItemData(
                    normalizedName,
                    currentMod,
                    parsedItem.Count,
                    parsedItem.Price);

                var itemKey = ForumItemKeyFactory.Create(
                    itemData.Name,
                    itemData.Mod);

                itemsByKey[itemKey] = itemData;
            }
        }

        return new ForumServerData(
            serverName,
            itemsByKey.Values.ToList());
    }

    private static string? TryGetModName(HtmlNode node)
    {
        if (!node.Name.Equals(
                "ul",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var mod = node.InnerText.Trim();

        return string.IsNullOrWhiteSpace(mod)
            ? null
            : mod;
    }

    private static string NormalizeServerName(string forumTitle)
    {
        return forumTitle
            .Replace(
                "Экономика ",
                string.Empty,
                StringComparison.OrdinalIgnoreCase)
            .Trim();
    }
}
