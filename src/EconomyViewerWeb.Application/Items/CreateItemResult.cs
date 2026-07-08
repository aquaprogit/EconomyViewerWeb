using EconomyViewerWeb.Application.Contracts.Items;

namespace EconomyViewerWeb.Application.Items;

public sealed record CreateItemResult(
    CreateItemStatus Status,
    ItemDto? Item,
    string? ErrorMessage);
