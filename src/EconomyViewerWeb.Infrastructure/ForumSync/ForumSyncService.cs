using EconomyViewerWeb.Infrastructure.Persistence;
using EconomyViewerWeb.Domain.Entities;
using EconomyViewerWeb.Domain.Enums;
using EconomyViewerWeb.Application.ForumSync;
using EconomyViewerWeb.Application.ForumSync.Models;
using EconomyViewerWeb.Application.Common.Keys;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace EconomyViewerWeb.Infrastructure.ForumSync;

public sealed class ForumSyncService : IForumSyncService
{
    private readonly IForumClient _forumClient;
    private readonly IForumParser _forumParser;
    private readonly EconomyViewerDbContext _dbContext;
    private readonly ILogger<ForumSyncService> _logger;

    public ForumSyncService(
        IForumClient forumClient,
        IForumParser forumParser,
        EconomyViewerDbContext dbContext,
        ILogger<ForumSyncService> logger)
    {
        _forumClient = forumClient;
        _forumParser = forumParser;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Forum synchronization started");

        var forumHtml = await _forumClient.DownloadMainPageAsync(cancellationToken);

        _logger.LogDebug("Downloaded forum page. Length: {Length}", forumHtml.Length);

        var serverLinks = _forumParser.ParseServerReferences(forumHtml);

        _logger.LogInformation("Discovered {Count} economy server links", serverLinks.Count);

        var downloadTasks = serverLinks.Select(async serverLink =>
        {
            try
            {
                var serverHtml = await _forumClient.DownloadServerPageAsync(serverLink, cancellationToken);

                return new
                {
                    ServerLink = serverLink,
                    Html = serverHtml
                };

            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
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

        cancellationToken.ThrowIfCancellationRequested();

        var successfulPages = serverPages
            .Where(page => page is not null)
            .ToList();

        _logger.LogInformation(
            "Successfully downloaded {SuccessCount} of {TotalCount} server pages",
            successfulPages.Count,
            serverLinks.Count);

        var parsedServers = successfulPages
            .Select(page => _forumParser.ParseServer(
                page!.ServerLink,
                page.Html))
            .Where(server => server.Items.Count > 0)
            .ToList();

        var existingServers = await _dbContext.Servers
            .Include(server => server.Items)
            .ToListAsync(cancellationToken);

        var existingServersByName = existingServers.ToDictionary(
            server => server.Name,
            StringComparer.OrdinalIgnoreCase);

        var addedServersCount = 0;
        var addedItemsCount = 0;
        var updatedItemsCount = 0;

        foreach (var parsedServer in parsedServers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!existingServersByName.TryGetValue(
                    parsedServer.Name,
                    out var existingServer))
            {
                existingServer = CreateServerEntity(parsedServer);

                await _dbContext.Servers.AddAsync(existingServer, cancellationToken);

                existingServersByName.Add(
                    existingServer.Name,
                    existingServer);

                addedServersCount++;
                addedItemsCount += existingServer.Items.Count;

                continue;
            }

            var result = SynchronizeServerItems(
                existingServer,
                parsedServer);

            addedItemsCount += result.AddedItems;
            updatedItemsCount += result.UpdatedItems;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Forum synchronization completed. Added {AddedServersCount} servers, " +
            "added {AddedItemsCount} items and updated {UpdatedItemsCount} items",
            addedServersCount,
            addedItemsCount,
            updatedItemsCount);


    }

    private static Server CreateServerEntity(
        ForumServerData serverData)
    {
        var server = new Server
        {
            Name = serverData.Name
        };

        foreach (var itemData in serverData.Items)
        {
            server.Items.Add(new Item
            {
                Name = itemData.Name,
                Mod = itemData.Mod,
                Count = itemData.Count,
                Price = itemData.Price,
                Source = ItemSource.Forum
            });
        }

        return server;
    }

    private static ServerSyncResult SynchronizeServerItems(
        Server existingServer,
        ForumServerData parsedServer)
    {
        var existingForumItemsByKey = existingServer.Items
            .Where(item => item.Source == ItemSource.Forum)
            .ToDictionary(
                item => ForumItemKeyFactory.Create(
                    item.Name,
                    item.Mod),
                StringComparer.OrdinalIgnoreCase);

        var addedItemsCount = 0;
        var updatedItemsCount = 0;

        foreach (var parsedItem in parsedServer.Items)
        {
            var itemKey = ForumItemKeyFactory.Create(
                parsedItem.Name,
                parsedItem.Mod);

            if (!existingForumItemsByKey.TryGetValue(
                    itemKey,
                    out var existingItem))
            {
                var newItem = new Item
                {
                    Name = parsedItem.Name,
                    Mod = parsedItem.Mod,
                    Count = parsedItem.Count,
                    Price = parsedItem.Price,
                    Source = ItemSource.Forum
                };

                existingServer.Items.Add(newItem);
                existingForumItemsByKey.Add(itemKey, newItem);

                addedItemsCount++;

                continue;
            }

            var itemWasUpdated = false;

            if (existingItem.Name != parsedItem.Name)
            {
                existingItem.Name = parsedItem.Name;
                itemWasUpdated = true;
            }

            if (existingItem.Mod != parsedItem.Mod)
            {
                existingItem.Mod = parsedItem.Mod;
                itemWasUpdated = true;
            }

            if (existingItem.Count != parsedItem.Count)
            {
                existingItem.Count = parsedItem.Count;
                itemWasUpdated = true;
            }

            if (existingItem.Price != parsedItem.Price)
            {
                existingItem.Price = parsedItem.Price;
                itemWasUpdated = true;
            }

            if (itemWasUpdated)
            {
                updatedItemsCount++;
            }
        }

        return new ServerSyncResult(
            addedItemsCount,
            updatedItemsCount);
    }


    private readonly record struct ServerSyncResult(
        int AddedItems,
        int UpdatedItems);
}
