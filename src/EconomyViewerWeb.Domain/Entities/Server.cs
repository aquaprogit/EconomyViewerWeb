namespace EconomyViewerWeb.Domain.Entities;

public class Server : BaseEntity
{
    public required string Name { get; set; }

    public ICollection<Item> Items { get; set; } = new List<Item>();

}

