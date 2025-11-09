namespace Invoices.Data.Entities;

public sealed class Vendor : IEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string TaxNumber { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}