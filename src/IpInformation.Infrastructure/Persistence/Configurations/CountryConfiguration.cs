using IpInformation.Domain.Countries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IpInformation.Infrastructure.Persistence.Configurations;

internal sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        // nvarchar(100): names may contain characters outside Latin-1.
        builder.Property(c => c.Name)
            .HasMaxLength(Country.NameMaxLength)
            .IsRequired();

        // char(2)
        builder.Property(c => c.TwoLetterCode)
            .HasMaxLength(Country.TwoLetterCodeLength)
            .IsFixedLength()
            .IsUnicode(false)
            .IsRequired();

        // char(3)
        builder.Property(c => c.ThreeLetterCode)
            .HasMaxLength(Country.ThreeLetterCodeLength)
            .IsFixedLength()
            .IsUnicode(false)
            .IsRequired();

        // datetime2; the default applies only to rows inserted outside the application.
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        // One row per country: the IP lookup finds countries by this code.
        builder.HasIndex(c => c.TwoLetterCode).IsUnique();

        builder.Ignore(c => c.DomainEvents);
    }
}
