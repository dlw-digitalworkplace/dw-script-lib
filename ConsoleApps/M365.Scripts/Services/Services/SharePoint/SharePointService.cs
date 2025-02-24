using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using Services.Interfaces.SharePoint;

namespace Services.SharePoint
{
    public class SharePointService : ISharePointService
    {
        private readonly IConfiguration _configuration;
        private readonly ISharePointContextProvider _sharePointContextProvider;

        public SharePointService(ISharePointContextProvider sharePointContextProvider, IConfiguration configuration)
        {
            _configuration = configuration;
            _sharePointContextProvider = sharePointContextProvider;
        }

        // Check connect for client context creation with certificate
        public async Task<ClientContext> CreateClientContextAsync(string siteUrl)
        {
            var context = await _sharePointContextProvider.CreateAsync(siteUrl);
            return context;
        }
    }
}
