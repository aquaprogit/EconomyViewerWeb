using EconomyViewerWeb.Application.ForumSync;
using Hangfire;

namespace EconomyViewerWeb.Api.Jobs;

public sealed class ForumSyncJob
{
    private readonly IForumSyncService _forumSyncService;

    public ForumSyncJob(
        IForumSyncService forumSyncService)
    {
        _forumSyncService = forumSyncService;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60)]
    public Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        return _forumSyncService.SyncAsync(
            cancellationToken);
    }
}
