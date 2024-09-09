using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Domain.UnitTests.Aggregates.PackingListAggregate.Entities;

public class PackingListTests
{
    [Fact]
    public void Create_ShouldReturnValidPackingList()
    {
        // Arrange
        var packingListName = "Vacation";
        var ownerId = Guid.NewGuid();

        // Act
        var packingList = PackingList.Create(packingListName, ownerId);

        // Assert
        Assert.NotNull(packingList);
        Assert.Equal(packingListName, packingList.Name);
        Assert.Equal(ownerId, packingList.OwnerId);
        Assert.Empty(packingList.Items);
        Assert.NotEqual(Guid.Empty, packingList.Id);
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PackingList.Create(null!, ownerId));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenOwnerIdIsEmpty()
    {
        // Arrange
        var packingListName = "Vacation";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PackingList.Create(packingListName, Guid.Empty));
    }

    [Fact]
    public void Load_ShouldReturnPackingListWithGivenProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var packingListName = "Vacation";
        var ownerId = Guid.NewGuid();
        var items = new List<Item> { Item.Create("Toothbrush") };

        // Act
        var packingList = PackingList.Load(id, packingListName, ownerId, items);

        // Assert
        Assert.NotNull(packingList);
        Assert.Equal(id, packingList.Id);
        Assert.Equal(packingListName, packingList.Name);
        Assert.Equal(ownerId, packingList.OwnerId);
        Assert.Equal(items.Count, packingList.Items.Count);
        Assert.Contains(packingList.Items, i => i.Name == "Toothbrush");
    }

    [Fact]
    public void ChangeName_ShouldUpdatePackingListName()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var newName = "Business Trip";

        // Act
        packingList.ChangeName(newName);

        // Assert
        Assert.Equal(newName, packingList.Name);
    }

    [Fact]
    public void ChangeName_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => packingList.ChangeName(null!));
    }

    [Fact]
    public void AddItem_ShouldAddNewItemToPackingList()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";

        // Act
        packingList.AddItem(itemName);

        // Assert
        Assert.Single(packingList.Items);
        Assert.Contains(packingList.Items, i => i.Name == itemName);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemFromPackingList()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";
        packingList.AddItem(itemName);
        var itemId = packingList.Items.First().Id;

        // Act
        packingList.RemoveItem(itemId);

        // Assert
        Assert.Empty(packingList.Items);
    }

    [Fact]
    public void ChangeItemName_ShouldUpdateItemName()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";
        packingList.AddItem(itemName);
        var itemId = packingList.Items.First().Id;
        var newItemName = "Hat";

        // Act
        packingList.ChangeItemName(itemId, newItemName);

        // Assert
        Assert.Contains(packingList.Items, i => i.Name == newItemName);
        Assert.DoesNotContain(packingList.Items, i => i.Name == itemName);
    }

    [Fact]
    public void MarkItemAsPacked_ShouldSetItemAsPacked()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";
        packingList.AddItem(itemName);
        var itemId = packingList.Items.First().Id;

        // Act
        packingList.MarkItemAsPacked(itemId);

        // Assert
        Assert.True(packingList.Items.First(i => i.Id == itemId).IsPacked);
    }

    [Fact]
    public void MarkItemAsUnpacked_ShouldSetItemAsUnpacked()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";
        packingList.AddItem(itemName);
        var itemId = packingList.Items.First().Id;

        // Act
        packingList.MarkItemAsPacked(itemId); // First, mark it as packed
        packingList.MarkItemAsUnpacked(itemId); // Then, mark it as unpacked

        // Assert
        Assert.False(packingList.Items.First(i => i.Id == itemId).IsPacked);
    }
}
