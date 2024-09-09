using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Domain.UnitTests.Aggregates.PackingListAggregate.Entities;

public class ItemTests
{
    [Fact]
    public void Create_ShouldReturnValidItem()
    {
        // Arrange
        var itemName = "Toothbrush";

        // Act
        var item = Item.Create(itemName);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(itemName, item.Name);
        Assert.False(item.IsPacked);
        Assert.NotEqual(Guid.Empty, item.Id);
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Item.Create(null!));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenNameIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Item.Create(string.Empty));
    }

    [Fact]
    public void Load_ShouldReturnItemWithGivenProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var itemName = "Shampoo";
        var isPacked = true;

        // Act
        var item = Item.Load(id, itemName, isPacked);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(id, item.Id);
        Assert.Equal(itemName, item.Name);
        Assert.Equal(isPacked, item.IsPacked);
    }

    [Fact]
    public void ChangeName_ShouldUpdateName()
    {
        // Arrange
        var item = Item.Create("Toothbrush");
        var newName = "Shampoo";

        // Act
        item.ChangeName(newName);

        // Assert
        Assert.Equal(newName, item.Name);
    }

    [Fact]
    public void ChangeName_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        var item = Item.Create("Toothbrush");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => item.ChangeName(null!));
    }

    [Fact]
    public void ChangeName_ShouldThrowArgumentNullException_WhenNameIsEmpty()
    {
        // Arrange
        var item = Item.Create("Toothbrush");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => item.ChangeName(string.Empty));
    }

    [Fact]
    public void MarkAsPacked_ShouldSetIsPackedToTrue()
    {
        // Arrange
        var item = Item.Create("Toothbrush");

        // Act
        item.MarkAsPacked();

        // Assert
        Assert.True(item.IsPacked);
    }

    [Fact]
    public void MarkAsUnpacked_ShouldSetIsPackedToFalse()
    {
        // Arrange
        var item = Item.Create("Toothbrush");
        item.MarkAsPacked();

        // Act
        item.MarkAsUnpacked();

        // Assert
        Assert.False(item.IsPacked);
    }
}
