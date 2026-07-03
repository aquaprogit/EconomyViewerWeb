using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EconomyViewerWeb.Infrastructure.Persistence;
using EconomyViewerWeb.Api.Contracts.Servers;



namespace EconomyViewerWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServersController : ControllerBase
{
    private readonly EconomyViewerDbContext _dbContext;

    public ServersController(EconomyViewerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("minimal")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ServerMinimalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ServerMinimalDto>>> GetMinimalServers()
    {
        var servers = await _dbContext.Servers
            .OrderBy(server => server.Name)
            .Select(server => new ServerMinimalDto(
                server.Id,
                server.Name))
            .ToListAsync();

        return Ok(servers);
    }

    [HttpGet("{serverId:guid}/mods")]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<string>>> GetServerMods(
        [FromRoute] Guid serverId)
    {
        var serverExists = await _dbContext.Servers
            .AnyAsync(server => server.Id == serverId);

        if (!serverExists)
        {
            return NotFound();
        }

        var mods = await _dbContext.Items
            .Where(item => item.ServerId == serverId)
            .Select(item => item.Mod)
            .Where(mod => !string.IsNullOrWhiteSpace(mod))
            .Distinct()
            .OrderBy(mod => mod)
            .ToListAsync();

        return Ok(mods);

    }
}
