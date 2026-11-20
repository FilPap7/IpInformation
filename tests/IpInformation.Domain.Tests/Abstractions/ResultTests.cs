using IpInformation.Domain.Abstractions;

namespace IpInformation.Domain.Tests.Abstractions;

public class ResultTests
{
    private static readonly Error SomeError = Error.Validation("Test.Invalid", "Something is invalid.");

    [Fact]
    public void Success_WhenCreated_IsSuccessWithNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_WhenCreated_IsFailureWithError()
    {
        var result = Result.Failure(SomeError);

        Assert.True(result.IsFailure);
        Assert.Equal(SomeError, result.Error);
    }

    [Fact]
    public void Failure_WhenErrorIsNone_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void SuccessOfT_WhenCreated_ExposesValue()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void FailureOfT_WhenValueAccessed_ThrowsInvalidOperationException()
    {
        var result = Result.Failure<int>(SomeError);

        Assert.True(result.IsFailure);
        Assert.Equal(SomeError, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Theory]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    public void ErrorFactory_WhenCalled_SetsCodeDescriptionAndType(ErrorType type)
    {
        var error = type switch
        {
            ErrorType.Failure => Error.Failure("Code", "Description"),
            ErrorType.Validation => Error.Validation("Code", "Description"),
            ErrorType.NotFound => Error.NotFound("Code", "Description"),
            ErrorType.Conflict => Error.Conflict("Code", "Description"),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

        Assert.Equal(new Error("Code", "Description", type), error);
    }
}
