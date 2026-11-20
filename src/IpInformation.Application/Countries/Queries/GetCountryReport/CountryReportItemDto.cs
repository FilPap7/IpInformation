namespace IpInformation.Application.Countries.Queries.GetCountryReport;

/// <param name="LastAddressUpdated">The latest <c>UpdatedAt</c> of the IPs of this country (UTC).</param>
public sealed record CountryReportItemDto(string CountryName, int AddressesCount, DateTime LastAddressUpdated);
