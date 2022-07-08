using System.Collections;

namespace EconomyViewer.Model;

internal class ItemList : IEnumerable<Item>, IList<Item>, ICollection<Item>
{
    private List<Item> _items;
    public Item this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public ItemList()
    {
        _items = new List<Item>();
    }
    public int Count { get => _items.Count; }
    public bool IsReadOnly => false;

    public void Add(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (_items.Any(i => i.Header == item.Header && i.Mod == item.Mod) == false)
            _items.Add(item.Clone());
        else
        {
            _items.First(i => i.Header == item.Header && i.Mod == item.Mod).IncreaseCount(item.Count);
        }
    }

    public void Clear()
    {
        _items.Clear();
    }

    public bool Contains(Item item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(Item[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public IEnumerator<Item> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    public int IndexOf(Item item)
    {
        return _items.IndexOf(item);
    }

    public void Insert(int index, Item item)
    {
        _items.Insert(index, item);
    }

    public bool Remove(Item item)
    {
        Item? toRemove = _items.Find(i => i.Header == item.Header && i.Mod == item.Mod);
        if (toRemove == null)
            return false;
        if (toRemove.Count == 1 || item.Count >= toRemove.Count)
            return _items.Remove(toRemove);

        _items.First(i => i.Equals(toRemove)).DecreaseCount(item.Count);
        return true;
    }

    public void RemoveAt(int index)
    {
        _items.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
