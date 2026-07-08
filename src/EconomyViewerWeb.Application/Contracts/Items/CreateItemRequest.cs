namespace EconomyViewerWeb.Application.Contracts.Items;

public sealed record CreateItemRequest(
    string Name,
    int Count,
    int Price,
    string Mod);
