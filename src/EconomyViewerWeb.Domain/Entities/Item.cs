using EconomyViewerWeb.Domain.Enums;

namespace EconomyViewerWeb.Domain.Entities;
public class Item : BaseEntity
{
    public required string Name { get; set; }
    public int Count { get; set; }
    public int Price { get; set; }

    public required string Mod { get; set; }

    public int PriceForOne { get; set; }

    public ItemSource Source { get; set; }

    public Guid ServerId { get; set; }

    public Server Server { get; set; } = null!;

}
