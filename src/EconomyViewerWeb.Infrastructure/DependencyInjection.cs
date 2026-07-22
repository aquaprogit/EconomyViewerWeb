using EconomyViewerWeb.Application.Items;
using EconomyViewerWeb.Application.Servers;
using EconomyViewerWeb.Infrastructure.ForumSync;
using EconomyViewerWeb.Infrastructure.Items;
using EconomyViewerWeb.Infrastructure.Persistence;
using EconomyViewerWeb.Infrastructure.Servers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EconomyViewerWeb.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<EconomyViewerDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.Configure<ForumSyncOptions>(
            configuration.GetSection(ForumSyncOptions.SectionName));

        services.AddHttpClient();

        services.AddScoped<IForumSyncService, ForumSyncService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IServerService, ServerService>();

        return services;
    }
}
