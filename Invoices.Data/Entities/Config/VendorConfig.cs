using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoices.Data.Entities.Config;

internal sealed class VendorConfig : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.HasKey(v => v.Id);

        builder.HasIndex(v => v.TaxNumber).IsUnique();

        builder.Property(v => v.Name)
               .IsRequired()
               .HasMaxLength(200);
    }
}