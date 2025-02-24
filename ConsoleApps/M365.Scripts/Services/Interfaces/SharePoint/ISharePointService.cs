using Microsoft.SharePoint.Client;

namespace Services.Interfaces.SharePoint
{
    public interface ISharePointService
    {
        Task<ClientContext> CreateClientContextAsync(string site);
    }
}
