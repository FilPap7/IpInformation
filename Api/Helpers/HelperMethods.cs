using Common.Cache;
using DataAccess.Data;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Net;

namespace IpInformation.Helpers
{
    public class HelperMethods
    {
        public static async Task<Countries> FindSingleIp(IpInformationDbContext dbContext, ICacheService cacheContext, string Ip)
        {
            string ipCacheKey = $"IpInfo-{Ip}";
            string countryCacheKey = $"CountryInfo-{Ip}";

            var ipAddress = cacheContext.GetData<IPAddresses>(ipCacheKey);
            var country = cacheContext.GetData<Countries>(countryCacheKey);


            if (ipAddress == null)
            {
                ipAddress = await dbContext.IPAddresses.FirstOrDefaultAsync(x => x.IP == Ip);

                if (ipAddress == null)
                {
                    // Get the Country from Ip2c Corresponding to the ID
                    if (country == null)
                    {
                        country = await Ip2c.GetIpInfo(Ip);

                        //check if country already exists in the DB
                        var tempCountry = await dbContext.Countries.FirstOrDefaultAsync(x => x.TwoLetterCode == country.TwoLetterCode);

                        if (tempCountry != null)
                        {
                            country = tempCountry;
                        }
                        else
                        {
                            // Add the Country to the Countries table
                            dbContext.Countries.Add(country);
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    ipAddress = new IPAddresses
                    {
                        IP = Ip,
                        CountryId = country.ID,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // Add the IP to the IPAddresses table
                    dbContext.IPAddresses.Add(ipAddress);
                    await dbContext.SaveChangesAsync();
                }
            }

            if (country == null)
            {
                country = await dbContext.Countries.FirstOrDefaultAsync(x => x.ID == ipAddress.CountryId);

                // If the Country does not exist in the Countries table, add it to the DB and the Cache
                if (country == null)
                {
                    // Get the Country from Ip2c Corresponding to the ID
                    country = await Ip2c.GetIpInfo(Ip);

                    // Add the Country to the Countries table
                    dbContext.Countries.Add(country);
                    await dbContext.SaveChangesAsync();
                }
            }

            // Ip and Country have values now so Update the cache
            cacheContext.SetData(ipCacheKey, ipAddress, TimeSpan.FromMinutes(5));
            cacheContext.SetData(countryCacheKey, country, TimeSpan.FromMinutes(5));

            return country;
        }

        public static async Task<List<CountryWithIp>> CreateCountryWithIPList (IpInformationDbContext dbContext)
        {
            var ipList = await dbContext.IPAddresses.ToListAsync();
            List<CountryWithIp> countriesWithIp = new List<CountryWithIp>();

            if (ipList != null)
            {
                foreach (var ipInfo in ipList)
                {
                    string ip = ipInfo.IP;
                    var country = await dbContext.Countries.FirstOrDefaultAsync(x => x.ID == ipInfo.CountryId);

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

        public static async Task<bool> UpdateDatabaseAsync(IpInformationDbContext dbContext, ICacheService cacheContext)
        {
            var ipList = await dbContext.IPAddresses.ToListAsync();

            if (ipList != null)
            {
                foreach (var ipInfo in ipList)
                {
                    string ip = ipInfo.IP;

                    string ipCacheKey = $"IpInfo-{ip}";
                    string countryCacheKey = $"CountryInfo-{ip}";

                    // Get the Country from Ip2c Corresponding to the ID
                    var country = await Ip2c.GetIpInfo(ip);

                    if(country == null)
                    {
                        throw new Exception($"Ip2c failed to return a Country for the IP: {ip}");
                    }                    
                    
                    var chechIfExists = await dbContext.Countries.FirstOrDefaultAsync(x => x.Name == country.Name);
                    if(chechIfExists == null)
                    {
                        dbContext.Add(country);
                    }

                    ipInfo.CountryId = country.ID;
                        
                    await dbContext.SaveChangesAsync();

                    // Ip and Country have values now so Update the cache
                    cacheContext.SetData(ipCacheKey, ip, TimeSpan.FromMinutes(5));
                    cacheContext.SetData(countryCacheKey, country, TimeSpan.FromMinutes(5));
                }
            }

            return true;
        }
    }
}
