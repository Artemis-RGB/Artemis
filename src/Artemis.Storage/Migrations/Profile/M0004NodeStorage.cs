using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;

namespace Artemis.Storage.Migrations.Profile
{
    /// <summary>
    /// Migrates nodes to be provider-based.
    /// This requires giving them a ProviderId and updating the their namespaces to match the namespace of the new plugin. 
    /// </summary>
    internal class M0004NodeStorage : IProfileMigration
    {
        private readonly ILogger _logger;

        public M0004NodeStorage(ILogger logger)
        {
            _logger = logger;
        }
        
        /// <inheritdoc />
        public int Version => 4;

        /// <inheritdoc />
        public void Migrate(JsonObject configurationJson, JsonObject profileJson)
        {
            MigrateNodeScript(configurationJson["ActivationCondition"]);
            
            JsonArray? folders = profileJson["Folders"]?.AsArray();
            JsonArray? layers = profileJson["Layers"]?.AsArray();

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
            
            JsonArray? layerEffects = profileElement["LayerEffects"]?.AsArray();
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

            JsonArray? properties = propertyGroup["Properties"]?.AsArray();
            JsonArray? propertyGroups = propertyGroup["PropertyGroups"]?.AsArray();
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
            JsonArray? nodes = nodeScript?["Nodes"]?.AsArray();
            if (nodes == null)
                return;

            foreach (JsonNode? jsonNode in nodes)
            {
                if (jsonNode == null)
                    continue;
                
                JsonObject nodeObject = jsonNode.AsObject();
                nodeObject["Storage"] = MigrateNodeStorageJson(nodeObject["Storage"]?.GetValue<string>(), _logger);
            }
        }
        
        internal static string? MigrateNodeStorageJson(string? json, ILogger logger)
        {
            if (string.IsNullOrEmpty(json))
                return json;

            try
            {
                JsonDocument jsonDocument = JsonDocument.Parse(json);
                if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
                    return json;

                JsonObject? jsonObject = JsonObject.Create(jsonDocument.RootElement);
                if (jsonObject == null)
                    return json;

                if (jsonObject["$type"] != null && jsonObject["$values"] != null)
                {
                    JsonArray? values = jsonObject["$values"]?.AsArray();
                    if (values != null)
                    {
                        foreach (JsonNode? jsonNode in values.ToList())
                        {
                            if (jsonNode is JsonObject childObject)
                                M0003SystemTextJson.ConvertToSystemTextJson(childObject);
                        }

                        return values.ToJsonString();
                    }
                }
                else
                {
                    M0003SystemTextJson.ConvertToSystemTextJson(jsonObject);
                }

                return jsonObject.ToJsonString();
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to migrate node storage JSON");
                return json;
            }
        }
    }
}
