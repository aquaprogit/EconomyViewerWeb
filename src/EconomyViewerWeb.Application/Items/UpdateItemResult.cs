using EconomyViewerWeb.Application.Contracts.Items;

namespace EconomyViewerWeb.Application.Items;

public sealed record UpdateItemResult(
    UpdateItemStatus Status,
    ItemDto? Item,
    string? ErrorMessage);
