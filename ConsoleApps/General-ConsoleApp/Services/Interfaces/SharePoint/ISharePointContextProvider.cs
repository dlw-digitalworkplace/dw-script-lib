using Microsoft.SharePoint.Client;

namespace Services.Interfaces.SharePoint
{
    public interface ISharePointContextProvider
    {
        Task<ClientContext> CreateAsync(string url);
    }
}
