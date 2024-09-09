using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Infrastructure.DataModels;

namespace PackAndGo.Infrastructure.UnitTests.DataModels;

public class ItemDataModelTests
{
    [Fact]
    public void ToDomain_ShouldReturnValidItem()
    {
        // Arrange
        var itemDataModel = new ItemDataModel
        {
            Id = Guid.NewGuid(),
            Name = "Toothbrush",
            IsPacked = false
        };

        // Act
        var item = itemDataModel.ToDomain();

        // Assert
        Assert.NotNull(item);
        Assert.Equal(itemDataModel.Id, item.Id);
        Assert.Equal(itemDataModel.Name, item.Name);
        Assert.Equal(itemDataModel.IsPacked, item.IsPacked);
    }

    [Fact]
    public void FromDomain_ShouldReturnValidItemDataModel()
    {
        // Arrange
        var item = Item.Create("Toothbrush");

        // Act
        var itemDataModel = ItemDataModel.FromDomain(item);

        // Assert
        Assert.NotNull(itemDataModel);
        Assert.Equal(item.Id, itemDataModel.Id);
        Assert.Equal(item.Name, itemDataModel.Name);
        Assert.Equal(item.IsPacked, itemDataModel.IsPacked);
    }
}
