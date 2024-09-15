namespace PackAndGo.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
    void Rollback(); 
}
