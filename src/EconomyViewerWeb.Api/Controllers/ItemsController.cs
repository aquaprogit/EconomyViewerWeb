using EconomyViewerWeb.Api.Contracts.Common;
using EconomyViewerWeb.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using EconomyViewerWeb.Api.Contracts.Items;
using EconomyViewerWeb.Domain.Entities;
using EconomyViewerWeb.Application.Parsing;
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
    public async Task<ActionResult<PagedResultDto<ItemDto>>> GetItems(
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

    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> CreateItem(
        [FromRoute] Guid serverId,
        [FromBody] CreateItemRequest request)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            return NotFound();
        }

        var validationError = ValidateItemInput(
            request.Name,
            request.Count,
            request.Price,
            request.Mod);

        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        var item = new Item
        {
            ServerId = serverId,
            Name = request.Name,
            Count = request.Count,
            Price = request.Price,
            Mod = request.Mod
        };

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        var itemDto = new ItemDto(
            item.Id,
            item.Name,
            item.Count,
            item.Price,
            item.Mod,
            item.PriceForOne);

        return CreatedAtAction(
            nameof(GetItem),
            new { serverId, id = item.Id },
            itemDto);
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
        var item = await _dbContext.Items
            .FirstOrDefaultAsync(item =>
                item.ServerId == serverId &&
                item.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        var validationError = ValidateItemInput(
            request.Name,
            request.Count,
            request.Price,
            request.Mod);

        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        item.Name = request.Name;
        item.Count = request.Count;
        item.Price = request.Price;
        item.Mod = request.Mod;

        await _dbContext.SaveChangesAsync();

        return Ok(new ItemDto(
            item.Id,
            item.Name,
            item.Count,
            item.Price,
            item.Mod,
            item.PriceForOne));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        [FromRoute] Guid serverId,
        [FromRoute] Guid id)
    {
        var item = await _dbContext.Items
            .FirstOrDefaultAsync(item =>
                item.ServerId == serverId &&
                item.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        _dbContext.Items.Remove(item);

        await _dbContext.SaveChangesAsync();

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
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Mod))
        {
            return BadRequest("Mod is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Lines))
        {
            return BadRequest("Lines are required.");
        }

        var existingMods = await _dbContext.Items
            .Where(item => item.ServerId == serverId)
            .Select(item => item.Mod)
            .Where(mod => !string.IsNullOrWhiteSpace(mod))
            .Distinct()
            .ToListAsync();

        if (!request.AllowNewMod && !existingMods.Contains(request.Mod))
        {
            return BadRequest($"Mod '{request.Mod}' does not exist for this server.");
        }

        var lines = request.Lines.Split(
            '\n',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var items = new List<Item>();
        var errors = new List<string>();

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            var lineNumber = index + 1;

            var parsedItem = ItemLineParser.TryParse(line);

            if (parsedItem is null)
            {
                errors.Add($"Line {lineNumber}: '{line}' has invalid format.");
                continue;
            }

            items.Add(new Item
            {
                ServerId = serverId,
                Name = parsedItem.Name,
                Count = parsedItem.Count,
                Price = parsedItem.Price,
                Mod = request.Mod
            });
        }

        if (items.Count > 0)
        {
            _dbContext.Items.AddRange(items);
            await _dbContext.SaveChangesAsync();
        }

        return Ok(new BulkCreateItemsResultDto(
            items.Count,
            errors.Count,
            errors));
    }


    private static string? ValidateItemInput(string name, int count, int price, string mod)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        if (string.IsNullOrWhiteSpace(mod))
        {
            return "Mod is required.";
        }

        if (count <= 0)
        {
            return "Count must be greater than zero.";
        }

        if (price <= 0)
        {
            return "Price must be greater than zero.";
        }

        return null;
    }
}
