using System;
using System.Linq;
using System.Threading.Tasks;
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Infrastructure.Repositories;
using PackAndGo.Infrastructure.IntegrationTests.RealDb.Fixtures;
using Xunit;
using System.Runtime.CompilerServices;

namespace PackAndGo.Infrastructure.IntegrationTests.RealDb.Repositories
{
    [Collection("Sequential Test Collection")] // Ensures the tests run sequentially
    public class PackingListRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _dbFixture;
        private readonly PackingListRepository _packingListRepository;

        public PackingListRepositoryTests(DatabaseFixture fixture)
        {
            _dbFixture = fixture;
            _packingListRepository = new PackingListRepository(_dbFixture.Context);
        }

        private static void PauseTest([CallerMemberName] string methodName = "")
        {
            // Check if the environment variable is set
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ENABLE_MANUAL_TEST_PAUSE")))
            {
                return;  // Skip pausing if the environment variable is not set
            }

            // Pause the test and display the method name
            Console.WriteLine($"Method: {methodName}");
            Console.WriteLine("Test paused. Press Enter to continue...");
            Console.ReadLine();
        }

        [RealDatabaseFact]
        public async Task AddAsync_ShouldAddPackingListToDatabase()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());

            // Act
            await _packingListRepository.AddAsync(packingList);
            var result = await _packingListRepository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Vacation", result.Name);
            Assert.Empty(result.Items);

            PauseTest();
        }

        [RealDatabaseFact]
        public async Task GetByIdAsync_ShouldReturnPackingList_WhenItExists()
        {
            // Arrange
            var packingList = PackingList.Create("Business Trip", Guid.NewGuid());
            await _packingListRepository.AddAsync(packingList);

            // Act
            var result = await _packingListRepository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packingList.Id, result.Id);
            Assert.Equal("Business Trip", result.Name);
            PauseTest();
        }

        [RealDatabaseFact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenPackingListDoesNotExist()
        {
            // Act
            var nonExistingPackingListId = Guid.NewGuid();
            var result = await _packingListRepository.GetByIdAsync(nonExistingPackingListId);

            // Assert
            Assert.Null(result);
            PauseTest();
        }

        [RealDatabaseFact]
        public async Task GetAllByOwnerIdAsync_ShouldReturnAllPackingListsForOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var packingList1 = PackingList.Create("List 1", ownerId);
            var packingList2 = PackingList.Create("List 2", ownerId);
            await _packingListRepository.AddAsync(packingList1);
            await _packingListRepository.AddAsync(packingList2);

            // Act
            var packingLists = await _packingListRepository.GetAllByOwnerIdAsync(ownerId);

            // Assert
            Assert.NotNull(packingLists);
            Assert.Equal(2, packingLists.Count());
            Assert.Contains(packingLists, pl => pl.Name == "List 1");
            Assert.Contains(packingLists, pl => pl.Name == "List 2");
            PauseTest();
        }

        [RealDatabaseFact]
        public async Task UpdateAsync_ShouldUpdatePackingListAndItems()
        {
            // Arrange
            var packingList = PackingList.Create("Old Name", Guid.NewGuid());
            packingList.AddItem("Old Item");
            await _packingListRepository.AddAsync(packingList);
            PauseTest();

            // Act
            packingList.ChangeName("New Name");
            packingList.ChangeItemName(packingList.Items.First().Id, "New Item");
            packingList.AddItem("Second Item");
            await _packingListRepository.UpdateAsync(packingList);
            var result = await _packingListRepository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            Assert.Equal(2, result.Items.Count);
            Assert.Contains(result.Items, i => i.Name == "New Item");
            Assert.Contains(result.Items, i => i.Name == "Second Item");
            PauseTest();
        }

        [RealDatabaseFact]
        public async Task DeleteAsync_ShouldRemovePackingListFromDatabase()
        {
            // Arrange
            var packingList = PackingList.Create("List to Delete", Guid.NewGuid());
            await _packingListRepository.AddAsync(packingList);
            PauseTest();

            // Act
            await _packingListRepository.DeleteAsync(packingList.Id);
            var result = await _packingListRepository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.Null(result);
            PauseTest();
        }
    }
}
