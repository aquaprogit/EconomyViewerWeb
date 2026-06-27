using EconomyViewerWeb.Infrastructure.Persistence;
using EconomyViewerWeb.Domain.Entities;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace EconomyViewerWeb.Infrastructure.ForumSync;

public class ForumSyncService : IForumSyncService
{
    private readonly HttpClient _httpClient;
    private readonly EconomyViewerDbContext _dbContext;
    private readonly ForumSyncOptions _options;
    private readonly ILogger<ForumSyncService> _logger;

    public ForumSyncService(
        HttpClient httpClient,
        EconomyViewerDbContext dbContext,
        IOptions<ForumSyncOptions> options,
        ILogger<ForumSyncService> logger)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedIfEmptyAsync()
    {
        _logger.LogInformation("Forum seed started");

        var forumHtml = await DownloadForumPageAsync();

        _logger.LogDebug("Downloaded forum page. Length: {Length}", forumHtml.Length);

        var serverLinks = DiscoverServerLinks(forumHtml);

        _logger.LogInformation("Discovered {Count} economy server links", serverLinks.Count);

        var existingServerNames = await _dbContext.Servers
            .Select(server => server.Name)
            .ToListAsync();

        var missingServerLinks = serverLinks
            .Where(link =>
            {
                var serverName = NormalizeServerName(link.Name);

                return !existingServerNames.Contains(serverName);
            })
            .ToList();

        if (missingServerLinks.Count == 0)
        {
            _logger.LogInformation("All forum servers already exist in database. Forum seed skipped");
            return;
        }

        var downloadTasks = missingServerLinks.Select(async serverLink =>
        {
            try
            {
                var serverHtml = await DownloadServerPageAsync(serverLink);

                return new
                {
                    ServerLink = serverLink,
                    Html = serverHtml
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to download server page for {ServerName}",
                    serverLink.Name);

                return null;
            }

        });

        var serverPages = await Task.WhenAll(downloadTasks);
        var successfulPages = serverPages
            .Where(page => page is not null)
            .ToList();
        _logger.LogInformation(
            "Successfully downloaded {SuccessCount} of {TotalCount} server pages",
            successfulPages.Count,
            missingServerLinks.Count);

        var servers = successfulPages
            .Select(page => ParseServerPage(page!.ServerLink, page.Html))
            .Where(server => server.Items.Any())
            .ToList();

        _logger.LogInformation(
            "Parsed {ServerCount} servers with {ItemsCount} total items",
            servers.Count,
            servers.Sum(server => server.Items.Count));

        await _dbContext.Servers.AddRangeAsync(servers);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Forum seed completed. Saved {ServerCount} servers",
            servers.Count);


    }

    private async Task<string> DownloadForumPageAsync()
    {
        _logger.LogInformation("Downloading economy forum page from {Url}", _options.EconomyForumUrl);

        return await _httpClient.GetStringAsync(_options.EconomyForumUrl);
    }

    private IReadOnlyCollection<ForumServerLink> DiscoverServerLinks(string html)
    {
        var document = new HtmlDocument();

        document.LoadHtml(html);

        var linkNodes = document.DocumentNode.SelectNodes("//a");

        _logger.LogInformation(
            "Found {Count} links on forum page",
            linkNodes?.Count ?? 0);

        var serverLinks = new List<ForumServerLink>();

        if (linkNodes is null)
        {
            return serverLinks;
        }

        foreach(var linkNode in linkNodes)
        {
            var title = linkNode.InnerText.Trim();
            if (!title.StartsWith("Экономика ", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var href = linkNode.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrWhiteSpace(href))
            {
                continue;
            }

            if (!href.Contains("/topic/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            serverLinks.Add(new ForumServerLink(title, href));

        }

        return serverLinks;
    }

    private async Task<string> DownloadServerPageAsync(ForumServerLink serverLink)
    {
        _logger.LogDebug(
            "Downloading price list for {ServerName} from {Url}",
            serverLink.Name,
            serverLink.Url);

        return await _httpClient.GetStringAsync(serverLink.Url);
    }

    private Server ParseServerPage(ForumServerLink serverLink, string html)
    {
        var document = new HtmlDocument();

        document.LoadHtml(html);

        var contentNode = document.DocumentNode
            .SelectSingleNode("//div[@data-role='commentContent']");

        var serverName = NormalizeServerName(serverLink.Name);

        var server = new Server
        {
            Name = serverName
        };

        if (contentNode is null)
        {
            return server;
        }

        var currentMod = string.Empty;

        foreach (var childNode in contentNode.ChildNodes)
        {
            var mod = TryGetModName(childNode);

            if (mod is not null)
            {
                currentMod = mod;
                continue;
            }

            var lines = childNode.InnerText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var line in lines)
            {
                var item = ForumItemParser.TryParseItem(line, currentMod);

                if (item is not null)
                {
                    server.Items.Add(item);
                }
            }
        }

        return server;
    }

    private static string? TryGetModName(HtmlNode node)
    {
        if (!node.Name.Equals("ul", StringComparison.OrdinalIgnoreCase))
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
            .Replace("Экономика ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }
}
