using EconomyViewerWeb.Api.Contracts.Common;
using EconomyViewerWeb.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using EconomyViewerWeb.Api.Contracts.Items;
using Microsoft.EntityFrameworkCore;

namespace EconomyViewerWeb.Api.Controllers;

[ApiController]
[Route("api/servers/{serverId:guid}/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly EconomyViewerDbContext _dbContext;

    public ItemsController(EconomyViewerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<ItemDto>>> GetItems(
        [FromRoute] Guid serverId,
        [FromQuery] string? mods,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);
        if (!serverExists)
        {
            return NotFound();
        }

        var selectedMods = mods?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var query = _dbContext.Items
            .Where(item => item.ServerId == serverId);

        if (selectedMods is { Length: > 0 })
        {
            query = query.Where(item =>
                item.Mod != null &&
                selectedMods.Contains(item.Mod));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(item => item.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new ItemDto(
                item.Id,
                item.Name,
                item.Count,
                item.Price,
                item.Mod,
                item.PriceForOne))
            .ToListAsync();

        var result = new PagedResultDto<ItemDto>(
            items,
            page,
            pageSize,
            totalCount);


        return Ok(result);

    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetItem(
        [FromRoute] Guid serverId,
        [FromRoute] Guid id)
    {
        var item = await _dbContext.Items
            .Where(item => item.ServerId == serverId && item.Id == id)
            .Select(item => new ItemDto(
                item.Id,
                item.Name,
                item.Count,
                item.Price,
                item.Mod,
                item.PriceForOne))
            .FirstOrDefaultAsync();

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}
