using System;
using System.Net.Http;
using System.Threading.Tasks;
using IpInformation.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IpInformation.Helpers
{
    public class Ip2c
    {
        public static async Task<Countries> GetIpInfo(string ip)
        {
            string url = $"http://ip2c.org/{ip}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    string[] parts = result.Split(';');

                    if (parts[0] == "1")
                    {
                        return new Countries
                        {
                            ID = 0,
                            TwoLetterCode = parts[1],
                            ThreeLetterCode = parts[2],
                            Name = parts[3],
                            CreatedAt = DateTime.Now
                        };
                    }
                    else
                    {
                        throw new Exception("Failed to get IP information");
                    }
                }
                else
                {
                    throw new Exception("Failed to get IP information");
                }
            }
        }
    }
}
