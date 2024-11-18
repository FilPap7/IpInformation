using DataAccess.Entities;

namespace ApiModels.DTO
{
    public class PagedIpAddress
    {
        public IEnumerable<IPAddresses> Data { get; set; } = [];
        public string ContinuationToken { get; set; } = string.Empty;
    }
}
