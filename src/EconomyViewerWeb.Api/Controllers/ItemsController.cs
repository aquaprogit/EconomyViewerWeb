using EconomyViewerWeb.Application.Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using EconomyViewerWeb.Application.Contracts.Items;
using EconomyViewerWeb.Application.Items;


namespace EconomyViewerWeb.Api.Controllers;

[ApiController]
[Route("api/servers/{serverId:guid}/[controller]")]
public class ItemsController : ControllerBase
{

    private readonly IItemService _itemService;

    public ItemsController(IItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResultDto<ItemDto>>> GetItems(
        [FromRoute] Guid serverId,
        [FromQuery] string? mods,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _itemService.GetItemsAsync(
            serverId,
            mods,
            page,
            pageSize);

        return Ok(result);

    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetItem(
        [FromRoute] Guid serverId,
        [FromRoute] Guid id)
    {
        var item = await _itemService.GetItemAsync(serverId, id);

        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> CreateItem(
        [FromRoute] Guid serverId,
        [FromBody] CreateItemRequest request)
    {
        var item = await _itemService.CreateItemAsync(serverId, request);

        return CreatedAtAction(
            nameof(GetItem),
            new { serverId, id = item.Id },
            item);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> UpdateItem(
        [FromRoute] Guid serverId,
        [FromRoute] Guid id,
        [FromBody] UpdateItemRequest request)
    {
        var item = await _itemService.UpdateItemAsync(
            serverId,
            id,
            request);

        return Ok(item);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        [FromRoute] Guid serverId,
        [FromRoute] Guid id)
    {
        await _itemService.DeleteItemAsync(serverId, id);

        return NoContent();
    }

    [HttpPost("bulk")]
    [ProducesResponseType(typeof(BulkCreateItemsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BulkCreateItemsResultDto>> BulkCreateItems(
        [FromRoute] Guid serverId,
        [FromBody] BulkCreateItemsRequest request)
    {
        var result = await _itemService.BulkCreateItemsAsync(
            serverId,
            request);

        return Ok(result);
    }

}
