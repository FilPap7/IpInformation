using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IpInformation.Infrastructure.Persistence.Configurations;

internal sealed class IpAddressConfiguration : IEntityTypeConfiguration<IpAddress>
{
    public void Configure(EntityTypeBuilder<IpAddress> builder)
    {
        builder.ToTable("IpAddresses");

        builder.HasKey(i => i.Id);

        // varchar(15), unique
        builder.Property(i => i.Ip)
            .HasColumnName("IP")
            .HasMaxLength(IpAddress.IpMaxLength)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(i => i.Ip).IsUnique();

        // FK to Countries.Id without a navigation property: Country is another aggregate.
        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(i => i.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Ignore(i => i.DomainEvents);
    }
}
