using EconomyViewerWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EconomyViewerWeb.Infrastructure.Configurations;

namespace EconomyViewerWeb.Infrastructure.Persistence;

public class EconomyViewerDbContext : DbContext
{
    public EconomyViewerDbContext(DbContextOptions<EconomyViewerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Server> Servers => Set<Server>();

    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ServerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ItemEntityConfiguration());

    }
}
