
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Domain.Aggregates.PackingListAggregate.Repositories;

public interface IPackingListRepository
{
    Task<PackingList?> GetByIdAsync(Guid id);
    Task<IEnumerable<PackingList>> GetAllByOwnerIdAsync(Guid ownerId);
    Task AddAsync(PackingList packingList);
    Task UpdateAsync(PackingList packingList);
    Task DeleteAsync(Guid id);
}
