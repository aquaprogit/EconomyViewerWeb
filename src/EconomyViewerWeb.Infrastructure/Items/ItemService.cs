using AutoMapper;
using AutoMapper.QueryableExtensions;
using EconomyViewerWeb.Application.Items;
using EconomyViewerWeb.Infrastructure.Persistence;
using EconomyViewerWeb.Application.Contracts.Common;
using EconomyViewerWeb.Application.Contracts.Items;
using EconomyViewerWeb.Application.Exceptions;
using EconomyViewerWeb.Application.Parsing;
using EconomyViewerWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EconomyViewerWeb.Infrastructure.Items;

public class ItemService : IItemService
{
    private readonly EconomyViewerDbContext _dbContext;
    private readonly IMapper _mapper;

    public ItemService(
        EconomyViewerDbContext dbContext,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ItemDto>> GetItemsAsync(
        Guid serverId,
        string? mods,
        int page,
        int pageSize)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            throw new NotFoundException(
                $"Server with id '{serverId}' was not found.");
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
            .ProjectTo<ItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResultDto<ItemDto>(
            items,
            page,
            pageSize,
            totalCount);
    }

    public async Task<ItemDto> GetItemAsync(Guid serverId, Guid id)
    {
        var itemDto = await _dbContext.Items
            .Where(item => item.ServerId == serverId && item.Id == id)
            .ProjectTo<ItemDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (itemDto is null)
        {
            throw new NotFoundException(
                $"Item with id '{id}' was not found on server '{serverId}'.");
        }

        return itemDto;
    }

    public async Task<ItemDto> CreateItemAsync(
        Guid serverId,
        CreateItemRequest request)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            throw new NotFoundException(
                $"Server with id '{serverId}' was not found.");
        }

        var item = new Item
        {
            ServerId = serverId,
            Name = request.Name.Trim(),
            Count = request.Count,
            Price = request.Price,
            Mod = request.Mod.Trim()
        };

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<ItemDto>(item);
    }

    public async Task<ItemDto> UpdateItemAsync(
        Guid serverId,
        Guid id,
        UpdateItemRequest request)
    {
        var item = await _dbContext.Items
            .FirstOrDefaultAsync(item =>
                item.ServerId == serverId &&
                item.Id == id);

        if (item is null)
        {
            throw new NotFoundException(
                $"Item with id '{id}' was not found on server '{serverId}'.");
        }

        item.Name = request.Name.Trim();
        item.Count = request.Count;
        item.Price = request.Price;
        item.Mod = request.Mod.Trim();

        await _dbContext.SaveChangesAsync();

        return _mapper.Map<ItemDto>(item);
    }

    public async Task DeleteItemAsync(Guid serverId, Guid id)
    {
        var item = await _dbContext.Items
            .FirstOrDefaultAsync(item =>
                item.ServerId == serverId &&
                item.Id == id);

        if (item is null)
        {
            throw new NotFoundException(
                $"Item with id '{id}' was not found on server '{serverId}'.");
        }

        _dbContext.Items.Remove(item);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<BulkCreateItemsResultDto> BulkCreateItemsAsync(
    Guid serverId,
    BulkCreateItemsRequest request)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            throw new NotFoundException(
                $"Server with id '{serverId}' was not found.");
        }

        var normalizedMod = request.Mod.Trim();

        var modExists = await _dbContext.Items
            .AnyAsync(item =>
                item.ServerId == serverId &&
                item.Mod != null &&
                EF.Functions.Collate(
                    item.Mod,
                    "Latin1_General_100_CI_AS") == normalizedMod);

        if (!request.AllowNewMod && !modExists)
        {
            throw new ValidationException(
                $"Mod '{normalizedMod}' does not exist for this server.");
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
                Mod = normalizedMod
            });
        }

        if (items.Count > 0)
        {
            _dbContext.Items.AddRange(items);
            await _dbContext.SaveChangesAsync();
        }

        var resultDto = new BulkCreateItemsResultDto(
        items.Count,
        errors.Count,
        errors);

        return resultDto;
    }

}
