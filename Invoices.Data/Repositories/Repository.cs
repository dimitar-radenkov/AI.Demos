using Invoices.Data.Context;
using Invoices.Data.Entities;

namespace Invoices.Data.Repositories;

internal abstract class Repository<T> : ReadOnlyRepository<T>, IRepository<T> where T : class, IEntity
{
    protected Repository(AppDbContext context) : base(context)
    {
    }

    public virtual async Task<T> Add(
        T entity,
        CancellationToken cancellationToken = default)
    {
        await this.dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task Update(
        T entity,
        CancellationToken cancellationToken = default)
    {
        this.dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task Delete(
        T entity,
        CancellationToken cancellationToken = default)
    {
        this.dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
