namespace EconomyViewerWeb.Application.ForumSync.Models;

public sealed record ForumServerData(
    string Name,
    IReadOnlyCollection<ForumItemData> Items);
