using Microsoft.Graph.Models.ExternalConnectors;

namespace Manage_ExternalGraphConnector.Schemas
{
    public abstract class SchemaBase
    {
        public abstract List<Property> GetSchemaProperties();
    }
}
