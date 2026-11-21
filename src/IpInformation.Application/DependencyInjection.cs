using System.Reflection;
using FluentValidation;
using IpInformation.Application.Abstractions.Events;
using IpInformation.Application.Behaviors;
using IpInformation.Application.Countries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IpInformation.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Application layer: MediatR with every handler, the pipeline behaviors,
    /// every FluentValidation validator and every domain event handler in this assembly.
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

        AddDomainEventHandlers(services, assembly);

        services.AddScoped<CountryResolver>();

        services.TryAddSingleton(TimeProvider.System);

        return services;
    }

    /// <summary>
    /// Registers every <see cref="IDomainEventHandler{TDomainEvent}"/> in the assembly as scoped,
    /// once per event type it handles.
    /// </summary>
    private static void AddDomainEventHandlers(IServiceCollection services, Assembly assembly)
    {
        var registrations =
            from type in assembly.GetTypes()
            where type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
            from handlerInterface in type.GetInterfaces()
            where handlerInterface.IsGenericType
                && handlerInterface.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
            select ServiceDescriptor.Scoped(handlerInterface, type);

        services.TryAddEnumerable(registrations);
    }
}
