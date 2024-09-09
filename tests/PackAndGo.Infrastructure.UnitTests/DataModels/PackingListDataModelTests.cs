using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Infrastructure.DataModels;

namespace PackAndGo.Infrastructure.UnitTests.DataModels;

public class PackingListDataModelTests
{
    [Fact]
    public void ToDomain_ShouldReturnValidPackingList()
    {
        // Arrange
        var packingListDataModel = new PackingListDataModel
        {
            Id = Guid.NewGuid(),
            Name = "Vacation",
            UserId = Guid.NewGuid(),
            Items = new List<ItemDataModel>
            {
                new ItemDataModel { Id = Guid.NewGuid(), Name = "Toothbrush", IsPacked = false }
            }
        };

        // Act
        var packingList = packingListDataModel.ToDomain();

        // Assert
        Assert.NotNull(packingList);
        Assert.Equal(packingListDataModel.Id, packingList.Id);
        Assert.Equal(packingListDataModel.Name, packingList.Name);
        Assert.Equal(packingListDataModel.UserId, packingList.OwnerId);
        Assert.Single(packingList.Items);
    }

    [Fact]
    public void FromDomain_ShouldReturnValidPackingListDataModel()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        packingList.AddItem("Toothbrush");

        // Act
        var packingListDataModel = PackingListDataModel.FromDomain(packingList);

        // Assert
        Assert.NotNull(packingListDataModel);
        Assert.Equal(packingList.Id, packingListDataModel.Id);
        Assert.Equal(packingList.Name, packingListDataModel.Name);
        Assert.Equal(packingList.OwnerId, packingListDataModel.UserId);
        Assert.NotNull(packingListDataModel.Items);
        Assert.Single(packingListDataModel.Items);
    }
}
