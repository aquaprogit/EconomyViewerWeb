namespace EconomyViewerWeb.Domain.Entities;
public class Item
{
    public int Id { get; set; }

    public string Header { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Price { get; set; }

    public string? Mod { get; set; }

    public int PriceForOne => Count == 0 ? 0 : Price / Count;

    public int ServerId { get; set; }

    public Server Server { get; set; } = null!;

}
