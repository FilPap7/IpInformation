using IpInformation.Domain.Abstractions;
using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;
using IpInformation.Infrastructure.Persistence;
using IpInformation.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Database";

    /// <summary>
    /// Registers the Infrastructure layer: EF Core on SQL Server (unit of work and repositories).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is missing. Locally, set it with dotnet user-secrets.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IIpAddressRepository, IpAddressRepository>();

        return services;
    }
}
