namespace EconomyViewerWeb.Api.Contracts.Items;

public sealed record BulkCreateItemsResultDto(
    int CreatedCount,
    int SkippedCount,
    IReadOnlyCollection<string> Errors);
