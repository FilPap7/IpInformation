using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Application layer: MediatR with every handler in this assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
