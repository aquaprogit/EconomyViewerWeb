namespace EconomyViewerWeb.Infrastructure.ForumSync;

public interface IForumSyncService
{
    Task SeedIfEmptyAsync();
}
