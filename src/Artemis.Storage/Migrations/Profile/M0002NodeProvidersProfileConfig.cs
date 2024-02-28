using System.Text.Json.Nodes;

namespace Artemis.Storage.Migrations.Profile;

/// <summary>
/// Migrates nodes to be provider-based on profile configurations as well..
/// This requires giving them a ProviderId and updating the their namespaces to match the namespace of the new plugin. 
/// </summary>
internal class M0002NodeProvidersProfileConfig : IProfileMigration
{
    /// <inheritdoc />
    public int Version => 2;

    /// <inheritdoc />
    public void Migrate(JsonObject configurationJson, JsonObject profileJson)
    {
        MigrateNodeScript(configurationJson["ActivationCondition"]);
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