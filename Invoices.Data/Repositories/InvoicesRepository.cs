using Invoices.Data.Context;
using Invoices.Data.Entities;

namespace Invoices.Data.Repositories;

internal sealed class InvoicesRepository : Repository<Invoice>, IInvoicesRepository
{
    public InvoicesRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}
