using Microsoft.Graph.Models;

namespace Manage_ExternalGraphConnector.Models
{
    public class Settings
    {
        public AzureAd AzureAd { get; set; }
    }
    public class AzureAd
    {
        public required string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public IEnumerable<ClientCertificate> ClientCertificates { get; set; }
    }
    public class ClientCertificate
    {
        public string CertificateThumbprint { get; set; }
    }
}
