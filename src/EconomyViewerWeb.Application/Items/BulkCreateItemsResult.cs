using EconomyViewerWeb.Application.Contracts.Items;

namespace EconomyViewerWeb.Application.Items;

public sealed record BulkCreateItemsResult(
    BulkCreateItemsStatus Status,
    BulkCreateItemsResultDto? Result,
    string? ErrorMessage);
