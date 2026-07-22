using EconomyViewerWeb.Application.Contracts.Common;
using EconomyViewerWeb.Application.Contracts.Items;

namespace EconomyViewerWeb.Application.Items;

public interface IItemService
{
    Task<PagedResultDto<ItemDto>> GetItemsAsync(
        Guid serverId,
        string? mods,
        int page,
        int pageSize);

    Task<ItemDto> GetItemAsync(Guid serverId, Guid id);

    Task<ItemDto> CreateItemAsync(
        Guid serverId,
        CreateItemRequest request);

    Task<ItemDto> UpdateItemAsync(
        Guid serverId,
        Guid id,
        UpdateItemRequest request);

    Task DeleteItemAsync(Guid serverId, Guid id);

    Task<BulkCreateItemsResultDto> BulkCreateItemsAsync(
        Guid serverId,
        BulkCreateItemsRequest request);
}
