using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Infrastructure.Repositories;
using PackAndGo.Infrastructure.IntegrationTests.Fixtures;

namespace PackAndGo.Infrastructure.IntegrationTests.Repositories
{
    public class PackingListRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly PackingListRepository _repository;

        public PackingListRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _repository = new PackingListRepository(_fixture.Context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddPackingListToDatabase()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Toothbrush");

            // Act
            await _repository.AddAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Vacation", result.Name);
            Assert.Single(result.Items);
            Assert.Equal("Toothbrush", result.Items.First().Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPackingListWithItems()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Toothbrush");

            await _repository.AddAsync(packingList);

            // Act
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Vacation", result.Name);
            Assert.Single(result.Items);
            Assert.Equal("Toothbrush", result.Items.First().Name);
        }

        [Fact]
        public async Task GetAllByOwnerIdAsync_ShouldReturnAllPackingListsForOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var packingList1 = PackingList.Create("Vacation", ownerId);
            var packingList2 = PackingList.Create("Business Trip", ownerId);
            await _repository.AddAsync(packingList1);
            await _repository.AddAsync(packingList2);

            // Act
            var results = await _repository.GetAllByOwnerIdAsync(ownerId);

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Contains(results, pl => pl.Name == "Vacation");
            Assert.Contains(results, pl => pl.Name == "Business Trip");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdatePackingListName()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            await _repository.AddAsync(packingList);

            // Act
            packingList.ChangeName("Updated Vacation");
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Vacation", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAddNewItemToPackingList()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            await _repository.AddAsync(packingList);

            // Act
            packingList.AddItem("Sunglasses");
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Contains(result.Items, i => i.Name == "Sunglasses");
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingItem()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Sunglasses");
            await _repository.AddAsync(packingList);
            var itemId = packingList.Items.First().Id;

            // Act
            packingList.ChangeItemName(itemId, "Updated Sunglasses");
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Updated Sunglasses", result.Items.First().Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldRemoveItemFromPackingList()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Sunglasses");
            await _repository.AddAsync(packingList);

            // Act
            var itemId = packingList.Items.First().Id;
            packingList.RemoveItem(itemId);
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task UpdateAsync_ShouldHandleMultipleChanges()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Sunglasses");
            packingList.AddItem("Hat");
            await _repository.AddAsync(packingList);

            // Act
            var sunglassesId = packingList.Items.First(i => i.Name == "Sunglasses").Id;
            packingList.ChangeItemName(sunglassesId, "Updated Sunglasses");
            packingList.RemoveItem(sunglassesId);
            packingList.AddItem("Shoes");
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.DoesNotContain(result.Items, i => i.Name == "Sunglasses");
            Assert.Contains(result.Items, i => i.Name == "Hat");
            Assert.Contains(result.Items, i => i.Name == "Shoes");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemovePackingListFromDatabase()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            await _repository.AddAsync(packingList);

            // Act
            await _repository.DeleteAsync(packingList.Id);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.Null(result);
        }
    }
}
