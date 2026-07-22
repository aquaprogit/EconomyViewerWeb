using EconomyViewerWeb.Application.Contracts.Servers;

namespace EconomyViewerWeb.Application.Servers;

public interface IServerService
{
    Task<IReadOnlyCollection<ServerMinimalDto>> GetMinimalServersAsync();

    Task<IReadOnlyCollection<string>> GetServerModsAsync(Guid serverId);
}
