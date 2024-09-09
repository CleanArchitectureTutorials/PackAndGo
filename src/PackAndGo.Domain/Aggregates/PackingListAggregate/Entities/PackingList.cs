using PackAndGo.Domain.Common;

namespace PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

public class PackingList : Entity
{
    public string Name { get; private set; }
    public Guid OwnerId { get; private set; }
    public ICollection<Item> Items { get; private set; }

    private PackingList(Guid id, string name, Guid ownerId, IEnumerable<Item> items)
    {
        Id = id;
        Name = name;
        OwnerId = ownerId;
        Items = items.ToList();
    }

    public static PackingList Create(string name, Guid ownerId)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        // Validate ownerId
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(ownerId));
        }
        return new PackingList(Guid.NewGuid(), name, ownerId, new List<Item>());
    }

    public static PackingList Load(Guid id, string name, Guid ownerId, IEnumerable<Item> items)
    {
        var packingList = new PackingList(id, name, ownerId, items);
        return packingList;
    }

    public void ChangeName(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void AddItem(string itemName)
    {
        var item = Item.Create(itemName);
        Items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        var item = Items.SingleOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public void ChangeItemName(Guid itemId, string name)
    {
        var item = Items.SingleOrDefault(i => i.Id == itemId);
        item?.ChangeName(name);
    }

    public void MarkItemAsPacked(Guid itemId)
    {
        var item = Items.SingleOrDefault(i => i.Id == itemId);
        item?.MarkAsPacked();
    }

    public void MarkItemAsUnpacked(Guid itemId)
    {
        var item = Items.SingleOrDefault(i => i.Id == itemId);
        item?.MarkAsUnpacked();
    }
}