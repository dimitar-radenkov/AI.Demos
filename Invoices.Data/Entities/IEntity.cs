namespace Invoices.Data.Entities;

public interface IEntity
{
    public Guid Id { get; }
    public DateTime CreatedDate { get; }
    public DateTime UpdatedDate { get; }
}