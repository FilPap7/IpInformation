using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;

namespace DataAccess.Data
{
    public class IpInformationDbContext : DbContext
    {
        public IpInformationDbContext(DbContextOptions<IpInformationDbContext> options) : base(options) { }

        public DbSet<Countries> Countries { get; set; }
        public DbSet<IPAddresses> IPAddresses { get; set; }
    }
}
