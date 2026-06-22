using EconomyViewerWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(server => server.Id);

            entity.Property(server => server.Name)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasMany(server => server.Items)
                .WithOne(item => item.Server)
                .HasForeignKey(item => item.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Header)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(item => item.Mod)
                .HasMaxLength(20);

            entity.Ignore(item => item.PriceForOne);

        });
    }
}
