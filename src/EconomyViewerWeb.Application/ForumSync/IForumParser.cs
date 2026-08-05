using EconomyViewerWeb.Application.ForumSync.Models;

namespace EconomyViewerWeb.Application.ForumSync;

public interface IForumParser
{
    IReadOnlyCollection<ForumServerReference> ParseServerReferences(string html);

    ForumServerData ParseServer(
        ForumServerReference serverReference,
        string html);
}
