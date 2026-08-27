namespace EconomyViewerWeb.Application.ForumSync;

public interface IForumSyncService
{
    Task SyncAsync(
        CancellationToken cancellationToken = default);
}
