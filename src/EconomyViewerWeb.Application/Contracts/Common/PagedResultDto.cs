namespace EconomyViewerWeb.Application.Contracts.Common;

public sealed record PagedResultDto<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
