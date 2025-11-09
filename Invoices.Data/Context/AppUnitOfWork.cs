using Invoices.Data.Repositories;

namespace Invoices.Data.Context;

internal sealed class AppUnitOfWork : UnitOfWork, IAppUnitOfWork
{
    private IInvoicesRepository? invoicesRepository;
    private IVendorsRepository? vendorsRepository;

    public AppUnitOfWork(AppDbContext dbContext)
        : base(dbContext)
    { }

    public IInvoicesRepository Invoices =>
        this.invoicesRepository ??= new InvoicesRepository(this.dbContext);

    public IVendorsRepository Vendors =>
        this.vendorsRepository ??= new VendorsRepository(this.dbContext);
}