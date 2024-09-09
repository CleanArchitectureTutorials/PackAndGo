using Microsoft.EntityFrameworkCore;
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Domain.Aggregates.PackingListAggregate.Repositories;
using PackAndGo.Infrastructure.DataModels;
using PackAndGo.Infrastructure.Persistence;

namespace PackAndGo.Infrastructure.Repositories;

public class PackingListRepository : IPackingListRepository
{
    private readonly AppDbContext _context;

    public PackingListRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PackingList?> GetByIdAsync(Guid id)
    {
        var packingListDataModel = await _context.PackingLists
            .Include(pl => pl.Items)
            .SingleOrDefaultAsync(pl => pl.Id == id);

        return packingListDataModel?.ToDomain();
    }

    public async Task<IEnumerable<PackingList>> GetAllByOwnerIdAsync(Guid ownerId)
    {
        var packingListDataModels = await _context.PackingLists
            .Include(pl => pl.Items)
            .Where(pl => pl.UserId == ownerId)
            .ToListAsync();

        return packingListDataModels.Select(pl => pl.ToDomain()).ToList();
    }

    public async Task AddAsync(PackingList packingList)
    {
        var packingListDataModel = PackingListDataModel.FromDomain(packingList);
        _context.PackingLists.Add(packingListDataModel);
        await _context.SaveChangesAsync();
    }

public async Task UpdateAsync(PackingList packingList)
{
    var existingPackingList = await _context.PackingLists
        .Include(pl => pl.Items)
        .SingleOrDefaultAsync(pl => pl.Id == packingList.Id);

    if (existingPackingList != null)
    {
        // Update the existing PackingListDataModel
        existingPackingList.Name = packingList.Name;
        existingPackingList.UserId = packingList.OwnerId;

        // Remove items that are no longer in the updated list
        foreach (var existingItem in existingPackingList.Items?.ToList() ?? new List<ItemDataModel>())
        {
            if (!packingList.Items.Any(i => i.Id == existingItem.Id))
            {
                _context.Items.Remove(existingItem);
            }
        }

        // Add or update the items in the existing list
        foreach (var item in packingList.Items)
        {
            var existingItem = existingPackingList.Items?.SingleOrDefault(i => i.Id == item.Id);
            if (existingItem == null)
            {
                // Ensure EF Core tracks the new item correctly
                var newItemDataModel = ItemDataModel.FromDomain(item);
                _context.Entry(newItemDataModel).State = EntityState.Added;
                existingPackingList.Items?.Add(newItemDataModel);
            }
            else
            {
                existingItem.Name = item.Name;
                existingItem.IsPacked = item.IsPacked;
                _context.Entry(existingItem).State = EntityState.Modified;
            }
        }

        await _context.SaveChangesAsync();
    }
}

    public async Task DeleteAsync(Guid id)
    {
        var packingListDataModel = await _context.PackingLists
            .Include(pl => pl.Items)
            .SingleOrDefaultAsync(pl => pl.Id == id);

        if (packingListDataModel != null)
        {
            _context.PackingLists.Remove(packingListDataModel);
            await _context.SaveChangesAsync();
        }
    }
}
