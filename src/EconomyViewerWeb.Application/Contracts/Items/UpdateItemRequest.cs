namespace EconomyViewerWeb.Application.Contracts.Items;

public sealed record UpdateItemRequest(
    string Name,
    int Count,
    int Price,
    string Mod);
