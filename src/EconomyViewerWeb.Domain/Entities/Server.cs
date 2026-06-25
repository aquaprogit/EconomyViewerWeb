namespace EconomyViewerWeb.Domain.Entities;

public class Server : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Item> Items { get; set; } = new List<Item>();

}

