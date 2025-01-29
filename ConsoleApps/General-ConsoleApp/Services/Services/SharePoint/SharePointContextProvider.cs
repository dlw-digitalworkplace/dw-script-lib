using System.Net;
using Microsoft.SharePoint.Client;
using Services.Interfaces;
using Services.Interfaces.SharePoint;

namespace Services.Services.SharePoint
{
    public class SharePointContextProvider : ISharePointContextProvider
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        public SharePointContextProvider(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        public async Task<ClientContext> CreateAsync(string url)
        {
            var targetUri = new Uri(url).GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            var resource = $"{targetUri}/.default";

            var spContext = new ClientContext(url);
            var accessToken = await _accessTokenProvider.GetTokenAsync(resource);

            spContext.ExecutingWebRequest += (sender, e) =>
            {
                e.WebRequestExecutor.WebRequest.Headers[HttpRequestHeader.Authorization] = $"Bearer {accessToken.Token}";
                e.WebRequestExecutor.WebRequest.UserAgent = "NONISV|delaware|TheFactory/1.0";
            };

            return spContext;
        }
    }
}
