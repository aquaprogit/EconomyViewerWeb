using EconomyViewerWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace EconomyViewerWeb.Infrastructure.Configurations;

internal class ItemEntityConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items")
            .HasKey(item => item.Id);

        builder.Property(item => item.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(item => item.Mod)
            .HasMaxLength(150);

        builder.Property(item => item.PriceForOne)
            .HasComputedColumnSql("CASE WHEN [Count] = 0 THEN 0 ELSE [Price] / [Count] END");

    }
}
