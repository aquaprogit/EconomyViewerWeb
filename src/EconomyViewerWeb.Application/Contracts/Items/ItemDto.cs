namespace EconomyViewerWeb.Application.Contracts.Items;

public sealed record ItemDto(
    Guid Id,
    string Name,
    int Count,
    int Price,
    string? Mod,
    int PriceForOne);
