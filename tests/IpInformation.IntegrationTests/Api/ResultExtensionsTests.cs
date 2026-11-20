using IpInformation.Api.Extensions;
using IpInformation.Domain.Abstractions;

namespace IpInformation.IntegrationTests.Api;

public class ResultExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Failure, 500)]
    public void ToProblem_WhenFailed_MapsTheErrorTypeToTheStatusCode(ErrorType type, int expectedStatusCode)
    {
        var result = Result.Failure(new Error("Test.Code", "Something went wrong.", type));

        var problem = result.ToProblem();

        Assert.Equal(expectedStatusCode, problem.StatusCode);
        Assert.Equal(expectedStatusCode, problem.ProblemDetails.Status);
    }

    [Fact]
    public void ToProblem_WhenFailed_PutsTheDescriptionInDetailAndTheCodeInExtensions()
    {
        var result = Result.Failure<int>(Error.NotFound("IpInformation.NotFound", "No information was found for this IP."));

        var problem = result.ToProblem();

        Assert.Equal("No information was found for this IP.", problem.ProblemDetails.Detail);
        Assert.Equal("IpInformation.NotFound", problem.ProblemDetails.Extensions["code"]);
        Assert.False(string.IsNullOrEmpty(problem.ProblemDetails.Title));
    }

    [Fact]
    public void ToProblem_WhenSuccessful_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Success().ToProblem());
    }
}
