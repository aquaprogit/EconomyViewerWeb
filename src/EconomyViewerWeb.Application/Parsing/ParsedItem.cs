namespace EconomyViewerWeb.Application.Parsing;

public sealed record ParsedItem(
    string Name,
    int Count,
    int Price);
