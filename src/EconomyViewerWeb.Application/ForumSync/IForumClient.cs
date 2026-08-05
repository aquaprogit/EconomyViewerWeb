using EconomyViewerWeb.Application.ForumSync.Models;

namespace EconomyViewerWeb.Application.ForumSync;

public interface IForumClient
{
    Task<string> DownloadMainPageAsync(
        CancellationToken cancellationToken = default);

    Task<string> DownloadServerPageAsync(
        ForumServerReference serverReference,
        CancellationToken cancellationToken = default);
}
