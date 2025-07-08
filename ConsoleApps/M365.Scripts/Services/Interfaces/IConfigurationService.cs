using Core.Enums;

namespace Services.Interfaces
{
    public interface IConfigurationService
    {
        Task<List<string>> GetSharePointSiteURLs(DataSource dataSource);
    }
}
