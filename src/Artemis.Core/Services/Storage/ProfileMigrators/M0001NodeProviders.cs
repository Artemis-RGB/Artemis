using Newtonsoft.Json.Linq;

namespace Artemis.Core.Services.ProfileMigrators;

/// <summary>
/// Migrates nodes to be provider-based.
/// This requires giving them a ProviderId and updating the their namespaces to match the namespace of the new plugin. 
/// </summary>
internal class M0001NodeProviders : IProfileMigration
{
    /// <inheritdoc />
    public int Version => 1;

    /// <inheritdoc />
    public void Migrate(JObject profileJson)
    {
        JArray? folders = (JArray?) profileJson["Folders"]?["$values"];
        JArray? layers = (JArray?) profileJson["Layers"]?["$values"];

        if (folders != null)
        {
            foreach (JToken folder in folders)
            {
                MigrateProfileElement(folder);
            }
        }

        if (layers != null)
        {
            foreach (JToken layer in layers)
            {
                MigrateProfileElement(layer);
                MigratePropertyGroup(layer["GeneralPropertyGroup"]);
                MigratePropertyGroup(layer["TransformPropertyGroup"]);
                MigratePropertyGroup(layer["LayerBrush"]?["PropertyGroup"]);
            }
        }
    }

    private void MigrateProfileElement(JToken profileElement)
    {
        JArray? layerEffects = (JArray?) profileElement["LayerEffects"]?["$values"];
        if (layerEffects != null)
        {
            foreach (JToken layerEffect in layerEffects)
                MigratePropertyGroup(layerEffect["PropertyGroup"]);
        }

        JToken? displayCondition = profileElement["DisplayCondition"];
        if (displayCondition != null)
            MigrateNodeScript(displayCondition["Script"]);
    }

    private void MigratePropertyGroup(JToken? propertyGroup)
    {
        if (propertyGroup == null || !propertyGroup.HasValues)
            return;

        JArray? properties = (JArray?) propertyGroup["Properties"]?["$values"];
        JArray? propertyGroups = (JArray?) propertyGroup["PropertyGroups"]?["$values"];

        if (properties != null)
        {
            foreach (JToken property in properties)
                MigrateNodeScript(property["DataBinding"]?["NodeScript"]);
        }

        if (propertyGroups != null)
        {
            foreach (JToken childPropertyGroup in propertyGroups)
                MigratePropertyGroup(childPropertyGroup);
        }
    }

    private void MigrateNodeScript(JToken? nodeScript)
    {
        if (nodeScript == null || !nodeScript.HasValues)
            return;

        JArray? nodes = (JArray?) nodeScript["Nodes"]?["$values"];
        if (nodes == null)
            return;

        foreach (JToken node in nodes)
        {
            node["Type"] = node["Type"]?.Value<string>()?.Replace("Artemis.VisualScripting.Nodes", "Artemis.Plugins.Nodes.General.Nodes");
            node["ProviderId"] = "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78";
        }
    }
}