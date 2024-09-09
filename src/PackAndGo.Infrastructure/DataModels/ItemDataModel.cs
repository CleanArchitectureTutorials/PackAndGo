using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Infrastructure.DataModels;

public class ItemDataModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsPacked { get; set; }

    // Mapping method to convert ItemDataModel to domain Item
    public Item ToDomain()
    {
        var item = Item.Load(Id, Name ?? string.Empty, IsPacked); // Load the item with the Id and Name
        return item;
    }

    // Mapping method to convert domain Item to ItemDataModel
    public static ItemDataModel FromDomain(Item item)
    {
        return new ItemDataModel
        {
            Id = item.Id,
            Name = item.Name,
            IsPacked = item.IsPacked
        };
    }
}
