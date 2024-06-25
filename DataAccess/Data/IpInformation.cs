using Microsoft.EntityFrameworkCore;
using DataAccess.Entities;

namespace DataAccess.Data
{
    public class IpInformation : DbContext
    {
        public IpInformation(DbContextOptions<IpInformation> options) : base(options) { }

        public DbSet<Countries> Countries { get; set; }
        public DbSet<IPAddresses> IPAddresses { get; set; }
    }
}
