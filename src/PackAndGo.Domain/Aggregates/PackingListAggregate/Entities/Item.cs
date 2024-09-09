using PackAndGo.Domain.Common;

namespace PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

public class Item : Entity
{
    public string Name { get; private set; }
    public bool IsPacked { get; private set; }

    private Item(Guid id, string name, bool isPacked)
    {
        Id = id;
        Name = name;
        IsPacked = isPacked;
    }

    public static Item Create(string name)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        return new Item(Guid.NewGuid(), name, false);
    }

    public static Item Load(Guid id, string name, bool isPacked)
    {
        var item = Item.Create(name);
        item.Id = id;
        item.IsPacked = isPacked;
        return item;
    }

    public void ChangeName(string name)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        Name = name;
    }
    
    public void MarkAsPacked()
    {
        IsPacked = true;
    }

    public void MarkAsUnpacked()
    {
        IsPacked = false;
    }
}
