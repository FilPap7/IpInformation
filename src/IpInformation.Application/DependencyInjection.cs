using FluentValidation;
using IpInformation.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Application layer: MediatR with every handler, the pipeline behaviors
    /// and every FluentValidation validator in this assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
