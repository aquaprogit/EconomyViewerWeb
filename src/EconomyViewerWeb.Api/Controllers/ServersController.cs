using Microsoft.AspNetCore.Mvc;
using EconomyViewerWeb.Application.Contracts.Servers;
using EconomyViewerWeb.Application.Servers;


namespace EconomyViewerWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServersController : ControllerBase
{
    private readonly IServerService _serverService;

    public ServersController(IServerService serverService)
    {
        _serverService = serverService;
    }

    [HttpGet("minimal")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ServerMinimalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ServerMinimalDto>>> GetMinimalServers()
    {
        var servers = await _serverService.GetMinimalServersAsync();

        return Ok(servers);
    }

    [HttpGet("{serverId:guid}/mods")]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<string>>> GetServerMods(
        [FromRoute] Guid serverId)
    {
        var mods = await _serverService.GetServerModsAsync(serverId);

        if (mods is null)
        {
            return NotFound();
        }

        return Ok(mods);

    }
}
