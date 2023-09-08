using Microsoft.Graph.Models.ExternalConnectors;

namespace Manage_ExternalGraphConnector.Services
{
    public interface IGraphService
    {
        Task<ExternalConnection> CreateConnectionAsync(string id, string name, string description);
        Task<IEnumerable<ExternalConnection>> GetExistingConnectionsAsync();
        Task DeleteConnectionAsync(string connectionId);
        Task RegisterSchemaAsync(string connectionId, Schema schema);
        Task<Schema> GetSchemaAsync(string connectionId);
    }
}
