using IpInformation.Application.Abstractions.Caching;
using IpInformation.Application.Abstractions.Data;
using IpInformation.Application.Abstractions.Ip2c;
using IpInformation.Domain.Abstractions;
using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;
using IpInformation.Infrastructure.BackgroundJobs;
using IpInformation.Infrastructure.Caching;
using IpInformation.Infrastructure.Events;
using IpInformation.Infrastructure.Ip2c;
using IpInformation.Infrastructure.Persistence;
using IpInformation.Infrastructure.Persistence.Readers;
using IpInformation.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IpInformation.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Database";

    /// <summary>
    /// Registers the Infrastructure layer: EF Core on SQL Server (unit of work, repositories, readers,
    /// domain event dispatch after SaveChanges),
    /// the IP information cache, the IP2C client and the hourly IP refresh job.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is missing. Locally, set it with dotnet user-secrets.");

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IIpAddressRepository, IpAddressRepository>();

        services.AddScoped<ICountryReportReader, CountryReportReader>();

        services.AddSingleton<IIpInformationCache, IpInformationCache>();

        services.AddOptions<Ip2cOptions>()
            .Bind(configuration.GetSection(Ip2cOptions.SectionName))
            .Validate(o => o.BaseAddress is { IsAbsoluteUri: true }, "Ip2c:BaseAddress must be an absolute URI.")
            .Validate(o => o.Timeout > TimeSpan.Zero, "Ip2c:Timeout must be positive.")
            .ValidateOnStart();

        services.AddHttpClient<IIp2cClient, Ip2cClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<Ip2cOptions>>().Value;
            client.BaseAddress = options.BaseAddress;
            client.Timeout = options.Timeout;
        });

        services.AddOptions<IpAddressRefreshOptions>()
            .Bind(configuration.GetSection(IpAddressRefreshOptions.SectionName))
            .Validate(o => o.Interval > TimeSpan.Zero, "IpAddressRefresh:Interval must be positive.")
            .Validate(o => o.BatchSize > 0, "IpAddressRefresh:BatchSize must be positive.")
            .ValidateOnStart();

        services.AddHostedService<IpAddressRefreshJob>();

        return services;
    }
}
