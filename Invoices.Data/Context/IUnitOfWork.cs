namespace Invoices.Data.Context;

public interface IUnitOfWork
{
    Task<int> Complete(CancellationToken cancellationToken = default);
}
