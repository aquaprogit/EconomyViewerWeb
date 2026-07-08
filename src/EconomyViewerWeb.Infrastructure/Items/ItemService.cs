using EconomyViewerWeb.Application.Items;
using EconomyViewerWeb.Infrastructure.Persistence;
using EconomyViewerWeb.Application.Contracts.Common;
using EconomyViewerWeb.Application.Contracts.Items;
using EconomyViewerWeb.Application.Parsing;
using EconomyViewerWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EconomyViewerWeb.Infrastructure.Items;

public class ItemService : IItemService
{
    private readonly EconomyViewerDbContext _dbContext;

    public ItemService(EconomyViewerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<ItemDto>?> GetItemsAsync(
        Guid serverId,
        string? mods,
        int page,
        int pageSize)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            return null;
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

        return new PagedResultDto<ItemDto>(
            items,
            page,
            pageSize,
            totalCount);
    }

    public async Task<ItemDto?> GetItemAsync(Guid serverId, Guid id)
    {
        return await _dbContext.Items
            .Where(item => item.ServerId == serverId && item.Id == id)
            .Select(item => new ItemDto(
                item.Id,
                item.Name,
                item.Count,
                item.Price,
                item.Mod,
                item.PriceForOne))
            .FirstOrDefaultAsync();
    }

    public async Task<CreateItemResult> CreateItemAsync(
        Guid serverId,
        CreateItemRequest request)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            return new CreateItemResult(
                CreateItemStatus.ServerNotFound,
                null,
                null);
        }

        var validationError = ValidateItemInput(
            request.Name,
            request.Count,
            request.Price,
            request.Mod);

        if (validationError is not null)
        {
            return new CreateItemResult(
                CreateItemStatus.ValidationError,
                null,
                validationError);
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

        return new CreateItemResult(
            CreateItemStatus.Success,
            itemDto,
            null);
    }

    public async Task<UpdateItemResult> UpdateItemAsync(
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
            return new UpdateItemResult(
                UpdateItemStatus.ItemNotFound,
                null,
                null);
        }

        var validationError = ValidateItemInput(
            request.Name,
            request.Count,
            request.Price,
            request.Mod);

        if (validationError is not null)
        {
            return new UpdateItemResult(
                UpdateItemStatus.ValidationError,
                null,
                validationError);
        }

        item.Name = request.Name;
        item.Count = request.Count;
        item.Price = request.Price;
        item.Mod = request.Mod;

        await _dbContext.SaveChangesAsync();

        var itemDto = new ItemDto(
            item.Id,
            item.Name,
            item.Count,
            item.Price,
            item.Mod,
            item.PriceForOne);

        return new UpdateItemResult(
            UpdateItemStatus.Success,
            itemDto,
            null);
    }

    public async Task<DeleteItemStatus> DeleteItemAsync(Guid serverId, Guid id)
    {
        var item = await _dbContext.Items
            .FirstOrDefaultAsync(item =>
                item.ServerId == serverId &&
                item.Id == id);

        if (item is null)
        {
            return DeleteItemStatus.ItemNotFound;
        }

        _dbContext.Items.Remove(item);

        await _dbContext.SaveChangesAsync();

        return DeleteItemStatus.Success;
    }

    public async Task<BulkCreateItemsResult> BulkCreateItemsAsync(
    Guid serverId,
    BulkCreateItemsRequest request)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            return new BulkCreateItemsResult(
                BulkCreateItemsStatus.ServerNotFound,
                null,
                null);
        }

        if (string.IsNullOrWhiteSpace(request.Mod))
        {
            return new BulkCreateItemsResult(
                BulkCreateItemsStatus.ValidationError,
                null,
                "Mod is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Lines))
        {
            return new BulkCreateItemsResult(
                BulkCreateItemsStatus.ValidationError,
                null,
                "Lines are required.");
        }

        var existingMods = await _dbContext.Items
        .Where(item => item.ServerId == serverId)
        .Select(item => item.Mod)
        .Where(mod => !string.IsNullOrWhiteSpace(mod))
        .Distinct()
        .ToListAsync();

        if (!request.AllowNewMod && !existingMods.Contains(request.Mod))
        {
            return new BulkCreateItemsResult(
                BulkCreateItemsStatus.ValidationError,
                null,
                $"Mod '{request.Mod}' does not exist for this server.");
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

        var resultDto = new BulkCreateItemsResultDto(
        items.Count,
        errors.Count,
        errors);

        return new BulkCreateItemsResult(
        BulkCreateItemsStatus.Success,
        resultDto,
        null);
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
