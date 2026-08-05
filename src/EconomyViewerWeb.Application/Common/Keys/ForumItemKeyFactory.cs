namespace EconomyViewerWeb.Application.Common.Keys;

public static class ForumItemKeyFactory
{
    public static string Create(
        string name,
        string mod)
    {
        return $"{name}\u001F{mod}";
    }
}
