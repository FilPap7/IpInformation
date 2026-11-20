using FluentValidation;
using IpInformation.Application.Behaviors;
using IpInformation.Domain.Abstractions;
using MediatR;

namespace IpInformation.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private sealed record TestCommand(string Name) : IRequest<Result>;

    private sealed record TestQuery(string Name) : IRequest<Result<int>>;

    private static InlineValidator<T> NameNotEmpty<T>(Func<T, string> name)
    {
        var validator = new InlineValidator<T>();
        validator.RuleFor(x => name(x)).NotEmpty().WithErrorCode("Test.NameEmpty").WithMessage("Name is empty.");
        return validator;
    }

    [Fact]
    public async Task Handle_WhenNoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([]);
        var nextCalled = false;

        var result = await behavior.Handle(new TestCommand(""), _ => { nextCalled = true; return Task.FromResult(Result.Success()); }, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_CallsNext()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([NameNotEmpty<TestCommand>(c => c.Name)]);
        var nextCalled = false;

        var result = await behavior.Handle(new TestCommand("ok"), _ => { nextCalled = true; return Task.FromResult(Result.Success()); }, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsValidationErrorWithoutCallingNext()
    {
        var behavior = new ValidationBehavior<TestCommand, Result>([NameNotEmpty<TestCommand>(c => c.Name)]);
        var nextCalled = false;

        var result = await behavior.Handle(new TestCommand(""), _ => { nextCalled = true; return Task.FromResult(Result.Success()); }, CancellationToken.None);

        Assert.False(nextCalled);
        Assert.True(result.IsFailure);
        Assert.Equal(Error.Validation("Test.NameEmpty", "Name is empty."), result.Error);
    }

    [Fact]
    public async Task Handle_WhenValidationFailsForResultOfT_ReturnsFailedResultOfT()
    {
        var behavior = new ValidationBehavior<TestQuery, Result<int>>([NameNotEmpty<TestQuery>(q => q.Name)]);
        var nextCalled = false;

        var result = await behavior.Handle(new TestQuery(""), _ => { nextCalled = true; return Task.FromResult(Result.Success(42)); }, CancellationToken.None);

        Assert.False(nextCalled);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WhenSeveralValidatorsFail_ReturnsFirstFailure()
    {
        var second = new InlineValidator<TestCommand>();
        second.RuleFor(c => c.Name).MinimumLength(10).WithErrorCode("Test.NameShort");
        var behavior = new ValidationBehavior<TestCommand, Result>([NameNotEmpty<TestCommand>(c => c.Name), second]);

        var result = await behavior.Handle(new TestCommand(""), _ => Task.FromResult(Result.Success()), CancellationToken.None);

        Assert.Equal("Test.NameEmpty", result.Error.Code);
    }
}
