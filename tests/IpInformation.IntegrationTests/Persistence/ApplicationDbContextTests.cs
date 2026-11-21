namespace IpInformation.IntegrationTests.Persistence;

public class ApplicationDbContextTests
{
    // Saving with events against a real database (save -> dispatch -> cache) is covered once
    // integration tests run on SQL Server via Testcontainers.

    [Fact]
    public void SaveChanges_WhenCalledSynchronously_ThrowsBecauseItCannotDispatchDomainEvents()
    {
        using var context = TestDbContext.CreateWithoutDatabase();

        Assert.Throws<NotSupportedException>(() => context.SaveChanges());
    }
}
