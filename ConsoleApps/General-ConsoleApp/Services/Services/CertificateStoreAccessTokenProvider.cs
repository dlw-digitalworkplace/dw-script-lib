using Azure.Core;
using Azure.Identity;
using Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using System.Security.Cryptography.X509Certificates;

namespace Services.Services
{
    public class CertificateStoreAccessTokenProvider : IAccessTokenProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private const string CachePrefix = nameof(CertificateStoreAccessTokenProvider);

        public CertificateStoreAccessTokenProvider(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async Task<Core.Classes.AccessToken> GetTokenAsync(string resource)
        {
            var cacheKey = $"{CachePrefix}_{resource}";
            var expirationOffset = TimeSpan.FromSeconds(-5);

            if (!_memoryCache.TryGetValue<Core.Classes.AccessToken>(cacheKey, out var token) ||
                token == null ||
                token.ExpiresOn < DateTimeOffset.UtcNow.Add(expirationOffset))
            {
                using var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);

                var certificates = certStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    _configuration[Globals.Application.ClientCertificateThumbprint],
                    false
                );

                var certificate = certificates.OfType<X509Certificate2>().FirstOrDefault();

                var credentialProvider = new ClientCertificateCredential(
                    _configuration[Globals.Tenant.TenantId],
                    _configuration[Globals.Application.ClientId],
                    certificate
                );

                var azureCredentialToken = await credentialProvider.GetTokenAsync(
                    new TokenRequestContext(new[] { resource })
                );

                token = new Core.Classes.AccessToken(
                    azureCredentialToken.Token,
                    azureCredentialToken.ExpiresOn.Add(expirationOffset)
                );

                _memoryCache.Set(cacheKey, token, token.ExpiresOn);
            }

            return token;
        }
    }
}
