namespace Invoices.Data.Context;

internal abstract class UnitOfWork : IUnitOfWork
{
    protected readonly AppDbContext dbContext;

    protected UnitOfWork(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<int> Complete(CancellationToken cancellationToken = default)
    {
        return await this.dbContext.SaveChangesAsync(cancellationToken);
    }
}
