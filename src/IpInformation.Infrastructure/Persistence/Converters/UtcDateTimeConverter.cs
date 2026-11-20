using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IpInformation.Infrastructure.Persistence.Converters;

/// <summary>
/// Marks every <see cref="DateTime"/> read from the database as UTC.
/// </summary>
/// <remarks>
/// The application writes only UTC, but <c>datetime2</c> does not store the kind, so EF Core reads
/// values back as <see cref="DateTimeKind.Unspecified"/> and they would be serialized without a "Z".
/// Writes are passed through unchanged; the value itself never changes, only its kind.
/// </remarks>
internal sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    toDatabase => toDatabase,
    fromDatabase => DateTime.SpecifyKind(fromDatabase, DateTimeKind.Utc));
