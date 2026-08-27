using EconomyViewerWeb.Application.ForumSync;
using EconomyViewerWeb.Application.ForumSync.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EconomyViewerWeb.Infrastructure.ForumSync;

public sealed class ForumClient : IForumClient
{
    private readonly HttpClient _httpClient;
    private readonly ForumSyncOptions _options;
    private readonly ILogger<ForumClient> _logger;

    public ForumClient(
        HttpClient httpClient,
        IOptions<ForumSyncOptions> options,
        ILogger<ForumClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> DownloadMainPageAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Downloading economy forum page from {Url}",
            _options.EconomyForumUrl);

        return await _httpClient.GetStringAsync(
            _options.EconomyForumUrl, cancellationToken);
    }

    public async Task<string> DownloadServerPageAsync(
        ForumServerReference? serverReference, CancellationToken cancellationToken = default)
    {
        var url = ResolveUrl(serverReference.Url);

        _logger.LogDebug(
            "Downloading price list for {ServerName} from {Url}",
            serverReference.Name,
            url);

        return await _httpClient.GetStringAsync(url, cancellationToken);
    }

    private Uri ResolveUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        var forumUri = new Uri(_options.EconomyForumUrl);

        return new Uri(forumUri, url);
    }
}
