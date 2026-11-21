using IpInformation.Infrastructure.Events;
using IpInformation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.IntegrationTests.Persistence;

internal static class TestDbContext
{
    /// <summary>
    /// A context for tests that need the model or the change tracker but never open a connection.
    /// </summary>
    public static ApplicationDbContext CreateWithoutDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer("Server=unused").Options;
        var dispatcher = new DomainEventDispatcher(new ServiceCollection().BuildServiceProvider());
        return new ApplicationDbContext(options, dispatcher);
    }
}
