namespace Invoices.Data.Entities;

public sealed class Invoice : IEntity
{
    public Guid Id { get; set; }
    public DateTime InvoiceDate { get; set; }
    public required string InvoiceNumber { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public Guid VendorId { get; set; }
    public required Vendor Vendor { get; set; }
}
