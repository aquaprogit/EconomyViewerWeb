using EconomyViewerWeb.Infrastructure.Persistence;
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

        var hasServers = await _dbContext.Servers.AnyAsync();

        if (hasServers)
        {
            _logger.LogInformation("Database already contains servers. Forum seed skipped");
            return;
        }

        _logger.LogInformation("Database is empty. Starting forum import");

        var forumHtml = await DownloadForumPageAsync();

        _logger.LogInformation("Downloaded forum page. Length: {Length}", forumHtml.Length);
    }

    private async Task<string> DownloadForumPageAsync()
    {
        _logger.LogInformation("Downloading economy forum page from {Url}", _options.EconomyForumUrl);

        return await _httpClient.GetStringAsync(_options.EconomyForumUrl);
    }
}
