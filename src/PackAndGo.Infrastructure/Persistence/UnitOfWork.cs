using PackAndGo.Application.Interfaces;

namespace PackAndGo.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync(); // Commit the transaction
    }

    public void Rollback()
    {
        // In EF Core, the transaction will automatically be rolled back if an exception is thrown.
        // You don't need an explicit rollback here unless you manually manage transactions.
    }
}