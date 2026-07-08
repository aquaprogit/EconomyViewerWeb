using EconomyViewerWeb.Application.Contracts.Servers;
using EconomyViewerWeb.Application.Servers;
using EconomyViewerWeb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EconomyViewerWeb.Infrastructure.Servers;

public class ServerService : IServerService
{
    private readonly EconomyViewerDbContext _dbContext;

    public ServerService(EconomyViewerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ServerMinimalDto>> GetMinimalServersAsync()
    {
        return await _dbContext.Servers
            .OrderBy(server => server.Name)
            .Select(server => new ServerMinimalDto(
                server.Id,
                server.Name))
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<string>?> GetServerModsAsync(Guid serverId)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            return null;
        }

        return await _dbContext.Items
            .Where(item => item.ServerId == serverId)
            .Select(item => item.Mod!)
            .Where(mod => !string.IsNullOrWhiteSpace(mod))
            .Distinct()
            .OrderBy(mod => mod)
            .ToListAsync();
    }
}
