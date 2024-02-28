using System.Text.Json.Nodes;

namespace Artemis.Storage.Migrations.Profile
{
    /// <summary>
    /// Migrates nodes to be provider-based.
    /// This requires giving them a ProviderId and updating the their namespaces to match the namespace of the new plugin. 
    /// </summary>
    internal class M0001NodeProviders : IProfileMigration
    {
        /// <inheritdoc />
        public int Version => 1;

        /// <inheritdoc />
        public void Migrate(JsonObject configurationJson, JsonObject profileJson)
        {
            JsonArray? folders = profileJson["Folders"]?["$values"]?.AsArray();
            JsonArray? layers = profileJson["Layers"]?["$values"]?.AsArray();

            if (folders != null)
            {
                foreach (JsonNode? folder in folders)
                    MigrateProfileElement(folder);
            }

            if (layers != null)
            {
                foreach (JsonNode? layer in layers)
                {
                    MigrateProfileElement(layer);
                    MigratePropertyGroup(layer?["GeneralPropertyGroup"]);
                    MigratePropertyGroup(layer?["TransformPropertyGroup"]);
                    MigratePropertyGroup(layer?["LayerBrush"]?["PropertyGroup"]);
                }
            }
        }

        private void MigrateProfileElement(JsonNode? profileElement)
        {
            if (profileElement == null)
                return;
            
            JsonArray? layerEffects = profileElement["LayerEffects"]?["$values"]?.AsArray();
            if (layerEffects != null)
            {
                foreach (JsonNode? layerEffect in layerEffects)
                    MigratePropertyGroup(layerEffect?["PropertyGroup"]);
            }

            JsonNode? displayCondition = profileElement["DisplayCondition"];
            if (displayCondition != null)
                MigrateNodeScript(displayCondition["Script"]);
        }

        private void MigratePropertyGroup(JsonNode? propertyGroup)
        {
            if (propertyGroup == null)
                return;

            JsonArray? properties = propertyGroup["Properties"]?["$values"]?.AsArray();
            JsonArray? propertyGroups = propertyGroup["PropertyGroups"]?["$values"]?.AsArray();
            if (properties != null)
            {
                foreach (JsonNode? property in properties)
                    MigrateNodeScript(property?["DataBinding"]?["NodeScript"]);
            }

            if (propertyGroups != null)
            {
                foreach (JsonNode? childPropertyGroup in propertyGroups)
                    MigratePropertyGroup(childPropertyGroup);
            }
        }

        private void MigrateNodeScript(JsonNode? nodeScript)
        {
            JsonArray? nodes = nodeScript?["Nodes"]?["$values"]?.AsArray();
            if (nodes == null)
                return;

            foreach (JsonNode? jsonNode in nodes)
            {
                if (jsonNode == null)
                    continue;
                JsonObject nodeObject = jsonNode.AsObject();
                nodeObject["Type"] = nodeObject["Type"]?.GetValue<string>().Replace("Artemis.VisualScripting.Nodes", "Artemis.Plugins.Nodes.General.Nodes");
                nodeObject["ProviderId"] = "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78";
            }
        }
    }
}
