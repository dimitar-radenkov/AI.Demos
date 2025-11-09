using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoices.Data.Entities.Config;

internal sealed class InvoiceConfig : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();

        builder.HasOne(i => i.Vendor)
               .WithMany()
               .HasForeignKey(i => i.VendorId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}