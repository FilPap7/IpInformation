using IpInformation.Application.Countries.Queries.GetCountryReport;
using IpInformation.Application.Tests.Fakes;

namespace IpInformation.Application.Tests.Countries.Queries.GetCountryReport;

public class GetCountryReportHandlerTests
{
    private readonly FakeCountryReportReader _reader = new();
    private readonly GetCountryReportHandler _handler;

    public GetCountryReportHandlerTests()
    {
        _handler = new GetCountryReportHandler(_reader);
    }

    [Fact]
    public async Task Handle_Always_ReturnsTheReportOfTheReader()
    {
        _reader.Report = [new CountryReportItemDto("Greece", 3, new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc))];

        var result = await _handler.Handle(new GetCountryReportQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(_reader.Report, result.Value);
    }

    [Fact]
    public async Task Handle_WhenCodesAreNull_AsksForAllCountries()
    {
        await _handler.Handle(new GetCountryReportQuery(null), CancellationToken.None);

        Assert.Null(_reader.ReceivedTwoLetterCodes);
    }

    [Fact]
    public async Task Handle_WhenCodesAreEmpty_AsksForAllCountries()
    {
        await _handler.Handle(new GetCountryReportQuery([]), CancellationToken.None);

        Assert.Null(_reader.ReceivedTwoLetterCodes);
    }

    [Fact]
    public async Task Handle_WhenCodesAreGiven_PassesThemUpperCaseWithoutDuplicates()
    {
        await _handler.Handle(new GetCountryReportQuery(["gr", "US", "Gr"]), CancellationToken.None);

        Assert.Equal(["GR", "US"], _reader.ReceivedTwoLetterCodes);
    }
}
