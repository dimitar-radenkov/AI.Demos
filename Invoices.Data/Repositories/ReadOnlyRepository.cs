using Invoices.Data.Context;
using Invoices.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Invoices.Data.Repositories;

internal class ReadOnlyRepository<T> : IReadOnlyRepository<T> where T : class, IEntity
{
    protected readonly AppDbContext context;
    protected readonly DbSet<T> dbSet;

    protected ReadOnlyRepository(AppDbContext context)
    {
        this.context = context;
        this.dbSet = context.Set<T>();
    }

    public async Task<TResult?> GetById<TResult>(
        Guid id,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await this.dbSet
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(selector)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<T?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await this.dbSet
            .AsNoTracking()
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TResult>> Find<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await this.dbSet
            .AsNoTracking()
            .Where(predicate)
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TResult>> Find<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
        CancellationToken cancellationToken = default)
    {
        var query = this.dbSet
            .AsNoTracking()
            .Where(predicate);

        query = orderBy(query);

        return await query.Select(selector).ToListAsync(cancellationToken);
    }
}
