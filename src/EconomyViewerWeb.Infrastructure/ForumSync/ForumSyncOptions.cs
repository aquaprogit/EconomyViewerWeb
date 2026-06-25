namespace EconomyViewerWeb.Infrastructure.ForumSync;

public sealed class ForumSyncOptions
{
    public const string SectionName = "ForumSync";

    public string EconomyForumUrl { get; set; } = string.Empty;
}
