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

        if (result is null)
        {
            return NotFound();
        }

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

        if (item is null)
        {
            return NotFound();
        }

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
        var result = await _itemService.CreateItemAsync(serverId, request);

        if (result.Status == CreateItemStatus.ServerNotFound)
        {
            return NotFound();
        }

        if (result.Status == CreateItemStatus.ValidationError)
        {
            return BadRequest(result.ErrorMessage);
        }

        return CreatedAtAction(
            nameof(GetItem),
            new { serverId, id = result.Item!.Id },
            result.Item);
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
        var result = await _itemService.UpdateItemAsync(
            serverId,
            id,
            request);

        if (result.Status == UpdateItemStatus.ItemNotFound)
        {
            return NotFound();
        }

        if (result.Status == UpdateItemStatus.ValidationError)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Item);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        [FromRoute] Guid serverId,
        [FromRoute] Guid id)
    {
        var status = await _itemService.DeleteItemAsync(serverId, id);

        if (status == DeleteItemStatus.ItemNotFound)
        {
            return NotFound();
        }

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
        var result = await _itemService.BulkCreateItemsAsync(serverId, request);

        if (result.Status == BulkCreateItemsStatus.ServerNotFound)
        {
            return NotFound();
        }

        if (result.Status == BulkCreateItemsStatus.ValidationError)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Result);
    }

}
