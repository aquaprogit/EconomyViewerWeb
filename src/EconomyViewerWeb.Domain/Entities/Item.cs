namespace EconomyViewerWeb.Domain.Entities;
public class Item : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Price { get; set; }

    public string? Mod { get; set; }

    public decimal PriceForOne { get; set; }

    public int ServerId { get; set; }

    public Server Server { get; set; } = null!;

}
