using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog.Core;

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
            JsonArray? folders = (JsonArray?) profileJson["Folders"]?["values"];
            JsonArray? layers = (JsonArray?) profileJson["Layers"]?["values"];

            if (folders != null)
            {
                foreach (JsonValue folder in folders)
                    MigrateProfileElement(folder);
            }

            if (layers != null)
            {
                foreach (JsonValue layer in layers)
                {
                    MigrateProfileElement(layer);
                    MigratePropertyGroup(layer["GeneralPropertyGroup"]);
                    MigratePropertyGroup(layer["TransformPropertyGroup"]);
                    MigratePropertyGroup(layer["LayerBrush"]?["PropertyGroup"]);
                }
            }
        }

        private void MigrateProfileElement(JsonNode profileElement)
        {
            JsonArray? layerEffects = (JsonArray?) profileElement["LayerEffects"]?["values"];
            if (layerEffects != null)
            {
                foreach (JsonValue layerEffect in layerEffects)
                    MigratePropertyGroup(layerEffect["PropertyGroup"]);
            }

            JsonNode? displayCondition = profileElement["DisplayCondition"];
            if (displayCondition != null)
                MigrateNodeScript(displayCondition["Script"]);
        }

        private void MigratePropertyGroup(JsonNode? propertyGroup)
        {
            if (propertyGroup == null)
                return;

            JsonArray? properties = (JsonArray?) propertyGroup["Properties"]?["values"];
            JsonArray? propertyGroups = (JsonArray?) propertyGroup["PropertyGroups"]?["values"];

            if (properties != null)
            {
                foreach (JsonValue property in properties)
                    MigrateNodeScript(property["DataBinding"]?["NodeScript"]);
            }

            if (propertyGroups != null)
            {
                foreach (JsonValue childPropertyGroup in propertyGroups)
                    MigratePropertyGroup(childPropertyGroup);
            }
        }

        private void MigrateNodeScript(JsonNode? nodeScript)
        {
            if (nodeScript == null)
                return;

            JsonArray? nodes = nodeScript["Nodes"]?.AsArray();
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
