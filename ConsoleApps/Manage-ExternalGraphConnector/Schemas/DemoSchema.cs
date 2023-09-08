using Microsoft.Graph.Models.ExternalConnectors;

namespace Manage_ExternalGraphConnector.Schemas
{
    public class DemoSchema : SchemaBase
    {
        public override List<Property> GetSchemaProperties()
        {
            var properties = new List<Property>
            {
                new Property { Name = "itemId", Type = PropertyType.String, IsQueryable = true, IsSearchable = true, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "itemName", Type = PropertyType.String, IsQueryable = true, IsSearchable = true, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "itemCategory", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = true }
            };
            return properties;
        }
    }
}
