using Azure.Core;

namespace Services.Interfaces
{
    public interface IAccessTokenProvider
    {
        Task<AccessToken> GetTokenAsync(string resource);

    }
}
