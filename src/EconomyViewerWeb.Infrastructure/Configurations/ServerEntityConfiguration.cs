using EconomyViewerWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace EconomyViewerWeb.Infrastructure.Configurations;

 internal class ServerEntityConfiguration : IEntityTypeConfiguration<Server>
{
    public void Configure(EntityTypeBuilder<Server> builder)
    {
        builder.ToTable("Servers")
            .HasKey(server => server.Id);

        builder.Property(server => server.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasMany(server => server.Items)
            .WithOne(item => item.Server)
            .HasForeignKey(item => item.ServerId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}


