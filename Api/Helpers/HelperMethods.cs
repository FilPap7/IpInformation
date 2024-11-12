using DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace IpInformation.Helpers
{
    public class HelperMethods
    {
        public static async Task<List<CountryWithIp>> CreateCountryWithIPList (IpInformationDbContext context)
        {
            var ipList = await context.IPAddresses.ToListAsync();
            List<CountryWithIp> countriesWithIp = new List<CountryWithIp>();

            if (ipList != null)
            {
                foreach (var ipInfo in ipList)
                {
                    string ip = ipInfo.IP;
                    var country = await context.Countries.FirstOrDefaultAsync(x => x.ID == ipInfo.CountryId);

                    if (country == null) 
                    {
                        throw new Exception("An Ip with no Country was found in the Database");
                    }

                    var countryWithIp = new CountryWithIp
                    {
                        IP = ipInfo.IP,
                        CountryName = country.Name,
                        TwoLetterCode = country.TwoLetterCode,
                        ThreeLetterCode = country.ThreeLetterCode
                    };

                    countriesWithIp.Add(countryWithIp);
                }
            }

            return countriesWithIp;
        }
    }
}
