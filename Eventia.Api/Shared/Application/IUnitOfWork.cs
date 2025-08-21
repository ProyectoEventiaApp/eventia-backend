namespace Shared.Application;

public interface IUnitOfWork
{
    Task CommitAsync();
}
