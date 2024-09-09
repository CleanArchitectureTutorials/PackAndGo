using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Infrastructure.DataModels;

public class PackingListDataModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid UserId { get; set; }
    public ICollection<ItemDataModel>? Items { get; set; }

    // Mapping method to convert PackingListDataModel to domain PackingList
    public PackingList ToDomain()
    {
        // Convert each ItemDataModel to Item domain entity
        var items = Items?.Select(i => i.ToDomain()).ToList() ?? new List<Item>();

        // Create the PackingList domain entity
        var packingList = PackingList.Load(
            Id,
            Name ?? string.Empty,  // Ensure Name is non-null
            UserId,
            items
        );

        return packingList;
    }

    // Mapping method to convert domain PackingList to PackingListDataModel
    public static PackingListDataModel FromDomain(PackingList packingList)
    {
        // Convert each Item domain entity to ItemDataModel
        var itemDataModels = packingList.Items.Select(ItemDataModel.FromDomain).ToList();

        return new PackingListDataModel
        {
            Id = packingList.Id,
            Name = packingList.Name,
            UserId = packingList.OwnerId,
            Items = itemDataModels
        };
    }
}
