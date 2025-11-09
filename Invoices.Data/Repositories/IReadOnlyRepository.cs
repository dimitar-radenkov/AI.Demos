using Invoices.Data.Entities;
using System.Linq.Expressions;

namespace Invoices.Data.Repositories;

public interface IReadOnlyRepository<T> where T : class, IEntity
{
    Task<TResult?> GetById<TResult>(
        Guid id,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);

    Task<T?> GetById(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TResult>> Find<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TResult>> Find<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
        CancellationToken cancellationToken = default);
}