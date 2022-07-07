using System.Text.RegularExpressions;

namespace EconomyViewer.Model;

internal class Item : IEquatable<Item?>
{
    public Item()
    {
        Header = "";
        Count = 1;
        Price = 1;
        Mod = "";
    }

    public Item(string header, int count, int price, string mod, int id = 0)
    {
        ID = id;
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Count = count != 0 ? count : throw new ArgumentOutOfRangeException(nameof(count));
        Price = price != 0 ? price : throw new ArgumentOutOfRangeException(nameof(price));
        Mod = mod ?? throw new ArgumentNullException(nameof(mod));
    }

    public int ID { get; init; }
    public string Header { get; private set; }
    public int Count { get; set; }
    public int Price { get; set; }
    public string Mod { get; set; }

    public int OriginalPrice => Price / Count;

    public static Item? FromString(string value, string mod)
    {
        if (Regex.IsMatch(value, @"(.+)\s([0-9]+) шт. - ([0-9]+)$"))
        {
            GroupCollection groups = Regex.Match(value, @"(?<Header>.+)\s(?<Count>[0-9]+) шт. - (?<Price>[0-9]+)$").Groups;
            string header = groups["Header"].Value;
            while (header.StartsWith(" ") || header.StartsWith("\t"))
            {
                header = header.Remove(0, 1);
            }
            int count = Convert.ToInt32(groups["Count"].Value);
            int price = Convert.ToInt32(groups["Price"].Value);
            return new Item(header, count, price, mod);
        }
        return null;
    }
    public override string ToString()
    {
        return $"{Header} {Count} шт. - {Price}";
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Header, Count, Price, Mod);
    }
    public override bool Equals(object? obj)
    {
        return Equals(obj as Item);
    }
    public bool Equals(Item? other)
    {
        return other != null &&
               Header == other.Header &&
               Count == other.Count &&
               Price == other.Price &&
               Mod == other.Mod;
    }
}