using Invoices.Data.Repositories;

namespace Invoices.Data.Context;

public interface IAppUnitOfWork : IUnitOfWork
{
    IInvoicesRepository Invoices { get; }
    IVendorsRepository Vendors { get; }
}
