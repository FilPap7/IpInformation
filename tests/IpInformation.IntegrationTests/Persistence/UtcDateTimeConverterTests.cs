using IpInformation.Infrastructure.Persistence;
using IpInformation.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace IpInformation.IntegrationTests.Persistence;

public class UtcDateTimeConverterTests
{
    private static readonly DateTime Value = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Unspecified);

    private readonly UtcDateTimeConverter _converter = new();

    [Fact]
    public void ConvertFromProvider_Always_ReturnsTheSameValueMarkedAsUtc()
    {
        var read = (DateTime)_converter.ConvertFromProvider(Value)!;

        Assert.Equal(DateTimeKind.Utc, read.Kind);
        Assert.Equal(Value.Ticks, read.Ticks);
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ConvertToProvider_Always_WritesTheValueUnchanged(DateTimeKind kind)
    {
        var value = DateTime.SpecifyKind(Value, kind);

        var written = (DateTime)_converter.ConvertToProvider(value)!;

        Assert.Equal(value.Ticks, written.Ticks);
    }

    [Theory]
    [InlineData("Countries", "CreatedAt")]
    [InlineData("IpAddresses", "CreatedAt")]
    [InlineData("IpAddresses", "UpdatedAt")]
    public void Model_EveryDateTimeProperty_UsesTheUtcConverter(string table, string column)
    {
        // Building the model needs no database connection.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer("Server=unused").Options;
        using var context = new ApplicationDbContext(options);

        var property = context.Model.GetEntityTypes()
            .Single(e => e.GetTableName() == table)
            .GetProperties()
            .Single(p => p.GetColumnName() == column);

        Assert.IsType<UtcDateTimeConverter>(property.GetValueConverter());
    }
}
