namespace EconomyViewerWeb.Application.Contracts.Items;

public sealed record BulkCreateItemsRequest(
    string Mod,
    string Lines,
    bool AllowNewMod);
