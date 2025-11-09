using Invoices.Data.Entities;

namespace Invoices.Data.Repositories;

public interface IRepository<T> : IReadOnlyRepository<T> where T : class, IEntity
{
    Task<T> Add(
        T entity,
        CancellationToken cancellationToken = default);

    Task Update(
        T entity,
        CancellationToken cancellationToken = default);

    Task Delete(
        T entity,
        CancellationToken cancellationToken = default);
}
