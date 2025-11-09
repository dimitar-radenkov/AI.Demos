using Invoices.Data.Context;
using Invoices.Data.Entities;

namespace Invoices.Data.Repositories;

internal sealed class VendorsRepository : Repository<Vendor>, IVendorsRepository
{
    public VendorsRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}