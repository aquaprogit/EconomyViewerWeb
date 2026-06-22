namespace EconomyViewerWeb.Domain.Entities;

public class Server
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Item> Items { get; set; } = new List<Item>();

}
