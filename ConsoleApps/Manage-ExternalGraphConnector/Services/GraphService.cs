using Azure.Identity;
using Manage_ExternalGraphConnector.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph.Models.ODataErrors;

namespace Manage_ExternalGraphConnector.Services
{
    public class GraphService : IGraphService
    {
        private readonly GraphServiceClient _graphServiceClient;

        public GraphService(Settings settings)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var tenantId = settings?.AzureAd?.TenantId;
            var clientId = settings?.AzureAd?.ClientId;
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId))
            {
                throw new InvalidOperationException("Client id or Tenant id not configured correctly");
            }

            // Try secret authentication
            var clientSecret = settings?.AzureAd?.ClientSecret;
            if (!string.IsNullOrEmpty(clientSecret))
            {
                var secretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                _graphServiceClient = new GraphServiceClient(secretCredential, scopes);
                return;
            }
            throw new InvalidOperationException("No valid authentication parameters configured");
        }

        public async Task<IEnumerable<ExternalConnection>> GetExistingConnectionsAsync()
        {
            try
            {
                return (await _graphServiceClient.External.Connections.GetAsync()).Value;
            }
            catch (ODataError error)
            {
                throw new Exception(error?.Error?.Message, error?.InnerException);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ExternalConnection> CreateConnectionAsync(string id, string name, string description)
        {
            try
            {
                return await _graphServiceClient.External.Connections.PostAsync(new()
                {
                    Id = id,
                    Name = name,
                    Description = description
                });
            }
            catch (ODataError error)
            {
                throw new Exception(error?.Error?.Message, error?.InnerException);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task DeleteConnectionAsync(string connectionId)
        {
            try
            {
                await _graphServiceClient.External.Connections[connectionId].DeleteAsync();
            }
            catch (ODataError error)
            {
                throw new Exception(error?.Error?.Message, error?.InnerException);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task RegisterSchemaAsync(string connectionId, Schema schema)
        {
            try
            {
                await _graphServiceClient.External.Connections[connectionId].Schema.PatchAsync(schema);
            }
            catch (ODataError error)
            {
                throw new Exception(error?.Error?.Message, error?.InnerException);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Schema> GetSchemaAsync(string connectionId)
        {
            try
            {
                return await _graphServiceClient.External.Connections[connectionId].Schema.GetAsync();
            }
            catch (ODataError error)
            {
                throw new Exception(error?.Error?.Message, error?.InnerException);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
